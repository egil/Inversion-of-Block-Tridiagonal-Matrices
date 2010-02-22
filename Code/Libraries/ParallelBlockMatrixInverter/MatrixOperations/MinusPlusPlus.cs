using System;
using System.Collections.Generic;
using System.Diagnostics;
using TiledMatrixInversion.ParallelBlockMatrixInverter.Enumerators;
using TiledMatrixInversion.ParallelBlockMatrixInverter.OperationResults;
using TiledMatrixInversion.Resources;

namespace TiledMatrixInversion.ParallelBlockMatrixInverter.MatrixOperations
{
    public class MinusPlusPlus<T> : IProducer<Action>
    {
        
        private readonly object _lock = new object();
        private readonly UnsortedOperationEnumerator<AbstractOperation> _gen;
        private readonly OperationResult<T> _inputa;
        private readonly OperationResult<T> _inputb;
        private readonly OperationResult<T> _inputc;
        private readonly OperationResult<T> _result;

        public MinusPlusPlus(OperationResult<T> a, OperationResult<T> b, OperationResult<T> c, out OperationResult<T> result)
        {
            Debug.Assert(a.Rows == b.Rows && a.Columns == b.Columns, "A does not have the same dimensions as B");
            Debug.Assert(b.Rows == c.Rows && b.Columns == c.Columns, "B does not have the same dimensions as C");

            _inputa = a;
            _inputb = b;
            _inputc = c;

            _result = result = new OperationResult<T>(a.Rows, a.Columns);
            _gen = new UnsortedOperationEnumerator<AbstractOperation>(AbstractOperationGenerator(a.Rows, a.Columns).GetEnumerator(), Constants.MAX_QUEUE_LENGTH);
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

        private Action GenerateAction(AbstractOperation op)
        {
            if (op == null)
                return null;

            return () =>
                       {
                           _result.Data[op.I, op.J] = -_inputa.Data[op.I, op.J] + _inputb.Data[op.I, op.J] + _inputc.Data[op.I, op.J];
                           _result[op.I, op.J] = true;

                           //// update final result completed bit
                           //if (op.I == _result.Rows && op.J == _result.Columns)
                           //    _result.Completed = true;
                       };
        }

        #endregion

        private bool IsRunnable(AbstractOperation op)
        {
            return _inputa[op.I, op.J] && _inputb[op.I, op.J] && _inputc[op.I, op.J];
        }

        private static IEnumerable<AbstractOperation> AbstractOperationGenerator(int rows, int columns)
        {
            for (int i = 1; i <= rows; i++)
            {
                for (int j = 1; j <= columns; j++)
                {
                    yield return new AbstractOperation(i, j); // { I = i, J = j, OP = OpType.Op };
                }
            }
        }
    }
}