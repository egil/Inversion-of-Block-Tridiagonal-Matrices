using System;
using System.Diagnostics;
using System.Threading;

namespace TiledMatrixInversion.ParallelBlockMatrixInverterSlim
{
    public sealed class Manager
    {
        #region fields

        private static readonly TraceSource Log = new TraceSource("Manager");
        private static readonly TimeSpan FAILED_WAIT_TIMEOUT = TimeSpan.FromSeconds(5);

        // making this field readonly somehow results in deadlocks
        private SpinWaitLock _tryGetLock = new SpinWaitLock();
        //private SpinWaitLock _producerLock = new SpinWaitLock();
        //private ThinEvent _workDoneEvent = new ThinEvent();
        private int _threadCount;
        private readonly Thread[] _workers;
        private readonly IProducer<Action> _producer;
        private readonly ManualResetEvent _workComplete;
        private readonly object _sleeper = new object();

        #endregion

        #region ctors

        public Manager(IProducer<Action> workProducer) : this(workProducer, Environment.ProcessorCount) { }
        public Manager(IProducer<Action> workProducer, int threadCount)
        {
            _workComplete = new ManualResetEvent(false);
            _producer = workProducer;

            _workers = new Thread[threadCount];
            _threadCount = threadCount;

            for (int i = 0; i < _workers.Length; i++)
            {
                _workers[i] = new Thread(GetWork) { Name = ("Worker " + i) };
            }
        }

        #endregion

        public void Start()
        {
            Log.TraceEvent(TraceEventType.Start, 0, "Threads");
            foreach (var thread in _workers) { thread.Start(); }
        }

        private void GetWork()
        {
            Log.TraceEvent(TraceEventType.Start, 0, Thread.CurrentThread.Name);

            while (true)
            {
                // queue up in line to get more work.
                // a spinlock is used since it is very fast to get work
                // from the producer, and a full wait in kernel mode
                // would eat up a lot of clock cycles
                Action action;
                bool isComplete = false;
                bool getNextResult = false;

                #region new manager

                //// enter producer lock
                //_producerLock.Enter();

                //// if producer is completed, exit producer lock and break out of loop
                //if(_producer.IsCompleted)
                //{
                //    _producerLock.Exit();
                //    Log.TraceEvent(TraceEventType.Information, 1, "Producer IsCompelete");
                //    break;
                //}

                //// look for work in producer
                //if(_producer.TryGetNext(out action))
                //{
                //    // if work was found, exit producer lock and continue
                //    _producerLock.Exit();
                    
                //    // perform work
                //    action();
                    
                //    // signal waiting threads
                //    _workDoneEvent.Set();

                //    // logging
                //    Statistics.WorkDoneCount++;
                //    Log.TraceEvent(TraceEventType.Verbose, 0, "{0} : Work done", Thread.CurrentThread.Name);
                //}
                //else
                //{
                //    // no work found, exit producer lock and wait for another worker to finsh
                //    _workDoneEvent.Reset();
                //    _producerLock.Exit();
                //    if(!_workDoneEvent.Wait(FAILED_WAIT_TIMEOUT))
                //    {                        
                //        Statistics.FailedWaitCount++;
                //        if (_producer.IsCompleted) Statistics.FailedWaitCountWhenComplete++;
                //        Log.TraceEvent(TraceEventType.Error, 0, "Thread waited to long for pulse!");
                //    }
                //    Statistics.WaitCount++;
                //    Log.TraceEvent(TraceEventType.Resume, 0, Thread.CurrentThread.Name);
                //}

                #endregion

                try
                {
                    _tryGetLock.Enter();
                    isComplete = _producer.IsCompleted;
                    getNextResult = _producer.TryGetNext(out action);
                }
                finally
                {
                    _tryGetLock.Exit();
                }

                // if there was any work found in TryGetNext, start processing
                // else wait till somebody else finishes something and try 
                // getting more work.
                if (getNextResult)
                {
                    action();
                    Log.TraceEvent(TraceEventType.Verbose, 0, "{0} : Work done", Thread.CurrentThread.Name);
                    Statistics.WorkDoneCount++;

                    // see http://www.albahari.com/threading/part4.aspx#_Wait_and_Pulse
                    lock (_sleeper) Monitor.PulseAll(_sleeper);
                }
                else if (isComplete)
                {
                    Log.TraceEvent(TraceEventType.Information, 1, "getNextResult {0}, Producer IsCompelete {1}", getNextResult, isComplete);
                    break;
                }
                else
                {
                    Log.TraceEvent(TraceEventType.Suspend, 0, Thread.CurrentThread.Name);

                    Statistics.WaitCount++;

                    // see http://www.albahari.com/threading/part4.aspx#_Wait_and_Pulse
                    lock (_sleeper)
                    {
                        if (_producer.IsCompleted)
                            break;

                        if (!Monitor.Wait(_sleeper, FAILED_WAIT_TIMEOUT))
                        {
                            Log.TraceEvent(TraceEventType.Error, 0, "Thread waited to long for pulse!");
                            if (_producer.IsCompleted) Statistics.FailedWaitCountWhenComplete++;
                            Statistics.FailedWaitCount++;
                        }
                    }

                    Log.TraceEvent(TraceEventType.Resume, 0, Thread.CurrentThread.Name);
                }
            }

            Log.TraceEvent(TraceEventType.Stop, 0, Thread.CurrentThread.Name);
            Statistics.Register();

            if (Interlocked.Decrement(ref _threadCount) == 0)
            {
                _workComplete.Set();
                Log.TraceEvent(TraceEventType.Stop, 0, "Threads");
            }
        }

        public void Join()
        {
            _workComplete.WaitOne();
        }
    }
}