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
    public class MinusMatrixInverseMatrixMultiply<T> : IProducer<Action>
    {
        #region Fields

        private readonly PipelinedOperationEnumerator<OperationEnumerator<AbstractOperation<OpType>>, AbstractOperation<OpType>> _gen;
        private readonly OperationResult<T> _inputa;
        private readonly OperationResult<T> _inputb;
        private readonly OperationResult<T> _result;


        /// <summary>
        /// Let i, j be an index in _bstatus.
        /// 
        /// then _bstatus[i, j] is
        /// 
        ///     0 : initially
        ///     k >= 0 : at calculation step k
        ///     -1 : when tile[i, j] of b = a*u^-1 has been calculated
        ///          when we are done with tile[i, j], we set _cstatus[i, j] to 0
        ///          to indicate that the second calculation can commence.
        /// </summary>
        private readonly int[][] _bstatus;
        /// <summary>
        /// Let i, j be an index in _cstatus.
        /// 
        /// then _cstatus[i, j] is
        /// 
        ///     -2 : initially
        ///     -1 : when tile[i, j] of c = b*l^-1 has been calculated
        ///     k >= 0 : at calculation step k
        /// 
        ///     when tile[i, j] of c = b*l^-1 has been calculated and saved in the result.Data[i, j],
        ///     result[i, j] is set to indicate the completion of the calculation.
        /// </summary>
        private readonly int[][] _cstatus;
        private readonly int N;
        private bool _hasCompletedInit;

        #endregion

        public MinusMatrixInverseMatrixMultiply(OperationResult<T> a, OperationResult<T> b, out OperationResult<T> result)
        {
            N = b.Columns;            
            _bstatus = Helpers.Init<int>(a.Rows + 1, b.Columns + 1);
            _cstatus = Helpers.Init<int>(a.Rows + 1, b.Columns + 1);
            for (int i = 0; i < a.Rows + 1; i++)
            {
                for (int j = 0; j < b.Columns + 1; j++)
                {
                    _cstatus[i][j] = -2;
                }
            }
            _inputa = a;
            _inputb = b;

            _result = result = new OperationResult<T>(a.Rows, a.Columns, true /* layzy init */);

            _gen = new PipelinedOperationEnumerator<OperationEnumerator<AbstractOperation<OpType>>, AbstractOperation<OpType>>(
                AbstractOperationGenerator(a.Rows, b.Columns),
                Environment.ProcessorCount /* maxmium amount of lines to generate at a time */);
        }

        private bool TryInit()
        {
            if (_inputa.Data == null)
                return false;

            if (_inputa.Data.Any(x => x == null))
                return false;
            
            // input a.Data is cloned because we are not doing inplace operations on a
            _result.Data = _inputa.Data.Clone();
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
            switch (op.OP)
            {
                case OpType.A:
                    return _bstatus[op.I][op.J] == op.K - 1 && _bstatus[op.I][op.K] == -1 && _inputb[op.K, op.J];

                case OpType.Bb:
                    return _bstatus[op.I][op.K] == op.K - 1 && _inputb[op.K, op.K];

                case OpType.Bc:
                    return _cstatus[op.I][op.J] == op.K - 1 && _cstatus[op.I][N - (op.K - 1)] == -1 &&
                           _inputb[N - (op.K - 1), op.J];

                case OpType.C:
                    return _cstatus[op.I][op.J] == op.K - 1 && _inputb[op.J, op.J];

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
                case OpType.A:
                    // calculates line 5 in figure 6, left side.
                    return () =>
                               {
                                   _result.Data[op.I, op.J] = _result.Data[op.I, op.J] -
                                                              _result.Data[op.I, op.K] *
                                                              _inputb.Data[op.K, op.J];

                                   _bstatus[op.I][op.J] = op.K;
                               };
                // calculates line 3 and 8 in figure 6, left side.
                case OpType.Bb:
                    return () =>
                               {
                                   _result.Data[op.I, op.K] = _result.Data[op.I, op.K] * _inputb.Data[op.K, op.K].GetUpperTriangle().Inverse();
                                   _bstatus[op.I][op.J] = -1;

                                   // when op.J > 1, we are completly done with the previous Bb,
                                   // and can update cstatus to zero and release it to further calculations
                                   if (op.J > 1)
                                   {
                                       _cstatus[op.I][op.J - 1] = 0;
                                   }

                                   if (op.J == N)
                                   {
                                       _cstatus[op.I][op.J] = 0;
                                   }
                               };
                // calculates line 5 in figure 6, right side.
                case OpType.Bc:
                    return () =>
                               {
                                   _result.Data[op.I, op.J] = _result.Data[op.I, op.J] +
                                                              _result.Data[op.I, N - (op.K - 1)] *
                                                              _inputb.Data[N - (op.K - 1), op.J];

                                   _cstatus[op.I][op.J] = op.K;

                               };
                // calculates line 3 and 8 in figure 6, right side.
                case OpType.C:
                    return () =>
                               {

                                   _result.Data[op.I, op.J] = -_result.Data[op.I, op.J] *
                                                              _inputb.Data[op.J, op.J].GetLowerTriangleWithFixedDiagonal().Inverse();

                                   _cstatus[op.I][op.J] = -1;

                                   if (op.J < N)
                                   {
                                       // we are done with this tile now, and it is free to be worked on by other operations, inplace.
                                       _result[op.I, op.J + 1] = true;
                                   }

                                   if (op.J == 1)
                                   {
                                       // we are done with this tile now, and it is free to be worked on by other operations, inplace.
                                       _result[op.I, op.J] = true;
                                   }
                               };

                default:
                    Debug.Fail("IsRunnable(AbstractOperation<OpType> op): Should not happen!");
                    return null;

            }
        }

        #endregion

        #region Abstract operation representation

        private static IEnumerable<OperationEnumerator<AbstractOperation<OpType>>> AbstractOperationGenerator(int M, int N)
        {
            for (int i = 1; i <= M; i++)
            {
                yield return new OperationEnumerator<AbstractOperation<OpType>>(B_RowActionGenerator(i, N), Constants.MAX_QUEUE_LENGTH);
            }

            for (int i = 1; i <= M; i++)
            {
                yield return new OperationEnumerator<AbstractOperation<OpType>>(C_RowActionGenerator(i, N), Constants.MAX_QUEUE_LENGTH);
            }
        }

        // Implementation of PIM algorithm from \cite{stig}, figure 7.
        private static IEnumerable<AbstractOperation<OpType>> B_RowActionGenerator(int i, int N)
        {
            for (int step = 1; step <= 2 * (N - 1) + 1; step++)
            {
                // The first $\color{OliveGreen}N$ steps, sweep 1 is the first.
                // From then on one sweep is completed at every step.
                int sweep = System.Math.Max(0, step - N);
                for (int j = System.Math.Min(step, N); j >= step / 2 + 1; j--)
                {
                    sweep++;
                    if (j == sweep)
                    {
                        // First operation is $\color{OliveGreen}B_{ij}$
                        yield return new AbstractOperation<OpType>(i, j, j, OpType.Bb);
                    }
                    else
                    {
                        // The rest are $\color{OliveGreen}A_{ij}^{(sweep)}$ operations
                        yield return new AbstractOperation<OpType>(i, j, sweep, OpType.A);
                    }
                }
            }
        }

        // A PIM based on \cite{stig} figure 7, but moving right to left. 
        // See figure \vref{fig:mmimm-step2}.
        private static IEnumerable<AbstractOperation<OpType>> C_RowActionGenerator(int i, int N)
        {
            for (int step = 1; step <= 2 * (N - 1) + 1; step++)
            {
                int sweep = System.Math.Max(0, step - N);

                // As seen in \cite{stig} figure 7, the first $\color{OliveGreen}N$ steps the $\color{OliveGreen}j$
                // index start at $\color{OliveGreen}step$, the rest of the steps $\color{OliveGreen}j$ starts
                // at $\color{OliveGreen}N$. It then counts down to, as seen in the figure, $\color{OliveGreen}sweep/2+1$.
                for (int j = System.Math.Max(N - (step - 1), 1); j <= N - (step / 2); j++)
                {
                    sweep++;
                    if (j == N - (sweep - 1))
                    {
                        yield return new AbstractOperation<OpType>(i, j, sweep, OpType.C);
                    }
                    else
                    {
                        yield return new AbstractOperation<OpType>(i, j, sweep, OpType.Bc);
                    }
                }
            }
        }

        enum OpType
        {
            A,
            /// <summary>
            /// This OpType describes the operation performed on line 3 and 8 in figure 6, the calculation of
            /// b = a*u^-1
            /// </summary>
            Bb,
            /// <summary>
            /// This OpType describes the operation performed on line 5 in figure 6, the calculation of
            /// c = b*l^-1
            /// </summary>
            Bc,
            C
        }

        #endregion

        
    }
}