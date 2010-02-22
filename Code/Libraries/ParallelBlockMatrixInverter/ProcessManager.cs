using System;
using System.Diagnostics;
using System.Threading;

namespace TiledMatrixInversion.ParallelBlockMatrixInverter
{
    public sealed class ProcessManager
    {
        #region fields

        private int _threadCount;
        private readonly Thread[] _workers;
        private readonly IProducer<Action> _producer;
        private readonly AutoResetEvent _actionDone;
        private readonly ManualResetEvent _workComplete;
        private bool _isRunning;
        #endregion

        #region ctors

        public ProcessManager(IProducer<Action> workProducer) : this(workProducer, Environment.ProcessorCount) { }
        public ProcessManager(IProducer<Action> workProducer, int threadCount)
        {
            _workComplete = new ManualResetEvent(false);
            _actionDone = new AutoResetEvent(false);
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
            _isRunning = true;
            foreach (var thread in _workers) { thread.Start(); }            
        }

        public void Stop()
        {
            _isRunning = false;
        }

        private void GetWork()
        {
            Debug.WriteLine(Thread.CurrentThread.Name + " [STARTING]");

            while (_isRunning && !_producer.IsCompleted)
            {
                // do some work if there is any
                Action action;
                if (_producer.TryGetNext(out action))
                {
                    action();
                    Debug.WriteLine(Thread.CurrentThread.Name + " [WORK DONE]");
                    _actionDone.Set();
                }
                else
                {
                    Debug.WriteLine(Thread.CurrentThread.Name + " [SLEEP]");
                    _actionDone.WaitOne();
                    Debug.WriteLine(Thread.CurrentThread.Name + " [WAKEUP]");
                }
            }

            Debug.WriteLine(Thread.CurrentThread.Name + " [EXITING]");
            if (Interlocked.Decrement(ref _threadCount) == 0)
                _workComplete.Set();
        }

        public void Join()
        {
            _workComplete.WaitOne();
        }
    }
}