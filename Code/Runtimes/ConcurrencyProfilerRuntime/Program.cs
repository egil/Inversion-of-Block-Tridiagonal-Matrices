using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim;

namespace ConcurrencyProfilerRuntime
{
    class Program
    {
        private static TraceSource Log = new TraceSource("App");
        private static int ThreadCount;
        private static int TileSize;
        private static string SourceFile;

        static void Main(string[] args)
        {
            if (!(args.Length > 0 && int.TryParse(args[0], out ThreadCount)))
            {
                Console.Write("Enter Thread Count (default {0}): ", Environment.ProcessorCount);
                var tc = Console.ReadLine();
                if (!int.TryParse(tc, out ThreadCount))
                    ThreadCount = Environment.ProcessorCount;
            }

            if (!(args.Length > 1 && int.TryParse(args[1], out TileSize)))
            {
                Console.Write("Enter TileSize (default {0}): ", 30);
                var tc = Console.ReadLine();
                if (!int.TryParse(tc, out TileSize))
                    TileSize = 30;
            }

            SourceFile = args.Length > 2 ? args[2] : @"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\ds100x100x300.dat";

            Log.TraceEvent(TraceEventType.Start, 0, "MessurePerformance");
            Log.TraceEvent(TraceEventType.Information, 0, "ProcessorCount = {0}", Environment.ProcessorCount);
            Log.TraceEvent(TraceEventType.Information, 0, "ThreadCount = {0}", ThreadCount);
            Log.TraceEvent(TraceEventType.Information, 0, "TileSize = {0}", TileSize);
            Log.TraceEvent(TraceEventType.Information, 0, "SourceFile = {0}", SourceFile);

            Stopwatch sw = new Stopwatch();

            var tileSize = TileSize;
            var threads = ThreadCount;
            var btm = BlockTridiagonalMatrix<double>.DeSerializeFromFile(SourceFile);

            sw.Start();

            BlockTridiagonalMatrix<double> result;
            var sf = new StigsFormulae<double>(btm, tileSize, out result);
            var nonpl = new NonPipelinedStigsFormulae(sf);
            var pm = new ProcessManagerSlim(nonpl, threads);
            pm.Start();
            pm.Join();

            sw.Stop();

            Log.TraceEvent(TraceEventType.Information, 0, "Running time: {0}", sw.Elapsed);

            //Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            //Debug.AutoFlush = true;

            //Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

            //AutoResetEventVsMonitorPulseWait.Start(ThreadCount, TileSize, args[2]);

            //var testData = @"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\ds3x100x100.dat";
            //var resultFile = @"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\ds3x100x100-res.csv";

            //var tileSize10To30Step5 = new TestParameter<string, int>("TileSizes", 10, 30, x => x + 5);
            //var tileSize40To60Step10 = new TestParameter<string, int>("TileSizes", 40, 60, x => x + 10);
            //var threads = new TestParameter<string, int>("ThreadCounts", new[] { 1, 2, 4, 6, 8 });

            //using (var btmTestSet = new BtmInversionPerformanceTestSet(testData, resultFile, threads, tileSize40To60Step10, tileSize10To30Step5))
            //{
            //    btmTestSet.Execute();
            //}

            Log.TraceEvent(TraceEventType.Stop, 0, "MessurePerformance");
            foreach (TraceListener listener in Trace.Listeners)
            {
                listener.Flush();
            }
        }
    }
}
