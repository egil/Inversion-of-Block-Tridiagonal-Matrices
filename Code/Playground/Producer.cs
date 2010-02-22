using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Playground.Threading;
using TiledMatrixInversion.ParallelBlockMatrixInverter;

namespace Playground
{
    public class Producer<T> where T : ThreadManagerBase, new()
    {
        private readonly int _minTaskQueueLength;
        private readonly int _maxTaskQueueLength;
        private readonly EventWaitHandle _waitLock = new ManualResetEvent(false);
        private int _threadsProducing;
        private readonly IProducer<Action> _tasks;
        private ThreadManagerBase _manager;
        private readonly int _workerCount;

        public Producer(IProducer<Action> tasks) : this(tasks, Environment.ProcessorCount) { }
        public Producer(IProducer<Action> tasks, int workerCount)
        {
             if(workerCount<=0) throw new ArgumentException("Worker count can not be less than or equal to zero.", "workerCount");

            _tasks = tasks;
            _minTaskQueueLength = workerCount;
            _maxTaskQueueLength = workerCount*8;
            _workerCount = workerCount;
        }

        public void Start()
        {            
            using(_manager = new T())
            {
                _manager.Init(_workerCount, TryProduce);
                TryProduce(0);

                // wait for all tasks to be finished
                _waitLock.WaitOne();
            }
        }

        private void TryProduce(int itemsLeftInQueue)
        {
            // use Interlocked.CompareExchange to create a full fench memory barrier
            // see http://www.albahari.com/threading/part4.aspx#_NonBlockingSynch
            if (itemsLeftInQueue < _minTaskQueueLength && Interlocked.CompareExchange(ref _threadsProducing, 1, 0) == 0)
            {
                // generate more runnable tasks
                List<Action> newTasks = new List<Action>(_maxTaskQueueLength - itemsLeftInQueue);
                Action task;
                while(itemsLeftInQueue < _maxTaskQueueLength && !_tasks.IsCompleted && _tasks.TryGetNext(out task))
                {
                    newTasks.Add(task);
                    itemsLeftInQueue++;
                }

                // enqueue runnable tasks
                if(newTasks.Count > 0) _manager.EnqueueTasks(newTasks);

                // if there are no more tasks to process, 
                // tell the producer to call dispose on ThreadManager and return
                if (_tasks.IsCompleted) _waitLock.Set();
                
                Interlocked.Decrement(ref _threadsProducing);
            }
        }
    }
}