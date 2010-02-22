using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim;
using System.Linq;

namespace MessurePerformance
{
    /// <summary>
    /// 2 45 "C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\ds100x100x300.dat"
    /// </summary>
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

            SourceFile = args.Length > 2 ? args[2] : ChooseDataFile();

            int modeId;
            Console.WriteLine("Please choose mode");
            Console.WriteLine("\t1) Run in pipelined mode");
            Console.WriteLine("\t2) Run in non pipelined mode");
            bool asPipelined = int.TryParse(Console.ReadLine(), out modeId) && modeId == 1;

            Log.TraceEvent(TraceEventType.Start, 0, "MessurePerformance");
            Log.TraceEvent(TraceEventType.Information, 0, "ProcessorCount = {0}", Environment.ProcessorCount);
            Log.TraceEvent(TraceEventType.Information, 0, "ThreadCount = {0}", ThreadCount);
            Log.TraceEvent(TraceEventType.Information, 0, "TileSize = {0}", TileSize);
            Log.TraceEvent(TraceEventType.Information, 0, "SourceFile = {0}", SourceFile);
            Log.TraceEvent(TraceEventType.Information, 0, "Mode = {0}", asPipelined ? "Pipelined" : "Non Pipelined");

            Stopwatch sw = new Stopwatch();

            var tileSize = TileSize;
            var threads = ThreadCount;
            var btm = BlockTridiagonalMatrix<double>.DeSerializeFromFile(SourceFile);

            sw.Start();

            BlockTridiagonalMatrix<double> result;
            var sf = new BlockTridiagonalMatrixInverse<double>(btm, tileSize, out result);
            var pm = asPipelined
                         ? new Manager(new PipelinedBlockTridiagonalMatrixInverse(sf), threads)
                         : new Manager(new NonPipelinedBlockTridiagonalMatrixInverse(sf), threads); 
            pm.Start();
            pm.Join();

            sw.Stop();

            foreach (var kpv in Statistics.GlobalWaitCount) Log.TraceInformation("Wait Count: {0} = {1}", kpv.Key, kpv.Value);
            foreach (var kpv in Statistics.GlobalWorkDoneCount) Log.TraceInformation("Work Done: {0} = {1}", kpv.Key, kpv.Value);
            foreach (var kpv in Statistics.GlobalSecondaryProducerCount) Log.TraceInformation("Second Producer Count: {0} = {1}", kpv.Key, kpv.Value);

            Log.TraceEvent(TraceEventType.Information, 0, "Running time: {0}", sw.Elapsed);
            Log.TraceEvent(TraceEventType.Stop, 0, "MessurePerformance");
            foreach (TraceListener listener in Trace.Listeners)
            {
                listener.Flush();
            }
        }

        static string ChooseDataFile()
        {
            Console.WriteLine("Please select a data file to use:");
            Console.WriteLine();

            var dir = new DirectoryInfo(ConfigurationManager.AppSettings["DataSetDirectory"] ?? Environment.CurrentDirectory);
            var dataFiles = dir.GetFiles("*.btm").OrderBy(x => x.Name).ToArray();

            for (int i = 0; i < dataFiles.Length; i++)
            {
                Console.WriteLine("\t{0}) {1}", i, dataFiles[i].Name);
            }

            int index;

            while (!(int.TryParse(Console.ReadLine(), out index) && index >= 0 && index < dataFiles.Length))
            {
                Console.WriteLine("Invalid input, please try again... ");
            }

            return dataFiles[index].FullName;
        }

    }
}

