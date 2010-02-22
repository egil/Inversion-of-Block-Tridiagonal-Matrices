using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Playground.Threading
{
    public class ThreadManager : ThreadManagerBase 
    {
        private readonly Queue<Action> _workQueue = new Queue<Action>();

        public override void EnqueueTasks(IList<Action> tasks)
        {
            lock (_lock)
            {
                for (int i = 0; i < tasks.Count; i++)
                {
                    _workQueue.Enqueue(tasks[i]);
                }
                Monitor.PulseAll(_lock);
            }
        }

        public override void EnqueueTask(Action task)
        {
            lock (_lock)
            {
                _workQueue.Enqueue(task);
                Monitor.PulseAll(_lock);
            }
        }

        protected override void Consume()
        {
            Debug.WriteLine(Thread.CurrentThread.Name + " [STARTING]");

            while (true)
            {
                Action task;
                lock (_lock)
                {
                    while (_workQueue.Count == 0)
                    {
                        Debug.WriteLine(Thread.CurrentThread.Name + " [SLEEP]");
                        Monitor.Wait(_lock);
                        Debug.WriteLine(Thread.CurrentThread.Name + " [WAKEUP]");
                    }
                    task = _workQueue.Dequeue();
                }

                if (task == null)
                {
                    // This signals our exit
                    Debug.WriteLine(Thread.CurrentThread.Name + " [EXITING]");
                    return;
                }

                task();
                Debug.WriteLine(Thread.CurrentThread.Name + " [WORK DONE]");

                // notify producer about work being finished
                if (_workDoneCallBackMethod != null) _workDoneCallBackMethod(_workQueue.Count);
            }            
        }
    }
}