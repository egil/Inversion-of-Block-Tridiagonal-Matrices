using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TiledMatrixInversion.Performance
{
    public static class Messure
    {
        public static MessureResults Init()
        {
            return Init(string.Empty);
        }
        public static MessureResults Init(string description)
        {
            return Init(new MessureContext { Description = description });
        }
        public static MessureResults Init(MessureContext context)
        {
            return new MessureResults(context);
        }
    }
}
