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

            string fileName = "result-";
            MeasurementDataSets.ReadFromDisk = true;
            if ((subtype & ExpSubType.BlockInverse) == ExpSubType.BlockInverse)
            {
                MeasurementDataSets.BTMFileName = args[2];
                fileName += MeasurementDataSets.BTMFileName + "-";
            }
            else
            {
                MeasurementDataSets.Matrix1FileName = args[2];
                MeasurementDataSets.Matrix2FileName = args.Length > 3 ? args[3] : string.Empty;
                MeasurementDataSets.Matrix3FileName = args.Length > 4 ? args[4] : string.Empty;
                fileName += MeasurementDataSets.Matrix1FileName + "-";
            }

            if ((type & ExpType.TileSizesInArgument) == ExpType.TileSizesInArgument)
            {
                var ts = args[args.Length - 1];
                var tss = ts.Split(',');
                var parsedTss = tss.Select(x => int.Parse(x));
                MeasurementPackages.TileSizeGenerator = parsedTss;
            }

            //Console.WriteLine("FUCKED MED PROCCESSOR COUNT");
            //MeasurementPackages.ProcessorCount = 4;

            MeasurementPackages.OnlyRunMaxProcessorCount = (type & ExpType.OnlyRunMaxProcessorTests) == ExpType.OnlyRunMaxProcessorTests;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Log.TraceEvent(TraceEventType.Start, 0, "Experiment");           

            MeasurementHelpers.TestPackageExecuter(type, fileName, subtype);

            sw.Stop();
            Log.TraceEvent(TraceEventType.Stop, 0, "Experiment, total elapsed time = {0}", sw.Elapsed);
            Console.WriteLine("Experiment, total elapsed time = {0}", sw.Elapsed);
        }

    }
}
