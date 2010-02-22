using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim;

namespace Experiments
{
    [Flags]
    public enum ExpType
    {
        All = 1,
        NonPipelinedBlockInverse = 2,
        Parallel = 4,
        SingleThreaded = 8,
        SingleThreadedTiled = 16,
        OnlyRunMaxProcessorTests = 32,
        TileSizesInArgument = 64
    }

    [Flags]
    public enum ExpSubType
    {
        All = 1,
        BlockInverse = 2,
        Inverse = 4,
        LUFactorize = 8,
        MinusPlusPlus = 16,
        Multiply = 32,
        MinusMatrixInverseMatrixMultiply = 64,
        PlusMultiply = 128
    }

    public class MeasurementHelpers
    {
        private static TraceSource Log = new TraceSource("App");

        public static void TestPackageExecuter(ExpType type, string fileName, ExpSubType subtype)
        {
            if ((type & ExpType.NonPipelinedBlockInverse) == ExpType.NonPipelinedBlockInverse ||
                (type & ExpType.All) == ExpType.All)
                SaveToCSVFile(Perform(MeasurementPackages.ParallelNonPipelined.BlockInverse()), fileName + "ParallelNonPipelined.BlockInverse.csv");

            if ((type & ExpType.Parallel) == ExpType.Parallel || (type & ExpType.All) == ExpType.All)
            {
                // parallel measurements
                if ((subtype & ExpSubType.BlockInverse) == ExpSubType.BlockInverse || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.Parallel.BlockInverse()), fileName + "Parallel.BlockInverse.csv");
                if ((subtype & ExpSubType.Inverse) == ExpSubType.Inverse || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.Parallel.Inverse()), fileName + "Parallel.Inverse.csv");
                if ((subtype & ExpSubType.LUFactorize) == ExpSubType.LUFactorize || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.Parallel.LUFactorize()), fileName + "Parallel.LUFactorize.csv");
                if ((subtype & ExpSubType.MinusPlusPlus) == ExpSubType.MinusPlusPlus || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.Parallel.MinusPlusPlus()), fileName + "Parallel.MinusPlusPlus.csv");
                if ((subtype & ExpSubType.Multiply) == ExpSubType.Multiply || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.Parallel.Multiply()), fileName + "Parallel.Multiply.csv");
                if ((subtype & ExpSubType.MinusMatrixInverseMatrixMultiply) == ExpSubType.MinusMatrixInverseMatrixMultiply || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.Parallel.MinusMatrixInverseMatrixMultiply()), fileName + "Parallel.MinusMatrixInverseMatrixMultiply.csv");
                if ((subtype & ExpSubType.PlusMultiply) == ExpSubType.PlusMultiply || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.Parallel.PlusMultiply()), fileName + "Parallel.PlusMultiply.csv");
            }

            if ((type & ExpType.SingleThreaded) == ExpType.SingleThreaded || (type & ExpType.All) == ExpType.All)
            {
                // SingleThreaded measurements
                if ((subtype & ExpSubType.BlockInverse) == ExpSubType.BlockInverse || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.SingleThreaded.BlockInverse()), fileName + "SingleThreaded.BlockInverse.csv");
                if ((subtype & ExpSubType.Inverse) == ExpSubType.Inverse || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.SingleThreaded.Inverse()), fileName + "SingleThreaded.Inverse.csv");
                if ((subtype & ExpSubType.LUFactorize) == ExpSubType.LUFactorize || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.SingleThreaded.LUFactorize()), fileName + "SingleThreaded.LUFactorize.csv");
                if ((subtype & ExpSubType.MinusPlusPlus) == ExpSubType.MinusPlusPlus || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.SingleThreaded.MinusPlusPlus()), fileName + "SingleThreaded.MinusPlusPlus.csv");
                if ((subtype & ExpSubType.Multiply) == ExpSubType.Multiply || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.SingleThreaded.Multiply()), fileName + "SingleThreaded.Multiply.csv");
                if ((subtype & ExpSubType.MinusMatrixInverseMatrixMultiply) == ExpSubType.MinusMatrixInverseMatrixMultiply || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.SingleThreaded.MinusMatrixInverseMatrixMultiply()), fileName + "SingleThreaded.MinusMatrixInverseMatrixMultiply.csv");
                if ((subtype & ExpSubType.PlusMultiply) == ExpSubType.PlusMultiply || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.SingleThreaded.PlusMultiply()), fileName + "SingleThreaded.PlusMultiply.csv");
            }

            if ((type & ExpType.SingleThreadedTiled) == ExpType.SingleThreadedTiled || (type & ExpType.All) == ExpType.All)
            {
                // SingleThreadedTiled measurements
                if ((subtype & ExpSubType.BlockInverse) == ExpSubType.BlockInverse || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.SingleThreadedTiled.BlockInverse()), fileName + "SingleThreadedTiled.BlockInverse.csv");
                if ((subtype & ExpSubType.Inverse) == ExpSubType.Inverse || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.SingleThreadedTiled.Inverse()), fileName + "SingleThreadedTiled.Inverse.csv");
                if ((subtype & ExpSubType.LUFactorize) == ExpSubType.LUFactorize || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.SingleThreadedTiled.LUFactorize()), fileName + "SingleThreadedTiled.LUFactorize.csv");
                if ((subtype & ExpSubType.MinusPlusPlus) == ExpSubType.MinusPlusPlus || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.SingleThreadedTiled.MinusPlusPlus()), fileName + "SingleThreadedTiled.MinusPlusPlus.csv");
                if ((subtype & ExpSubType.Multiply) == ExpSubType.Multiply || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.SingleThreadedTiled.Multiply()), fileName + "SingleThreadedTiled.Multiply.csv");
                if ((subtype & ExpSubType.MinusMatrixInverseMatrixMultiply) == ExpSubType.MinusMatrixInverseMatrixMultiply || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.SingleThreadedTiled.MinusMatrixInverseMatrixMultiply()), fileName + "SingleThreadedTiled.MinusMatrixInverseMatrixMultiply.csv");
                if ((subtype & ExpSubType.PlusMultiply) == ExpSubType.PlusMultiply || (subtype & ExpSubType.All) == ExpSubType.All)
                    SaveToCSVFile(Perform(MeasurementPackages.SingleThreadedTiled.PlusMultiply()), fileName + "SingleThreadedTiled.PlusMultiply.csv");
            }
        }

        public static void SaveToCSVFile(IList<Measurement> results, string fileName)
        {
            Log.TraceInformation("Writing file: {0}", fileName);
            const string seperator = "\t";

            using (var file = new StreamWriter(fileName, false /* overwrite exiting file */))
            {
                WriteTable(results, file, seperator, (m) => m.Elapsed, "Running time");
                WriteTable(results, file, seperator, (m) => m.Statistics["WaitCount"], "Number of times a thread has waited for work");
                WriteTable(results, file, seperator, (m) => m.Statistics["WorkDoneCount"], "Number of times a thread has completed a computational kernel");
                WriteTable(results, file, seperator, (m) => m.Statistics["SecondaryProducerCount"], "Number of times a thread has received work from a secondary producer (in pipelined mode)");
                WriteTable(results, file, seperator, (m) => m.Statistics["FailedWaitCount"] + "," + m.Statistics["FailedWaitCountWhenComplete"], "Number of times threads have waited to long for a pulse / waited to long for a pulse when producer was completed");
            }
        }

        public static void WriteTable(IList<Measurement> results, StreamWriter file, string seperator, Func<Measurement, object> getData, string tableName)
        {
            file.WriteLine();
            file.WriteLine(tableName);

            // write out the row with just the tile sizes (starting with an empty column)
            file.Write(seperator);
            foreach (var tileSize in results.Select(x => x.TileSize).Distinct().OrderBy(x => x))
            {
                file.Write(tileSize);
                file.Write(seperator);
            }
            file.WriteLine();

            var groupedResults = results.GroupBy(x => x.ThreadCount).Select(x => new { ThreadCount = x.Key, Results = x.OrderBy(y => y.TileSize) });
            foreach (var resultRun in groupedResults)
            {
                file.Write(resultRun.ThreadCount);
                file.Write(seperator);
                foreach (var result in resultRun.Results)
                {
                    file.Write(getData(result));
                    file.Write(seperator);
                }
                file.WriteLine();
            }
        }

        public static IList<Measurement> Perform(IEnumerable<Measurement> measurements)
        {
            Stopwatch sw = new Stopwatch();
            IList<Measurement> results = new List<Measurement>();
            foreach (var measurement in measurements)
            {
                Log.TraceEvent(TraceEventType.Start, 0, "Messurement with ThreadCount {0}, TileSize {1}",
                               measurement.ThreadCount, measurement.TileSize);

                sw.Reset();

                measurement.Init();

                sw.Start();
                measurement.Run();
                sw.Stop();

                measurement.CleanUp();

                measurement.Elapsed = sw.Elapsed;

                measurement.Statistics["WaitCount"] = Statistics.GlobalWaitCount.Sum(x => x.Value);
                measurement.Statistics["WorkDoneCount"] = Statistics.GlobalWorkDoneCount.Sum(x => x.Value);
                measurement.Statistics["SecondaryProducerCount"] = Statistics.GlobalSecondaryProducerCount.Sum(x => x.Value);
                measurement.Statistics["FailedWaitCount"] = Statistics.GlobalFailedWaitCount;
                measurement.Statistics["FailedWaitCountWhenComplete"] = Statistics.GlobalFailedWaitCountWhenComplete;
                Statistics.Reset();

                results.Add(measurement);

                Log.TraceEvent(TraceEventType.Stop, 0, "Messurement, elapsed time {0}", measurement.Elapsed);
            }
            return results;
        }        
    }

    public abstract class Measurement
    {

        protected Measurement() { Statistics = new Dictionary<string, int>(); }
        protected Measurement(int tileSize, int threadCount) : this()
        {
            TileSize = tileSize; ThreadCount = threadCount;
        }

        public abstract void Init();
        public virtual void CleanUp()
        {
            //Force garbage collection.
            GC.Collect();

            // Wait for all finalizers to complete before continuing.
            // Without this call to GC.WaitForPendingFinalizers, 
            // the worker loop below might execute at the same time 
            // as the finalizers.
            // With this call, the worker loop executes only after
            // all finalizers have been called.
            GC.WaitForPendingFinalizers();
        }

        public abstract void Run();

        public TimeSpan Elapsed { get; set; }
        public int TileSize { get; set; }
        public int ThreadCount { get; set; }
        public Dictionary<string, int> Statistics { get; set; }
    }

    public class BlockTridiagonalMatrixMeasurement : Measurement
    {
        protected BlockTridiagonalMatrix<double> BTM { get; set; }
        public Action<BlockTridiagonalMatrix<double>, int, int> Execute { get; set; }

        public override void Init()
        {
            BTM = MeasurementDataSets.BTM;
        }

        public override void CleanUp()
        {
            BTM = null;
            base.CleanUp();
        }

        public override void Run()
        {
            Execute(BTM, TileSize, ThreadCount);
        }
    }

    public class OneBlockMatrixMeasurement : Measurement
    {
        protected Matrix<Matrix<double>> Block1 { get; set; }
        public Action<Matrix<Matrix<double>>, int> Execute { get; set; }

        public override void Init()
        {
            Block1 = MeasurementDataSets.Tile(MeasurementDataSets.Matrix1, TileSize);
        }

        public override void CleanUp()
        {
            Block1 = null;
            base.CleanUp();
        }

        public override void Run()
        {
            Execute(Block1, ThreadCount);
        }
    }

    public class OneMatrixMeasurement : Measurement
    {
        protected Matrix<double> Matrix1 { get; set; }
        public Action<Matrix<double>> Execute { get; set; }

        public override void Init()
        {
            Matrix1 = MeasurementDataSets.Matrix1;
        }

        public override void CleanUp()
        {
            Matrix1 = null;
            base.CleanUp();
        }

        public override void Run()
        {
            Execute(Matrix1);
        }
    }

    public class TwoMatrixMeasurement : OneMatrixMeasurement
    {
        protected Matrix<double> Matrix2 { get; set; }
        public new Action<Matrix<double>, Matrix<double>> Execute { get; set; }

        public override void Init()
        {
            Matrix2 = MeasurementDataSets.Matrix2;
            base.Init();
        }

        public override void CleanUp()
        {
            Matrix2 = null;
            base.CleanUp();
        }

        public override void Run()
        {
            Execute(Matrix1, Matrix2);
        }
    }

    public class ThreeMatrixMeasurement : TwoMatrixMeasurement
    {
        protected Matrix<double> Matrix3 { get; set; }
        public new Action<Matrix<double>, Matrix<double>, Matrix<double>> Execute { get; set; }

        public override void Init()
        {            
            Matrix3 = MeasurementDataSets.Matrix3;
            base.Init();
        }

        public override void CleanUp()
        {
            Matrix3 = null;
            base.CleanUp();
        }

        public override void Run()
        {
            Execute(Matrix1, Matrix2, Matrix3);
        }
    }

    public class TwoBlockMatrixMeasurement : OneBlockMatrixMeasurement
    {
        protected Matrix<Matrix<double>> Block2 { get; set; }
        public new Action<Matrix<Matrix<double>>, Matrix<Matrix<double>>, int> Execute { get; set; }

        public override void Init()
        {
            Block2 = MeasurementDataSets.Tile(MeasurementDataSets.Matrix2, TileSize);
            base.Init();
        }

        public override void CleanUp()
        {
            Block2 = null;
            base.CleanUp();
        }

        public override void Run()
        {
            Execute(Block1, Block2, ThreadCount);
        }
    }

    public class ThreeBlockMatrixMeasurement : TwoBlockMatrixMeasurement
    {
        protected Matrix<Matrix<double>> Block3 { get; set; }
        public new Action<Matrix<Matrix<double>>, Matrix<Matrix<double>>, Matrix<Matrix<double>>, int> Execute { get; set; }

        public override void Init()
        {
            Block3 = MeasurementDataSets.Tile(MeasurementDataSets.Matrix3, TileSize);
            base.Init();
        }

        public override void CleanUp()
        {
            Block3 = null;
            base.CleanUp();
        }

        public override void Run()
        {
            Execute(Block1, Block2, Block3, ThreadCount);
        }
    }

}