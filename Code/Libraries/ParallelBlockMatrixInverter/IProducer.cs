namespace TiledMatrixInversion.ParallelBlockMatrixInverter
{
    public interface IProducer<T>
    {
        bool IsCompleted { get; }
        bool TryGetNext(out T action);
    }
}