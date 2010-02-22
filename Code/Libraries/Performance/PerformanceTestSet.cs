using System;
using System.Collections.Generic;

namespace TiledMatrixInversion.Performance
{
    public abstract class PerformanceTestSet<TTestContext> where TTestContext : new()
    {
        protected PerformanceTestSet()
        {
            Context = new TTestContext();
        }

        public TTestContext Context { get; protected set; }

        public virtual void TestSetup()
        {            
        }

        public virtual void TestTeardown()
        {            
        }

        public virtual void TestClassSetup()
        {
        }

        public virtual void TestClassTeardown()
        {
        }        
    }
}