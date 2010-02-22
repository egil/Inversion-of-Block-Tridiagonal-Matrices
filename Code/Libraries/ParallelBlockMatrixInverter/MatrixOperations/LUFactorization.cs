using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using TiledMatrixInversion.ParallelBlockMatrixInverter.Enumerators;
using TiledMatrixInversion.ParallelBlockMatrixInverter.OperationResults;
using TiledMatrixInversion.Resources;

namespace TiledMatrixInversion.ParallelBlockMatrixInverter.MatrixOperations
{
    public sealed class LUFactorization<T> : IProducer<Action>
    {
        private readonly object _lock = new object();
        private readonly OperationResult<T> _result;
        
        //private readonly OperationResult<T> _input;
        private readonly OperationEnumerator<LUOP> _modifiedPDSElim;
        
        // represents the time step that has been calculated for a given position in the input
        private readonly int[,] _luStatus;

        public LUFactorization(OperationResult<T> input, out OperationResult<T> result) : this(input, out result, false) { }
        public LUFactorization(OperationResult<T> input, out OperationResult<T> result, bool inplace)
        {
            Debug.Assert(input.Data.Rows == input.Data.Columns);

            if(inplace)
            {
                // reuse the data array from the input, thus making the operation inplace
                _result = result = new OperationResult<T>(input.Data, false /* completed */);
            }
            else
            {
                // create new data array in result
                _result = result = new OperationResult<T>(input.Data.Clone(), false /* completed */);
            }

            _modifiedPDSElim = new OperationEnumerator<LUOP>(PDSElimAlgo(input.Data.Rows).GetEnumerator(), Constants.MAX_QUEUE_LENGTH);
            _luStatus = new int[input.Data.Rows + 1, input.Data.Columns + 1];
        }

        #region IProducer Members

        public bool IsCompleted
        {
            get { return _modifiedPDSElim.Completed; }
        }

        public bool TryGetNext(out Action action)
        {
            action = null;
            lock (_lock)
            {
                // find one in queue that is ready
                var op = _modifiedPDSElim.Find(IsRunnable);

                if (op != null)
                {
                    // generate action
                    action = GenerateAction(op);
                    return true;
                }

                // otherwise sleep
                return false;
            }
        }

        private Action GenerateAction(LUOP op)
        {
            //Debug.WriteLine(Thread.CurrentThread.Name + " [ LUFactorization: " + op + "]");
            switch (op.OP)
            {
                case LUOPType.LU:
                    return () =>
                               {
                                   _result.Data[op.I, op.J] = _result.Data[op.I, op.J].GetLU();
                                   _result[op.I, op.J] = true;
                                   _luStatus[op.I, op.J] = -1;
                                   //// LU operations are always performed on the diagonal, so when
                                   //// last line is reached, we are done.
                                   //if(op.I == _result.Data.Rows)
                                   //    _result.Completed = true;
                               };
                case LUOPType.Adiag:
                case LUOPType.A:
                    return () =>
                               {
                                   _result.Data[op.I, op.J] = _result.Data[op.I, op.J] -
                                                              _result.Data[op.I, op.K] * _result.Data[op.K, op.J];
                                   //_result[op.I, op.J] = false;
                                   _luStatus[op.I, op.J] = op.K;
                               };
                case LUOPType.L:
                    return () =>
                               {
                                   _result.Data[op.I, op.J] = _result.Data[op.I, op.J] * _result.Data[op.J, op.J].GetUpperTriangle().Inverse();
                                   _result[op.I, op.J] = true;
                                   _luStatus[op.I, op.J] = -1;
                               };
                case LUOPType.U:
                    return () =>
                    {
                        _result.Data[op.I, op.J] = _result.Data[op.I, op.I].GetLowerTriangleWithFixedDiagonal().Inverse() * _result.Data[op.I, op.J];
                        _result[op.I, op.J] = true;
                        _luStatus[op.I, op.J] = -1;
                    };
                default:
                    Debug.Fail("Should not happen!");
                    return null;
            }
        }

        private bool IsRunnable(LUOP op)
        {
            switch (op.OP)
            {
                case LUOPType.LU:
                    return _luStatus[op.I, op.I] == op.I - 1;
                case LUOPType.Adiag:
                case LUOPType.A:
                    return _luStatus[op.I, op.J] == op.K - 1 &&
                           _luStatus[op.I, op.K] == -1 &&
                           _luStatus[op.K, op.J] == -1;
                case LUOPType.L:
                    return _luStatus[op.I, op.J] == op.J - 1 &&
                           _luStatus[op.J, op.J] == -1;
                case LUOPType.U:
                    return _luStatus[op.I, op.J] == op.I - 1 &&
                           _luStatus[op.I, op.I] == -1;
                default:
                    Debug.Fail("Should not happen!");
                    return false;
            }
        }

        #endregion

        internal class LUOP : IComparable<LUOP>
        {
            public LUOPType OP
            {
                get
                {
                    if (I == J && J == K)
                    {
                        return LUOPType.LU;
                    }
                    if (J == I)
                    {
                        return LUOPType.Adiag;
                    }
                    if (I == K)
                    {
                        return LUOPType.U;
                    }
                    if (J == K)
                    {
                        return LUOPType.L;
                    }
                    return LUOPType.A;
                }
            }
            public int I { get; set; }
            public int J { get; set; }
            public int K { get; set; }

            /// <summary>
            /// The unique number assigned to every pass of a anti diagonal line.
            /// </summary>
            public int SweepCount { get; set; }
            public int Line { get; set; }

            /// <summary>
            /// Orders LUOP first by line, then by operation type.
            /// The order of operations is:
            /// LU > Adiag > L > U > A
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public int CompareTo(LUOP other)
            {

                // compare by line first
                if (SweepCount < other.SweepCount)
                {
                    return -1;
                }

                if (SweepCount > other.SweepCount)
                {
                    return 1;
                }

                // compare by operation second
                if (OP < other.OP)
                {
                    return -1;
                }

                if (OP > other.OP)
                {
                    return 1;
                }

                return 0;
            }
            public override string ToString()
            {
                if (I == J && J == K)
                {
                    return string.Format("(L_{0}{0}U_{0}{0})", I);
                }
                if (I == K)
                {
                    return string.Format("U_{0}{1}", I, J);
                }
                if (J == K)
                {
                    return string.Format("L_{0}{1}", I, J);
                }

                return string.Format("A_{0}{1}^{2}", I, J, K);
            }
        }

        internal enum LUOPType
        {
            LU = 1,
            Adiag = 2,
            L = 3,
            U = 4,
            A = 5
        }

        /// <summary>
        /// Generates the operation order as presented in figure 4 in stigs article.
        /// </summary>
        /// <param name="N"></param>
        /// <returns></returns>
        private static IEnumerable<LUOP> PDSElimAlgo(int N)
        {
            var count = 1;
            yield return new LUOP() { I = 1, J = 1, K = 1, SweepCount = count, Line = 1 };

            for (int tsum = 2; tsum <= 3 * (N - 1) + 1 + 1; tsum++)
            {
                //yield return (tsum - 1) + ":  ";
                for (int sweep = 1; sweep <= (tsum - 1) / 2; sweep++)
                {
                    count++;
                    // optimize away excessive to nested for loop
                    // gensweep
                    var ttsum = tsum - sweep + 1;
                    for (int i = System.Math.Min(N, ttsum - sweep); i >= System.Math.Max(ttsum - N, sweep); i--)
                    {
                        // genop
                        yield return new LUOP() { I = i, J = ttsum - i, K = sweep, SweepCount = count, Line = tsum - 1 };
                    }
                }
            }
        }

    }
}