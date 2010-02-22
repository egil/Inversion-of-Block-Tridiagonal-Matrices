using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using TiledMatrixInversion.ParallelBlockMatrixInverter.Enumerators;
using TiledMatrixInversion.ParallelBlockMatrixInverter.OperationResults;
using TiledMatrixInversion.Resources;

namespace TiledMatrixInversion.ParallelBlockMatrixInverter.MatrixOperations
{
    public class NegateMatrixInverseMatrixMultiply<T> : IProducer<Action>
    {
        #region Fields

        //private const int MAX_ROW_QUEUE_LENGTH = 10;
        private readonly PipelinedOperationEnumerator<UnsortedOperationEnumerator<AbstractOperation<OpType>>, AbstractOperation<OpType>> _gen;
        private readonly object _lock = new object();
        //private readonly OperationResult<T> _inputa;
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
        private readonly int[,] _bstatus;
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
        private readonly int[,] _cstatus;
        private readonly int N;
        private readonly int M;

        #endregion

        public NegateMatrixInverseMatrixMultiply(OperationResult<T> a, OperationResult<T> b, out OperationResult<T> result)
        {
            N = b.Data.Columns;
            M = a.Data.Rows;
            _bstatus = new int[a.Data.Rows + 1, b.Data.Columns + 1];
            _cstatus = new int[a.Data.Rows + 1, b.Data.Columns + 1];
            for (int i = 0; i < _cstatus.GetLength(0); i++)
            {
                for (int j = 0; j < _cstatus.GetLength(1); j++)
                {
                    _cstatus[i, j] = -2;
                }
            }

            //_inputa = a;
            _inputb = b;
            // a.Data is cloned because we are not doing inplace operations on a
            _result = result = new OperationResult<T>(a.Data.Clone(), false);
            _gen = new PipelinedOperationEnumerator<UnsortedOperationEnumerator<AbstractOperation<OpType>>, AbstractOperation<OpType>>(
                RowActionGenerator(a.Data.Rows, b.Data.Columns).GetEnumerator(), Environment.ProcessorCount /* maxmium amount of lines to generate at a time */);
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
                case OpType.A:
                    return _bstatus[op.I, op.J] == op.K - 1 && _bstatus[op.I, op.K] == -1 && _inputb[op.K, op.J];

                case OpType.Bb:
                    return _bstatus[op.I, op.K] == op.K - 1 && _inputb[op.K, op.K];

                case OpType.Bc:
                    return _cstatus[op.I, op.J] == op.K - 1 && _cstatus[op.I, N - (op.K - 1)] == -1 &&
                           _inputb[N - (op.K - 1), op.J];

                case OpType.C:
                    return _cstatus[op.I, op.J] == op.K && _inputb[op.J, op.J];

                default:
                    Debug.Fail("IsRunnable(AbstractOperation<OpType> op): Should not happen!");
                    return false;
            }
        }

