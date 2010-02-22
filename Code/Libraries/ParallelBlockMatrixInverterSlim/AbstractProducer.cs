using System;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.Enumerators;

namespace TiledMatrixInversion.ParallelBlockMatrixInverterSlim
{
    public abstract class AbstractProducer<T, A> : IProducer<T> 
        where T : class 
        where A : AbstractOperation
    {
        protected OperationEnumerator<A> _operationBuffer;

        #region Implementation of IProducer<T>

        public bool IsCompleted { get { return _operationBuffer.Completed; } }
        public bool TryGetNext(out T action)
        {
            var operation = _operationBuffer.Find(IsRunnable);
            action = GenerateAction(operation);
            return action != null;
        }

        protected abstract T GenerateAction(A operation);
        protected abstract bool IsRunnable(A operation);

        #endregion
    }
}