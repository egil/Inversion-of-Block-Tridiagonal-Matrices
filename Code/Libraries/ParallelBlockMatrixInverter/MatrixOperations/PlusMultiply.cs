using System;
using System.Collections.Generic;
using System.Diagnostics;
using TiledMatrixInversion.ParallelBlockMatrixInverter.Enumerators;
using TiledMatrixInversion.ParallelBlockMatrixInverter.OperationResults;
using TiledMatrixInversion.Resources;

namespace TiledMatrixInversion.ParallelBlockMatrixInverter.MatrixOperations
{
    public sealed class PlusMultiply<T> : IProducer<Action>
    {
        
        private readonly object _lock = new object();
        private readonly UnsortedOperationEnumerator<AbstractOperation> _gen;
        private readonly OperationResult<T> _inputa;
        private readonly OperationResult<T> _inputb;
        private readonly OperationResult<T> _inputc;
        private readonly OperationResult<T> _result;

        public PlusMultiply(OperationResult<T> a, OperationResult<T> b, OperationResult<T> c, out OperationResult<T> result)
        {
            Debug.Assert(b.Columns == c.Rows, "The number of columns in matrix B is not equal to the number of rows in matrix C.");
            Debug.Assert(a.Rows == b.Rows && a.Columns == c.Columns, "A does not have the same dimensions as the product B*C");

            _inputa = a;
            _inputb = b;
            _inputc = c;

            _result = result = new OperationResult<T>(a.Rows, a.Columns);
            _gen = new UnsortedOperationEnumerator<AbstractOperation>(AbstractOperationGenerator(b.Rows, c.Columns).GetEnumerator(), Constants.MAX_QUEUE_LENGTH);
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
                           var cols = _inputb.Columns;
                           var b = _inputb.Data;
                           var c = _inputc.Data;
                           var i = op.I;
                           var j = op.J;

                           var s = b[i, 1] * c[1, j];
                           for (int k = 2; k <= cols; k++)
                           {
                               s += b[i, k] * c[k, j];
                           }
                           _result.Data[i, j] = _inputa.Data[i, j] + s;
                           _result[i, j] = true;
                       };
        }

        #endregion

        private bool IsRunnable(AbstractOperation op)
        {
            bool res = _inputa[op.I, op.J];

            if (!res)
                return false;

            var bcols = _inputb.Columns;
            var b = _inputb;
            var crows = _inputc.Rows;
            var c = _inputc;

            for (int j = 1; j <= bcols; j++)
            {
                res = res && b[op.I, j];
            }

            for (int i = 1; i <= crows; i++)
            {
                res = res && c[i, op.J];
            }

            return res;
        }

        private static IEnumerable<AbstractOperation> AbstractOperationGenerator(int rows, int columns)
        {
            for (int i = 1; i <= rows; i++)
            {
                for (int j = 1; j <= columns; j++)
                {
                    yield return new AbstractOperation(i, j); // {I = i, J = j, OP = OpType.Op};
                }
            }
        }
    }
}