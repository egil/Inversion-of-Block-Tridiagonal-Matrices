using System;
using System.Diagnostics;

namespace TiledMatrixInversion.ParallelBlockMatrixInverterSlim.MatrixOperations
{
    public class SimpleProducer : IProducer<Action>
    {
        private readonly Action _action;

        public SimpleProducer(Action action)
        {
            _action = action;
        }

        #region Implementation of IProducer<T>

        public bool IsCompleted
        {
            get;
            private set;
        }

        public bool TryGetNext(out Action action)
        {
            if (!IsCompleted)
            {
                IsCompleted = true;
                action = _action;
                return IsCompleted;
            }

            action = null;
            return false;
        }

        #endregion
    }
}