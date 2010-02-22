using System;

namespace TiledMatrixInversion.ParallelBlockMatrixInverterSlim
{
    public interface IProducer<T>    
    {
        bool IsCompleted { get; }
        bool TryGetNext(out T action);
    }
}