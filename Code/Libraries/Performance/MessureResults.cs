using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace TiledMatrixInversion.Performance
{
    
    [Serializable]
    public class MessureResults
    {
        [NonSerialized]
        private readonly Stopwatch watch = new Stopwatch();
        [NonSerialized]
        private readonly List<Action> _testQueue = new List<Action>();
        private readonly List<MessureResult> _results = new List<MessureResult>();

        #region properties
        
        public bool Collecting { get; protected set; }
        public DateTimeOffset StartDateTime { get; protected set; }
        public DateTimeOffset StopDateTime { get; protected set; }        
        public string Description { get { return Context.Description; } }
        public MessureContext Context { get; protected set;}

        public ReadOnlyCollection<MessureResult> Results
        {
            get { return _results.AsReadOnly(); }
        }

        #endregion

        internal MessureResults(MessureContext context)
        {
            Collecting = !context.CollectAndWait;
            Context = context;
            Context.AutoSaveFileName = string.IsNullOrEmpty(Context.AutoSaveFileName)
                                           ? string.Format("{0} - {1:yyyy-MM-dd hh_mm_ss}.pmr", Description, DateTimeOffset.Now)
                                           : Context.AutoSaveFileName;

            if(!Context.CollectAndWait)
            {
                StartDateTime = DateTimeOffset.Now;
            }
            
        }

        #region Load/Save methods

        public static MessureResults LoadFromFile(string fileName)
        {
            using (var fileStream = new FileStream(Path.GetFullPath(fileName), FileMode.Open, FileAccess.Read))
            {
                var binFormatter = new BinaryFormatter();
                return (MessureResults)binFormatter.Deserialize(fileStream);
            }
        }

        public static void SaveToFile(MessureResults mrs, string fileName)
        {
            mrs.Stop();
            using (var fileStream = new FileStream(Path.GetFullPath(fileName), FileMode.Create, FileAccess.Write))
            {
                var binFormatter = new BinaryFormatter();
                binFormatter.Serialize(fileStream, mrs);
            }
        }

        #endregion

        #region helpers

        static string GetTestMethodFullName(Action<MessureContext> test)
        {
            return test.Method.DeclaringType.FullName + "." + test.Method.Name;
        }

        static string GetTestMethodName(Action<MessureContext> test)
        {
            return test.Method.Name;
        }


        void RaiseExceptionIfNotCollecting()
        {
            if(!Collecting)
            {
                throw new InvalidOperationException("MessureResults is not collecting, messuring is not allowed with this instance.");
            }
        }

        #endregion

        public MessureResults SaveToFile(string fileName)
        {
            SaveToFile(this, fileName);
            return this;
        }

        public MessureResults Start()
        {
            if (!Collecting && Context.CollectAndWait)
            {                
                Collecting = true;
                StartDateTime = DateTimeOffset.Now;

                for (int i = 0; i < Context.Replays; i++)
                {
                    foreach (var test in _testQueue)
                    {
                        test();
                    }
                }
                Stop();
            }
            return this;
        }

        public MessureResults Stop()
        {
            if(Collecting)
            {
                StopDateTime = DateTimeOffset.Now;
                Collecting = false;   

                if(Context.AutoSaveToFile)
                {
                    SaveToFile(Context.AutoSaveFileName);
                }
            }
            return this;
        }

        public MessureResults Messure(Action<MessureContext> test)
        {
            // if CollectAndWait is true, the user must explicitly call Start to start messuring
            if (!Collecting && Context.CollectAndWait)
            {
                _testQueue.Add(() => Messure(test));
                return this;
            }
            // test if Stop has been called
            RaiseExceptionIfNotCollecting();

            // begin performance messurement
            watch.Reset();
            Context.Reset(Context);

            Console.WriteLine("[Start]\t\t" + GetTestMethodName(test));
            watch.Start();

            test(Context);

            watch.Stop();
            Console.WriteLine("[Finished in]\t" + watch.Elapsed);
            Console.WriteLine();

            // add result to results collection
            _results.Add(new MessureResult { Time = watch.Elapsed, Name = GetTestMethodName(test), FullName = GetTestMethodFullName(test) });

            return this;
        }
        
        public MessureResults SpeedUp(Action<MessureContext> test)
        {
            // if CollectAndWait is true, the user must explicitly call Start to start messuring
            if (!Collecting && Context.CollectAndWait)
            {
                _testQueue.Add(() => SpeedUp(test));
                return this;
            }
            // test if Stop has been called
            RaiseExceptionIfNotCollecting();
            
            // begin performance messurement
            Messure(test);

            if (Results.Count > 1)
            {
                var res1 = Results[Results.Count - 2];
                var res2 = Results[Results.Count - 1];
                var speedup = res1.Time.TotalMilliseconds / res2.Time.TotalMilliseconds;
                Console.WriteLine("SpeedUp is: " + speedup);
            }
            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(Description);

            for (int i = 0; i < _results.Count; i++)
            {
                sb.AppendLine(_results[i].ToString());
            }

            sb.AppendLine();
            sb.AppendLine("*** SPEED UP COMPARISON ***");

            foreach (var result in _results)
            {
                sb.AppendLine(result.Name);

                foreach (var vs in _results)
                {
                    if(result!=vs)
                    {
                        sb.AppendLine("\t" + vs.Name + ": " + vs.Time.TotalMilliseconds / result.Time.TotalMilliseconds);       
                    }                    
                }
            }

            return sb.ToString();
        }
    }
}