        private Action GenerateAction(AbstractOperation<OpType> op)
        {
            if (op == null)
                return null;

            //Debug.WriteLine(Thread.CurrentThread.Name + " [ NegateMatrixInverseMatrixMultiply: " + op + "]");
            switch (op.OP)
            {
                case OpType.A:
                    // calculates line 5 in figure 6, left side.
                    return () =>
                               {
                                   _result.Data[op.I, op.J] = _result.Data[op.I, op.J] -
                                                              _result.Data[op.I, op.K] *
                                                              _inputb.Data[op.K, op.J];
                                   
                                   _bstatus[op.I, op.J] = op.K;
                               };
                // calculates line 3 and 8 in figure 6, left side.
                case OpType.Bb:
                    return () =>
                               {
                                   _result.Data[op.I, op.K] = _result.Data[op.I, op.K] * _inputb.Data[op.K, op.K].GetUpperTriangle().Inverse();
                                   _bstatus[op.I, op.J] = -1;

                                   // when op.J > 1, we are completly done with the previous Bb,
                                   // and can update cstatus to zero and release it to further calculations
                                   if(op.J > 1)
                                   {
                                       _cstatus[op.I, op.J - 1] = 0;
                                   }

                                   if (op.J == N)
                                   {
                                       _cstatus[op.I, op.J] = 0;
                                   }

                               };
                // calculates line 5 in figure 6, right side.
                case OpType.Bc:
                    return () =>
                               {
                                   _result.Data[op.I, op.J] = _result.Data[op.I, op.J] -
                                                              _result.Data[op.I, N - (op.K - 1)] *
                                                              _inputb.Data[N - (op.K - 1), op.J];

                                   _cstatus[op.I, op.J] = op.K;

                               };
                // calculates line 3 and 8 in figure 6, right side.
                case OpType.C:
                    return () =>
                               {

                                   _result.Data[op.I, op.J] = _result.Data[op.I, op.J]*
                                                              _inputb.Data[op.J, op.J].GetLowerTriangleWithFixedDiagonal
                                                                  ().Inverse();
                                   _cstatus[op.I, op.J] = -1;

                                   if (op.J < N)
                                   {
                                       // it is now safe to negate the tile in _result.Data[op.I, op.J + 1]
                                       _result.Data[op.I, op.J + 1] = -_result.Data[op.I, op.J + 1];

                                       // we are done with this tile now, and it is free to be worked on by other operations, inplace.
                                       _result[op.I, op.J + 1] = true;
                                   }

                                   if (op.J == 1)
                                   {
                                       // it is now safe to negate the tile in _result.Data[op.I, op.J]
                                       _result.Data[op.I, op.J] = -_result.Data[op.I, op.J];

                                       // we are done with this tile now, and it is free to be worked on by other operations, inplace.
                                       _result[op.I, op.J] = true;
                                   }

                                    //if (op.I == M && op.J == 1)
                                    //{
                                    //    _result.Completed = true;
                                    //}
                                };

                default:
                    Debug.Fail("IsRunnable(AbstractOperation<OpType> op): Should not happen!");
                    return null;

            }
        }

        #endregion

        #region Abstract operation representation

        private static IEnumerable<UnsortedOperationEnumerator<AbstractOperation<OpType>>> RowActionGenerator(int M, int N)
        {
            for (int i = 1; i <= M; i++)
            {
                yield return new UnsortedOperationEnumerator<AbstractOperation<OpType>>(B_RowActionGenerator(i, N).GetEnumerator(), Constants.MAX_QUEUE_LENGTH);
            }

            for (int i = 1; i <= M; i++)
            {
                yield return new UnsortedOperationEnumerator<AbstractOperation<OpType>>(C_RowActionGenerator(i, N).GetEnumerator(), Constants.MAX_QUEUE_LENGTH);
            }
        }

        private static IEnumerable<AbstractOperation<OpType>> B_RowActionGenerator(int i, int N)
        {
            // this does not generate the order described in figure 7.
            for (int k = 1; k <= N - 1; k++)
            {
                yield return new AbstractOperation<OpType>(i, k, k, OpType.Bb); // { OP = OpType.Bb, I = i, J = k, K = k };
                for (int j = k + 1; j <= N; j++)
                {
                    yield return new AbstractOperation<OpType>(i, j, k, OpType.A); // { OP = OpType.A, I = i, J = j, K = k };
                }
            }
            yield return new AbstractOperation<OpType>(i, N, N, OpType.Bb); // { OP = OpType.Bb, I = i, J = N, K = N };
        }

        private static IEnumerable<AbstractOperation<OpType>> C_RowActionGenerator(int i, int N)
        {
            // this does not generate the order described in figure 7.
            for (int k = 0; k <= N - 2; k++)
            {
                yield return new AbstractOperation<OpType>(i, N - k, k, OpType.C); // { OP = OpType.C, I = i, J = N - k, K = k };
                for (int j = N - 1 - k; j >= 1; j--)
                {
                    yield return new AbstractOperation<OpType>(i, j, k + 1, OpType.Bc); // { OP = OpType.Bc, I = i, J = j, K = k + 1 };
                }
            }
            yield return new AbstractOperation<OpType>(i, 1, N - 1, OpType.C); // { OP = OpType.C, I = i, J = 1, K = N - 1 };
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