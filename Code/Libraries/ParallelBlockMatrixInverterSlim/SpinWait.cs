using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TiledMatrixInversion.ParallelBlockMatrixInverterSlim
{
    public struct SpinWait
    {
        private int m_count;
        private static readonly bool s_isSingleProc = (Environment.ProcessorCount == 1);
        private const int s_yieldFrequency = 4000;
        private const int s_yieldOneFrequency = 3 * s_yieldFrequency;

        public int Spin()
        {
            int oldCount = m_count;

            // On a single-CPU machine, we ensure our counter is always
            // a multiple of ‘s_yieldFrequency’, so we yield every time.
            // Else, we just increment by one.
            m_count += (s_isSingleProc ? s_yieldFrequency : 1);

            // If not a multiple of ‘s_yieldFrequency’ spin (w/ backoff).
            int countModFrequency = m_count % s_yieldFrequency;
            if (countModFrequency > 0)
                Thread.SpinWait((int)(1 + (countModFrequency * 0.05f)));
            else
                Thread.Sleep(m_count <= s_yieldOneFrequency ? 0 : 1);

            return oldCount;
        }

        private void Yield()
        {
            Thread.Sleep(m_count < s_yieldOneFrequency ? 0 : 1);
        }
    }

}
