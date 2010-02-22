using System;
using System.Collections.Generic;
using System.Threading;

namespace Playground.Threading
{
    public abstract class ThreadManagerBase : IDisposable
    {
        protected readonly object _lock = new object();
        protected Thread[] _workers;
        protected Action<int> _workDoneCallBackMethod;
        public void Init()
        {
            Init(Environment.ProcessorCount);   
        }
        public void Init(int workerCount)
        {
            Init(workerCount, null);
        }
        public void Init(int workerCount, Action<int> workDoneCallBackMethod)
        {
            _workers = new Thread[workerCount];
            _workDoneCallBackMethod = workDoneCallBackMethod;

            // Create and start a separate thread for each worker
            for (int i = 0; i < workerCount; i++)
            {
                _workers[i] = new Thread(Consume) { Name = "Worker " + (1 + i) };
                _workers[i].Start();
            } 
        }
        public abstract void EnqueueTasks(IList<Action> tasks);
        public abstract void EnqueueTask(Action task);
        protected abstract void Consume();
        public virtual void Dispose()
        {
            // Enqueue one null task per worker to make each exit.
            foreach (Thread worker in _workers) EnqueueTask(null);
            foreach (Thread worker in _workers) worker.Join();
        }
    }
}