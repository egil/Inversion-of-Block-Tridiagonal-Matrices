using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace TiledMatrixInversion.ParallelBlockMatrixInverterSlim
{
    /// <summary>
    /// This SpinWaitLock is copied from the MSDN Concurrent Affairs column "Performance-Conscious Thread Synchronization"
    /// by Jeffrey Richter, see http://msdn.microsoft.com/en-us/magazine/cc163726.aspx#S3
    /// </summary>
    /// <remarks>
    /// NOTE: This is a value type so it works very efficiently when used as
    /// a field in a class. Avoid boxing this or you will lose thread safety!
    /// </remarks>
    public struct SpinWaitLock
    {
        private const Int32 c_lsFree = 0;
        private const Int32 c_lsOwned = 1;
        private Int32 m_LockState; // Defaults to 0=c_lsFree

        public void Enter()
        {
            Thread.BeginCriticalRegion();
            while (true)
            {
                // If resource available, set it to in-use and return
                if (Interlocked.Exchange(ref m_LockState, c_lsOwned) == c_lsFree)
                {
                    return;
                }

                // Efficiently spin, until the resource looks like it might 
                // be free. NOTE: Just reading here (as compared to repeatedly 
                // calling Exchange) improves performance because writing 
                // forces all CPUs to update this value
                while (Thread.VolatileRead(ref m_LockState) == c_lsOwned)
                {
                    StallThread();
                }
            }
        }

        public void Exit()
        {
            // Mark the resource as available
            Interlocked.Exchange(ref m_LockState, c_lsFree);
            Thread.EndCriticalRegion();
        }

        private static void StallThread()
        {
            Thread.SpinWait(1);
        }
    }


}