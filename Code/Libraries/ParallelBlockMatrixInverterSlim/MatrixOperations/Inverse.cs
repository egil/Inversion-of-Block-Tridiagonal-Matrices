using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.Enumerators;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.OperationResults;
using TiledMatrixInversion.Resources;

namespace TiledMatrixInversion.ParallelBlockMatrixInverterSlim.MatrixOperations
{
    public class Inverse<T> : IProducer<Action>
    {
        private readonly PipelinedOperationEnumerator<OperationEnumerator<AbstractOperation<OpType>>, AbstractOperation<OpType>> _gen;
        private readonly OperationResult<T> _inputa;
        private readonly OperationResult<T> _result;

        /// <summary>
        /// Let i, j be an index in _fstatus.
        /// 
        /// then _fstatus[i, j] is
        /// 
        ///     0 : initially
        ///     k >= 0 : at calculation step k
        ///     -1 : when tile[i, j] of f = l^-1*r has been calculated
        ///          when we are done with tile[i, j], we set _gstatus[i, j] to 0
        ///          to indicate that the second calculation can commence.
        /// </summary>
        private readonly int[][] _fstatus;
        /// <summary>
        /// Let i, j be an index in _gstatus.
        /// 
        /// then _gstatus[i, j] is
        /// 
        ///     -2 : initially
        ///     -1 : when tile[i, j] of g = u^-1*f has been calculated
        ///     k >= 0 : at calculation step k
        /// 
        ///     when tile[i, j] of g = u^-1*f has been calculated and saved in the result.Data[i, j],
        ///     result[i, j] is set to indicate the completion of the calculation.
        /// </summary>
        private readonly int[][] _gstatus;
        private readonly int N;
        private readonly int M;
        private bool _hasCompletedInit;

        public Inverse(OperationResult<T> a, out OperationResult<T> result)
        {
            Debug.Assert(a.Rows == a.Columns, "Can not invert non square matrix.");

            N = a.Columns;
            M = a.Rows;

            _fstatus = Helpers.Init<int>(a.Rows + 1, a.Columns + 1);
            _gstatus = Helpers.Init<int>(a.Rows + 1, a.Columns + 1);

            for (int i = 0; i < M + 1; i++)
            {
                for (int j = 0; j < N + 1; j++)
                {
                    // initialize upper triangle to zero and lower triangle and diagonal to -2
                    _gstatus[i][j] = j > i ? 0 : -2;
                }
            }

            _inputa = a;
            _result = result = new OperationResult<T>(a.Rows, a.Columns);

            _gen = new PipelinedOperationEnumerator<OperationEnumerator<AbstractOperation<OpType>>, AbstractOperation<OpType>>(
                AbstractOperationGenerator(M), Environment.ProcessorCount /* maxmium amount of lines to generate at a time */);
        }

        private bool TryInit()
        {
            if (_inputa.Data == null)
                return false;

            if (_inputa.Data.Any(x => x == null))
                return false;

            for (int i = 1; i <= _inputa.Rows; i++)
            {
                for (int j = 1; j <= _inputa.Columns; j++)
                {
                    _result.Data[i, j] = new Matrix<T>(_inputa.Data[i, j].Rows, _inputa.Data[i, j].Columns, i == j /* init as identity matrix */);
                }
            }
            return _hasCompletedInit = true;
        }

        #region Implementation of IProducer<Action>

        public bool IsCompleted { get { return _gen.Completed; } }

        public bool TryGetNext(out Action action)
        {
            action = null;
            if (_hasCompletedInit || TryInit())
            {
                var op = _gen.Find(IsRunnable);
                action = GenerateAction(op);
            }
            return action != null;
        }

        #endregion

        #region Helper functions

        private bool IsRunnable(AbstractOperation<OpType> op)
        {
            // not done varifying this...
            switch (op.OP)
            {
                case OpType.F:
                    // Figure 9, line 3, left side: L^-1 is stored in _inputa.
                    // _fstatus[op.I][op.J] contains the time step for R
                    return _inputa[op.I, op.I] && _fstatus[op.I][op.J] == op.K - 1;

                case OpType.R:
                    // note that I and J are exchanged compared to figure 9 in Abstract Row Action Generator
                    return _fstatus[op.I][op.J] == op.K && _inputa[op.I, op.K + op.J] && _fstatus[op.K + op.J][op.J] == -1;

                case OpType.G:
                    return _inputa[op.I, op.I] && _gstatus[op.I][op.J] == op.K - 1;

                case OpType.H:
                    return _gstatus[op.I][op.J] == op.K && _inputa[op.I, N - op.K] && _gstatus[N - op.K][op.J] == -1;

                default:
                    Debug.Fail("IsRunnable(AbstractOperation<OpType> op): Should not happen!");
                    return false;
            }
        }

