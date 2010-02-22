using System;
using System.Collections;
using System.Collections.Generic;

namespace TiledMatrixInversion.ParallelBlockMatrixInverter.Enumerators
{
    public class OperationEnumerator<T>
    {
        protected readonly List<T> _queue;
        protected readonly Enumerator<T> _gen;
        protected readonly int _maxQueueLength;

        public OperationEnumerator(IEnumerator<T> generator, int maxQueueLength)
        {
            _queue = new List<T>(maxQueueLength);
            _maxQueueLength = maxQueueLength;
            _gen = new Enumerator<T>(generator);
        }

        public virtual bool Completed { get { return _gen.Completed && _queue.Count == 0; } }

        public virtual bool Exists(Predicate<T> match)
        {
            FillQueue();
            return _queue.Exists(match);
        }

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
            if (_queue.Count == 0)
            {
                while (_queue.Count < _maxQueueLength && !_gen.Completed)
                {
                    _queue.Add(_gen.Next());
                }
                Sort();
            }
        }

        protected virtual void Sort()
        {
            _queue.Sort();
        }

        /// <summary>
        /// A wrapper for the IEnumerator type in .net, supports java like iterators.
        /// </summary>
        /// <typeparam name="U"></typeparam>
        protected class Enumerator<U>
        {
            private readonly IEnumerator<U> _gen;
            public Enumerator(IEnumerator<U> generator)
            {
                _gen = generator;
                Completed = !_gen.MoveNext();
            }

            public bool Completed { get; private set; }
            public U Next()
            {
                var tmp = _gen.Current;
                Completed = !_gen.MoveNext();
                return tmp;
            }
        }
    }
}