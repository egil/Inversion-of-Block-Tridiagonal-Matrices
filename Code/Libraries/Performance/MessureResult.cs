using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TiledMatrixInversion.Performance
{
    [Serializable]
    public class MessureResult
    {
        public TimeSpan Time { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public override string ToString()
        {
            return Name + ": " + Time;
        }


    }
}
