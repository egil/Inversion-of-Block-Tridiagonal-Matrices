using System;
using System.Threading;

namespace TiledMatrixInversion.ParallelBlockMatrixInverterSlim
{
    public struct ThinEvent
    {
        private int m_state; // 0 means unset, 1 means set.
        private EventWaitHandle m_eventObj;
        private const int s_spinCount = 4000;

        public void Set()
        {
            m_state = 1;
            Thread.MemoryBarrier(); // required.
            if (m_eventObj != null)
                m_eventObj.Set();
        }

        public void Reset()
        {
            m_state = 0;
            if (m_eventObj != null)
                m_eventObj.Reset();
        }

        public void Wait()
        {
            SpinWait s = new SpinWait();
            while (m_state == 0)
            {
                if (s.Spin() >= s_spinCount)
                {
                    if (m_eventObj == null)
                    {
                        ManualResetEvent newEvent =
                            new ManualResetEvent(m_state == 1);
                        if (Interlocked.CompareExchange<EventWaitHandle>(
                                ref m_eventObj, newEvent, null) == null)
                        {
                            // If someone set the flag before seeing the new
                            // event obj, we must ensure it’s been set.
                            if (m_state == 1)
                                m_eventObj.Set();
                        }
                        else
                        {
                            // Lost the race w/ another thread. Just use
                            // its event.
                            newEvent.Close();
                        }
                    }
                    m_eventObj.WaitOne();
                }
            }
        }

        public bool Wait(TimeSpan timeout)
        {
            SpinWait s = new SpinWait();
            bool waitResult = true;
            while (m_state == 0)
            {
                if (s.Spin() >= s_spinCount)
                {
                    if (m_eventObj == null)
                    {
                        ManualResetEvent newEvent =
                            new ManualResetEvent(m_state == 1);
                        if (Interlocked.CompareExchange<EventWaitHandle>(
                                ref m_eventObj, newEvent, null) == null)
                        {
                            // If someone set the flag before seeing the new
                            // event obj, we must ensure it’s been set.
                            if (m_state == 1)
                                m_eventObj.Set();
                        }
                        else
                        {
                            // Lost the race w/ another thread. Just use
                            // its event.
                            newEvent.Close();
                        }
                    }
                    waitResult = m_eventObj.WaitOne(timeout);
                }
            }
            return waitResult;
        }

    }

}