using System;
using System.Collections.Generic;

namespace TiledMatrixInversion.ParallelBlockMatrixInverterSlim.Enumerators
{
    public class OperationEnumerator<T>
    {
        protected List<T> _queue;
        protected Enumerator<T> _gen;
        protected readonly int _maxQueueLength;
        protected readonly int _minQueueLength;

        public OperationEnumerator(IEnumerable<T> generator, int maxQueueLength)
        {
            _queue = new List<T>(maxQueueLength);
            _maxQueueLength = maxQueueLength;
            _minQueueLength = maxQueueLength > 2 ? maxQueueLength / 2 : 1;
            _gen = new Enumerator<T>(generator.GetEnumerator());
        }

        public virtual bool Completed { get { return _gen.Completed && _queue.Count == 0; } }

        public virtual T Find(Predicate<T> match)
        {
            FillQueue();
            T res = _queue.Find(match);
            if (!Equals(res, default(T)))
            {
                _queue.Remove(res);
            }
            return res;
        }

        protected virtual void FillQueue()
        {
            // generate data for queue
            if (_queue.Count < _minQueueLength)
            {
                while (_queue.Count < _maxQueueLength && !_gen.Completed)
                {
                    _queue.Add(_gen.Next());
                }
            }
        }

        /// <summary>
        /// A wrapper for the IEnumerator type in .net, supports java like iterators.
        /// </summary>
        /// <typeparam name="TU"></typeparam>
        protected class Enumerator<TU>
        {
            private IEnumerator<TU> _gen;
            public Enumerator(IEnumerator<TU> generator)
            {
                _gen = generator;
                Completed = !_gen.MoveNext();
            }

            public bool Completed { get; private set; }
            public TU Next()
            {
                var tmp = _gen.Current;
                Completed = !_gen.MoveNext();
                return tmp;
            }
        }
    }
}