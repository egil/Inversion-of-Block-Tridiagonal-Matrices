using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using JmBucknall.Structures;

namespace Playground.Threading
{
    public class ThreadManager2 : ThreadManagerBase
    {
        private readonly LockFreeQueue<Action> _workQueue = new LockFreeQueue<Action>();
        private int _queueLength;
        
        public override void EnqueueTasks(IList<Action> tasks)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                _workQueue.Enqueue(tasks[i]);
            }
            Interlocked.Add(ref _queueLength, tasks.Count);
            lock (_lock) Monitor.PulseAll(_lock); 
        }

        public override void EnqueueTask(Action task)
        {
            _workQueue.Enqueue(task);
            Interlocked.Increment(ref _queueLength);
            lock (_lock) Monitor.PulseAll(_lock);
        }

        protected override void Consume()
        {
            Debug.WriteLine(Thread.CurrentThread.Name + " [STARTING]");

            while (true)
            {
                Action task;
                if (!_workQueue.Dequeue(out task))
                {
                    lock (_lock)
                    {
                        Debug.WriteLine(Thread.CurrentThread.Name + " [SLEEP]");
                        Monitor.Wait(_lock);
                        Debug.WriteLine(Thread.CurrentThread.Name + " [WAKEUP]");
                    }
                    // jump to start of while loop
                    continue;
                }

                // update queue length after succesfull dequeue
                Interlocked.Decrement(ref _queueLength);

                // This signals our exit
                if (task == null)
                {                    
                    Debug.WriteLine(Thread.CurrentThread.Name + " [EXITING]");
                    return;
                }

                task();                
                Debug.WriteLine(Thread.CurrentThread.Name + " [WORK DONE]");

                // notify producer about work being finished
                if (_workDoneCallBackMethod != null) _workDoneCallBackMethod(_queueLength);
            }
        }
    }
}