using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.Enumerators;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.OperationResults;
using TiledMatrixInversion.Resources;

namespace TiledMatrixInversion.ParallelBlockMatrixInverterSlim.MatrixOperations
{
    public sealed class LUFactorization<T> : IProducer<Action>
    {
        private readonly OperationResult<T> _result;
        private readonly OperationResult<T> _inputa;
        private OperationEnumerator<AbstractOperation<OpType>> _gen;

        // represents the time step that has been calculated for a given position in the input
        // -1 if the calculation is complete.
        private readonly int[][] _luStatus;
        private readonly bool _inplace;
        private bool _hasCompletedInit;

        public LUFactorization(OperationResult<T> input, out OperationResult<T> result) : this(input, out result, false) { }
        public LUFactorization(OperationResult<T> input, out OperationResult<T> result, bool inplace)
        {
            Debug.Assert(input.Data.Rows == input.Data.Columns);
            _inplace = inplace;
            _inputa = input;

            _result = result = new OperationResult<T>(input.Rows, input.Columns, true /* layzy init */);

            _gen = new OperationEnumerator<AbstractOperation<OpType>>(AbstractOperationGenerator(input.Rows), Constants.MAX_QUEUE_LENGTH);
            _luStatus = Helpers.Init<int>(input.Rows + 1, input.Columns + 1);
        }


        private bool TryInit()
        {            
            if (_inputa.Data == null)
                return false;

            if (_inputa.Data.Any(x => x == null))
                return false;

            // reuse the data array from the input if _inplace is true,
            // thus making the operation inplace.
            _result.Data = _inplace ? _inputa.Data : _inputa.Data.Clone();

            return _hasCompletedInit = true;
        }

        #region IProducer Members

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

        private Action GenerateAction(AbstractOperation<OpType> op)
        {
            if (op == null)
                return null;

            switch (op.OP)
            {
                case OpType.LU:
                    return () =>
                               {
                                   _result.Data[op.I, op.J] = _result.Data[op.I, op.J].GetLU();
                                   _result[op.I, op.J] = true;
                                   _luStatus[op.I][op.J] = -1;
                               };
                case OpType.A:
                    return () =>
                               {
                                   _result.Data[op.I, op.J] = _result.Data[op.I, op.J] -
                                                              _result.Data[op.I, op.K] * _result.Data[op.K, op.J];
                                   _luStatus[op.I][op.J] = op.K;
                               };
                case OpType.L:
                    return () =>
                               {
                                   _result.Data[op.I, op.J] = _result.Data[op.I, op.J] * _result.Data[op.J, op.J].GetUpperTriangle().Inverse();
                                   _result[op.I, op.J] = true;
                                   _luStatus[op.I][op.J] = -1;
                               };
                case OpType.U:
                    return () =>
                               {
                                   _result.Data[op.I, op.J] = _result.Data[op.I, op.I].GetLowerTriangleWithFixedDiagonal().Inverse() * _result.Data[op.I, op.J];
                                   _result[op.I, op.J] = true;
                                   _luStatus[op.I][op.J] = -1;
                               };
                default:
                    Debug.Fail("Should not happen!");
                    return null;
            }
        }

        private bool IsRunnable(AbstractOperation<OpType> op)
        {
            switch (op.OP)
            {
                case OpType.LU:
                    return _luStatus[op.I][op.I] == op.I - 1;
                case OpType.A:
                    return _luStatus[op.I][op.J] == op.K - 1 &&
                           _luStatus[op.I][op.K] == -1 &&
                           _luStatus[op.K][op.J] == -1;
                case OpType.L:
                    return _luStatus[op.I][op.J] == op.J - 1 &&
                           _luStatus[op.J][op.J] == -1;
                case OpType.U:
                    return _luStatus[op.I][op.J] == op.I - 1 &&
                           _luStatus[op.I][op.I] == -1;
                default:
                    Debug.Fail("Should not happen!");
                    return false;
            }
        }

        #endregion

        private static IEnumerable<AbstractOperation<OpType>> AbstractOperationGenerator(int N)
        {
            // PDS elimination algorithm from Stigs article §3.2.1
            for (int stage = 1, endStage = 3 * (N - 1) + 1; stage <= endStage; stage++)
            {
                // Lower bound: The first sweep completes after 2*N-1 stages and at each succesive stage another
                //   sweep completes. Thus at stage S > 2*N-1, S-(2*N-1) sweeps have completed and S-(2*N-1)+1
                //   is the first to be processed.
                // Upper bound: For every third _completed_ stages, a new sweep can start. Thus at stage four
                //   the second sweep can start.
                for (int sweep = System.Math.Max(1, stage - (2 * N - 1) + 1), endSweep = (stage - 1) / 3 + 1; sweep <= endSweep; sweep++)
                {
                    // generate sweep
                    // sweep = is the diagonal sweep number, sweep \in [1,N]
                    // tsum = is the sum of the indices in the antidiagonal line to process, tsum \in [2*sweep,2*N]
                    // tsum is calculated like this: the index sum of elements being processed at stage S
                    //   is S + 1. Each sweep is one step behind the previous, and thus is at index sum 
                    //   stage + 1 - (sweep - 1). (Sweeps start at one).
                    int tsum = stage + 1 - (sweep - 1);
                    int iMax = System.Math.Min(N, tsum - sweep);
                    int iMin = System.Math.Max(tsum - N, sweep);

                    // if it has a diagonal element, do it first
                    if (tsum % 2 == 0)
                    {
                        var i = tsum / 2;

                        if (i == sweep)
                        {
                            yield return new AbstractOperation<OpType>(i, i, OpType.LU);
                            continue; // jump to start of for loop again
                        }
                        yield return new AbstractOperation<OpType>(i, i, sweep, OpType.A);
                    }

                    if (tsum - sweep <= N)
                    {
                        // do L_ij and U_ij second
                        yield return new AbstractOperation<OpType>(iMax, sweep, OpType.L);
                        yield return new AbstractOperation<OpType>(sweep, tsum - iMin, OpType.U);
                    }

                    // walk the anti diagonal with indices tsum - j, j
                    // The j index of the sub matrix to be updated using 
                    // A_i,j^(k) operations is bounded by sweep+1 and tsum-(sweep+1)
                    // when above the antidiagonal and tsum-N and N when below
                    // the antidiagonal.
                    for (int j = System.Math.Max(sweep + 1, tsum - N); j <= System.Math.Min(tsum - (sweep + 1), N); j++)
                    {
                        // skip the diagonal, already calculated above
                        if (j != tsum - j)
                        {
                            yield return new AbstractOperation<OpType>(tsum - j, j, sweep, OpType.A);
                        }
                    }
                }
            }
        }

        enum OpType
        {
            LU,
            U,
            L,
            A
        }
    }
}