using System;
using System.Collections.Generic;
using System.Diagnostics;
using TiledMatrixInversion.ParallelBlockMatrixInverter.Enumerators;
using TiledMatrixInversion.ParallelBlockMatrixInverter.OperationResults;
using TiledMatrixInversion.Resources;

namespace TiledMatrixInversion.ParallelBlockMatrixInverter.MatrixOperations
{
    public class Multiply<T> : IProducer<Action>
    {
        
        private readonly object _lock = new object();
        private readonly UnsortedOperationEnumerator<AbstractOperation> _gen;
        private readonly OperationResult<T> _inputa;
        private readonly OperationResult<T> _inputb;
        private readonly OperationResult<T> _result;

        public Multiply(OperationResult<T> a, OperationResult<T> b, out OperationResult<T> result)
        {
            Debug.Assert(a.Columns == b.Rows, "The number of columns in matrix A is not equal to the number of rows in matrix B.");
            _inputa = a;
            _inputb = b;
            _result = result = new OperationResult<T>(a.Data.Rows, b.Data.Columns);
            _gen = new UnsortedOperationEnumerator<AbstractOperation>(AbstractOperationGenerator(a.Rows, b.Columns).GetEnumerator(), Constants.MAX_QUEUE_LENGTH);
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
                           var cols = _inputa.Columns;
                           var a = _inputa.Data;
                           var b = _inputb.Data;
                           var i = op.I;
                           var j = op.J;

                           var s = a[i, 1] * b[1, j];
                           for (int k = 2; k <= cols; k++)
                           {
                               s += a[i, k] * b[k, j];
                           }
                           _result.Data[i, j] = s;
                           _result[i, j] = true;

                           //// update final result completed bit
                           //if(i == _result.Rows && j == _result.Columns)
                           //    _result.Completed = true;
                       };
        }

        #endregion

        private bool IsRunnable(AbstractOperation op)
        {
            bool res = true;
            var acols = _inputa.Columns;
            var a = _inputa;
            var brows = _inputb.Rows;
            var b = _inputb;

            for (int j = 1; j <= acols; j++)
            {
                res = res && a[op.I, j];
            }

            for (int i = 1; i <= brows; i++)
            {
                res = res && b[i, op.J];
            }

            return res;
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
