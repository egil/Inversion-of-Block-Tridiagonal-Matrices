using System;
using System.Collections.Generic;
using System.Threading;

namespace TiledMatrixInversion.ParallelBlockMatrixInverterSlim
{
    public static class Statistics
    {
        private static object _lock = new object();
        [ThreadStatic] public static int WaitCount;
        [ThreadStatic] public static int WorkDoneCount;
        [ThreadStatic] public static int SecondaryProducerCount;
        [ThreadStatic] public static int FailedWaitCount;
        [ThreadStatic] public static int FailedWaitCountWhenComplete;

        public static Dictionary<string, int> GlobalWaitCount = new Dictionary<string, int>();
        public static Dictionary<string, int> GlobalWorkDoneCount = new Dictionary<string, int>();
        public static Dictionary<string, int> GlobalSecondaryProducerCount = new Dictionary<string, int>();
        public static int GlobalFailedWaitCount;
        public static int GlobalFailedWaitCountWhenComplete;

        public static void Register()
        {
            lock(_lock)
            {
                GlobalWaitCount[Thread.CurrentThread.Name] = WaitCount;
                GlobalWorkDoneCount[Thread.CurrentThread.Name] = WorkDoneCount;
                GlobalSecondaryProducerCount[Thread.CurrentThread.Name] = SecondaryProducerCount;   
            }
            Interlocked.Add(ref GlobalFailedWaitCount, FailedWaitCount);
            Interlocked.Add(ref GlobalFailedWaitCountWhenComplete, FailedWaitCountWhenComplete);
        }
        
        public static void Reset()
        {
            lock (_lock)
            {
                GlobalWorkDoneCount.Clear();
                GlobalSecondaryProducerCount.Clear();
                GlobalWaitCount.Clear();
            }
            Interlocked.Exchange(ref GlobalFailedWaitCount, 0);
            Interlocked.Exchange(ref GlobalFailedWaitCountWhenComplete, 0);
        }
    }
}
