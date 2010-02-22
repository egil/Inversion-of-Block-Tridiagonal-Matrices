using System;
using System.Collections.Generic;

namespace TiledMatrixInversion.ParallelBlockMatrixInverterSlim.Enumerators
{
    public class PipelinedOperationEnumerator<T, TU> : OperationEnumerator<T> where T : OperationEnumerator<TU>
    {
        public PipelinedOperationEnumerator(IEnumerable<T> generator, int maxQueueLength) : base(generator, maxQueueLength)
        { }       

        public TU Find(Predicate<TU> match)
        {
            FillQueue();

            for (int i = 0; i < _queue.Count; i++)
            {
                TU res = _queue[i].Find(match);
                if (!Equals(res, default(TU)))
                {
                    if (_queue[i].Completed)
                        _queue.Remove(_queue[i]);
                    
                    return res;
                }
            }

            return default(TU);
        }

        protected override void FillQueue()
        {
            // generate data for queue
            if (_queue.Count < _maxQueueLength)
            {
                while (_queue.Count < _maxQueueLength && !_gen.Completed)
                {
                    _queue.Add(_gen.Next());
                }
            }
        }
    }
}