using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.ParallelBlockMatrixInverter.Enumerators;
using TiledMatrixInversion.ParallelBlockMatrixInverter.OperationResults;
using TiledMatrixInversion.Resources;

namespace TiledMatrixInversion.ParallelBlockMatrixInverter.MatrixOperations
{
    public class Inverse<T> : IProducer<Action>
    {
        private readonly PipelinedOperationEnumerator<UnsortedOperationEnumerator<AbstractOperation<OpType>>, AbstractOperation<OpType>> _gen;
        private readonly object _lock = new object();
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
        private readonly int[,] _fstatus;
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
        private readonly int[,] _gstatus;
        private readonly int N;
        private readonly int M;

        public Inverse(OperationResult<T> a, out OperationResult<T> result)
        {
            Debug.Assert(a.Data.Rows == a.Data.Columns, "Can not invert non square matrix.");

            N = a.Data.Columns;
            M = a.Data.Rows;

            _fstatus = new int[a.Data.Rows + 1, a.Data.Columns + 1];
            _gstatus = new int[a.Data.Rows + 1, a.Data.Columns + 1];
            
            // TODO: INIT _GSTATUS UPPER TO something different than -2
            for (int i = 0; i < _gstatus.GetLength(0); i++)
            {
                for (int j = 0; j < _gstatus.GetLength(1); j++)
                {
                    // initialize upper triangle to zero and lower triangle and diagonal to -2
                    _gstatus[i, j] = j > i ? 0 : -2;
                }
            }

            _inputa = a;
            _result = result = new OperationResult<T>(a.Rows, a.Columns);
            for (int i = 1; i <= a.Rows; i++)
            {
                for (int j = 1; j <= a.Columns; j++)
                {
                    if (i == j)
                        _result.Data[i, j] = new Matrix<T>(a.Data[i, j].Rows, a.Data[i, j].Columns, true /* init as identity matrix */);
                    else
                        _result.Data[i, j] = new Matrix<T>(a.Data[i, j].Rows, a.Data[i, j].Columns, false /* init as identity matrix */);
                }
            }
            _gen = new PipelinedOperationEnumerator
                <UnsortedOperationEnumerator<AbstractOperation<OpType>>, AbstractOperation<OpType>>(
                RowActionGenerator(M).GetEnumerator(),
                Environment.ProcessorCount /* maxmium amount of lines to generate at a time */);   
        }

        #region Implementation of IProducer<Action>

        public bool IsCompleted { get { return _gen.Completed; } }


        public bool TryGetNext(out Action action)
        {
            lock (_lock)
            {
                var op = _gen.Find(IsRunnable);
                action = GenerateAction(op);
                return action != null;
            }
        }

        #endregion

        #region Helper functions

        private bool IsRunnable(AbstractOperation<OpType> op)
        {
            // not done varifying this...
            switch (op.OP)
            {
                //case OpType.Init:
                //    return _r != null;

                case OpType.F:
                    // Figure 9, line 3, left side: L^-1 is stored in _inputa.
                    // _fstatus[op.I, op.J] contains the time step for R
                    return _inputa[op.I, op.I] && _fstatus[op.I, op.J] == op.K;

                case OpType.R:
                    // note that I and J are exchanged compared to figure 9 in Abstract Row Action Generator
                    return _fstatus[op.I, op.J] == op.K && _inputa[op.I, op.K + op.J] && _fstatus[op.K + op.J, op.J] == -1;

                case OpType.G:
                    return _inputa[op.I, op.I] && _gstatus[op.I, op.J] == op.K;

                case OpType.H:
                    return _gstatus[op.I, op.J] == op.K && _inputa[op.I, N - op.K] && _gstatus[N - op.K, op.J] == -1;

                default:
                    Debug.Fail("IsRunnable(AbstractOperation<OpType> op): Should not happen!");
                    return false;
            }
        }

