using System;

namespace TiledMatrixInversion.Resources
{
    public static class Constants
    {
        public static int MAX_QUEUE_LENGTH = 2 * Environment.ProcessorCount;
        public const int WORK_BUFFER_LENGTH = 50;
    }
}