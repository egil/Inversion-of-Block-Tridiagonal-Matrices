using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim;

namespace Experiments
{
    class Program
    {
        private static TraceSource Log = new TraceSource("App");     

        static void Main(string[] args)
        {
            ExpType type = ExpType.All;
            if (args.Length > 0) type = (ExpType)Enum.Parse(typeof(ExpType), args[0]);

            ExpSubType subtype = ExpSubType.All;
            if (args.Length > 1) subtype = (ExpSubType)Enum.Parse(typeof(ExpSubType), args[1]);

            if (args.Length > 2) MeasurementDataSets.Rows = int.Parse(args[2]);
            if (args.Length > 3) MeasurementDataSets.Columns = int.Parse(args[3]);
            if (args.Length > 4) MeasurementDataSets.BtmSize = int.Parse(args[4]);
            if (args.Length > 5) MeasurementDataSets.BtmMinBlockSize = int.Parse(args[5]);
            if (args.Length > 6) MeasurementDataSets.BtmMaxBlockSize = int.Parse(args[6]);

            //Console.WriteLine("FUCKED MED PROCCESSOR COUNT");
            //MeasurementPackages.ProcessorCount = 4;

            MeasurementPackages.OnlyRunMaxProcessorCount = (type & ExpType.OnlyRunMaxProcessorTests) == ExpType.OnlyRunMaxProcessorTests;

            if((type & ExpType.TileSizesInArgument) == ExpType.TileSizesInArgument)
            {
                var ts = args[args.Length - 1];
                var tss = ts.Split(',');
                var parsedTss = tss.Select(x => int.Parse(x));
                MeasurementPackages.TileSizeGenerator = parsedTss;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Log.TraceEvent(TraceEventType.Start, 0, "Experiment");
            //string fileName = args.Length > 2 ? args[2] : "result-";
            string fileName = "result-";

            MeasurementHelpers.TestPackageExecuter(type, fileName, subtype);

            sw.Stop();
            Log.TraceEvent(TraceEventType.Stop, 0, "Experiment, total elapsed time = {0}", sw.Elapsed);
            Console.WriteLine("Experiment, total elapsed time = {0}", sw.Elapsed);
        }

    }
}