        private Action GenerateAction(AbstractOperation<OpType> op)
        {
            if (op == null)
                return null;

            //Debug.WriteLine(Thread.CurrentThread.Name + " [ Inverse: " + op + "]");
            switch (op.OP)
            {
                case OpType.F:
                    // calculates line 3 and 8 in figure 9, left side.
                    return () =>
                    {
                        _result.Data[op.I, op.J] =
                            _inputa.Data[op.I, op.I].GetLowerTriangleWithFixedDiagonal().Inverse()*
                            _result.Data[op.I, op.J];

                        _fstatus[op.I, op.J] = -1;

                        if (op.I == M)
                        {
                            // done with this tile
                            _gstatus[op.I, op.J] = 0;
                        }
                    };
                // calculates line 5 in figure 9, left side.
                case OpType.R:
                    return () =>
                    {
                        _result.Data[op.I, op.J] = _result.Data[op.I, op.J] -
                                                   _inputa.Data[op.I, op.K + op.J]*_result.Data[op.K + op.J, op.J];
                        _fstatus[op.I, op.J] = op.K + 1;

                        // when op.I == M, we are completly done with the previous R,
                        // and can update _gstatus to zero and release it to further calculations
                        if (op.I == M)
                        {
                            _gstatus[op.K + op.J, op.J] = 0;
                        }
                    };

                // calculates line 3 and 8 in figure 9, right side.
                case OpType.G:
                    return () =>
                    {
                        _result.Data[op.I, op.J] = _inputa.Data[op.I, op.I].GetUpperTriangle().Inverse()*
                                                   _result.Data[op.I, op.J];

                        _gstatus[op.I, op.J] = -1;

                        if (op.I == 1)
                        {
                            // done with this tile
                            _result[op.I, op.J] = true;

                            //if(op.J == M)
                            //    _result.Completed = true;
                        }

                    };
                
                // calculates line 5 in figure 9, right side.
                case OpType.H:
                    return () =>
                    {
                        _result.Data[op.I, op.J] = _result.Data[op.I, op.J] -
                                                   _inputa.Data[op.I, M - (op.K)]*
                                                   _result.Data[M - (op.K), op.J];

                        _gstatus[op.I, op.J] = op.K+1;

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

        private static IEnumerable<UnsortedOperationEnumerator<AbstractOperation<OpType>>> RowActionGenerator(int M)
        {            
            for (int i = 1; i <= M; i++)
            {
                yield return new UnsortedOperationEnumerator<AbstractOperation<OpType>>(F_RowActionGenerator(i, M).GetEnumerator(), Constants.MAX_QUEUE_LENGTH);
            }

            for (int i = 1; i <= M; i++)
            {
                yield return new UnsortedOperationEnumerator<AbstractOperation<OpType>>(G_RowActionGenerator(i, M).GetEnumerator(), Constants.MAX_QUEUE_LENGTH);
            }
        }

        private static IEnumerable<AbstractOperation<OpType>> F_RowActionGenerator(int i, int M)
        {
            //if (i == 1)
            //{
            //    // init R to identity matrix
            //    yield return new AbstractOperation<OpType>() { OP = OpType.Init };
            //}

            // this does not generate the order described in figure 7.
            for (int k = 0; k <= M - i - 1; k++)
            {
                yield return new AbstractOperation<OpType>(k + i, i, k, OpType.F); // { OP = OpType.F, I = k + i, J = i, K = k };
                for (int j = i + k + 1; j <= M; j++)
                {
                    yield return new AbstractOperation<OpType>(j, i, k, OpType.R); // { OP = OpType.R, I = j, J = i, K = k };
                }
            }
            yield return new AbstractOperation<OpType>(M, i, M - i, OpType.F); // { OP = OpType.F, I = M, J = i, K = M - i };
        }

        private static IEnumerable<AbstractOperation<OpType>> G_RowActionGenerator(int i, int M)
        {
            // this does not generate the order described in figure 7.
            for (int k = 0; k <= M - 2; k++)
            {
                yield return new AbstractOperation<OpType>(M - k, i, k, OpType.G); // { OP = OpType.G, I = M - k, J = i, K = k };
                for (int j = M - 1 - k; j >= 1; j--)
                {
                    yield return new AbstractOperation<OpType>(j, i, k, OpType.H); // { OP = OpType.H, I = j, J = i, K = k };
                }
            }
            yield return new AbstractOperation<OpType>(1, i, M - 1, OpType.G); // { OP = OpType.G, I = 1, J = i, K = M - 1 };
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