        private Action GenerateAction(AbstractOperation<OpType> op)
        {
            if (op == null)
                return null;

            switch (op.OP)
            {
                case OpType.F:
                    // calculates line 3 and 8 in figure 9, left side.
                    return () =>
                               {
                                   _result.Data[op.I, op.J] =
                                       _inputa.Data[op.I, op.I].GetLowerTriangleWithFixedDiagonal().Inverse() *
                                       _result.Data[op.I, op.J];

                                   _fstatus[op.I][op.J] = -1;

                                   if (op.I == M)
                                   {
                                       // done with this tile
                                       _gstatus[op.I][op.J] = 0;
                                   }
                               };
                // calculates line 5 in figure 9, left side.
                case OpType.R:
                    return () =>
                               {
                                   _result.Data[op.I, op.J] = _result.Data[op.I, op.J] -
                                                              _inputa.Data[op.I, op.K + op.J] *
                                                              _result.Data[op.K + op.J, op.J];

                                   _fstatus[op.I][op.J] = op.K + 1;

                                   // when op.I == M, we are completly done with the previous R,
                                   // and can update _gstatus to zero and release it to further calculations
                                   if (op.I == M)
                                   {
                                       _gstatus[op.K + op.J][op.J] = 0;
                                   }
                               };

                // calculates line 3 and 8 in figure 9, right side.
                case OpType.G:
                    return () =>
                               {
                                   _result.Data[op.I, op.J] = _inputa.Data[op.I, op.I].GetUpperTriangle().Inverse() *
                                                              _result.Data[op.I, op.J];

                                   _gstatus[op.I][op.J] = -1;

                                   if (op.I == 1)
                                   {
                                       // done with this tile
                                       _result[op.I, op.J] = true;
                                   }
                               };

                // calculates line 5 in figure 9, right side.
                case OpType.H:
                    return () =>
                               {
                                   _result.Data[op.I, op.J] = _result.Data[op.I, op.J] -
                                                              _inputa.Data[op.I, M - (op.K)] *
                                                              _result.Data[M - (op.K), op.J];

                                   _gstatus[op.I][op.J] = op.K + 1;

                                   if (op.I == 1)
                                   {
                                       // we are done with this tile now, and it is free to be worked on by other operations, inplace.
                                       _result[M - (op.K), op.J] = true;
                                   }
                               };

                default:
                    Debug.Fail("IsRunnable(AbstractOperation<OpType> op): Should not happen!");
                    return null;

            }
        }

        #endregion

        #region Abstract operation representation

        private static IEnumerable<OperationEnumerator<AbstractOperation<OpType>>> AbstractOperationGenerator(int N)
        {
            for (int i = 1; i <= N; i++)
            {
                yield return new OperationEnumerator<AbstractOperation<OpType>>(F_ColumnAbstractActionGenerator(i, N), Constants.MAX_QUEUE_LENGTH);
            }

            for (int i = 1; i <= N; i++)
            {
                yield return new OperationEnumerator<AbstractOperation<OpType>>(G_ColumnAbstractActionGenerator(i, N), Constants.MAX_QUEUE_LENGTH);
            }
        }
        
        // A generator for the schedule of Inverse step 1. It is the 
        // same schedule as that of minus matrix inverse matrix multiply, except
        // that $\color{OliveGreen}i$ and $\color{OliveGreen}j$ are interchanged
        // and some elements are skipped as they do not contribute to the result.
        private static IEnumerable<AbstractOperation<OpType>> F_ColumnAbstractActionGenerator(int j, int N)
        {
            // Sweeps starting at elements above the diagonal are skipped
            // by starting at the step where sweep $\color{OliveGreen}j$ starts.
            for (int step = 2 * (j - 1) + 1; step <= 2 * (N - 1) + 1; step++)
            {
                // Skip to sweep $\color{OliveGreen}j$
                int sweep = System.Math.Max(j - 1, step - N);

                // For the $\color{OliveGreen}N$ steps, $\color{OliveGreen}j-1$ rows
                // are skipped.
                for (int i = System.Math.Min(step - (j - 1), N); i >= step / 2 + 1; i--)
                {
                    sweep++;
                    if (i == sweep)
                    {
                        // The timestep (third parameter) is moved to begin at 1.
                        yield return new AbstractOperation<OpType>(i, j, sweep - (j - 1), OpType.F);
                    }
                    else
                    {
                        // The timestep (third parameter) is moved to begin at 1.                        
                        yield return new AbstractOperation<OpType>(i, j, sweep - (j - 1) - 1, OpType.R);
                    }
                }
            }
        }
        
        // A generator for the schedule of Inverse step 2. It is identical to the scheduler
        // of minus matrix inverse matrix multiply step 2, with $\color{OliveGreen}i$ and $\color{OliveGreen}j$
        // interchanged.
        private static IEnumerable<AbstractOperation<OpType>> G_ColumnAbstractActionGenerator(int j, int N)
        {
            for (int step = 1; step <= 2 * (N - 1) + 1; step++)
            {
                int sweep = System.Math.Max(0, step - N);
                for (int i = System.Math.Max(N - (step - 1), 1); i <= N - (step / 2); i++)
                {
                    sweep++;
                    if (i == N - (sweep - 1))
                    {
                        yield return new AbstractOperation<OpType>(i, j, sweep, OpType.G);
                    }
                    else
                    {
                        yield return new AbstractOperation<OpType>(i, j, sweep - 1, OpType.H);
                    }
                }
            }
        }

        enum OpType
        {
            F,
            R,
            G,
            /// <summary>
            /// H refers to F in the right side of figure 9
            /// </summary>
            H
        }

        #endregion
    }
}