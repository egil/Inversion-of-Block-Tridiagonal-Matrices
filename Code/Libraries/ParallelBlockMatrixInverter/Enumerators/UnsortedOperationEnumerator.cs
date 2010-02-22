using System.Collections.Generic;

namespace TiledMatrixInversion.ParallelBlockMatrixInverter.Enumerators
{
    public class UnsortedOperationEnumerator<T> : OperationEnumerator<T>
    {
        public UnsortedOperationEnumerator(IEnumerator<T> generator, int maxQueueLength) : base(generator, maxQueueLength){ }
        protected override void Sort() { }
    }
}