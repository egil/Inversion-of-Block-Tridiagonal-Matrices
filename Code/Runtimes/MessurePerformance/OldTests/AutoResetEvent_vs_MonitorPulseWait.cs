using System;
using System.IO;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.Math.MatrixOperations;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim;
using TiledMatrixInversion.Performance;
using NonSlim = TiledMatrixInversion.ParallelBlockMatrixInverter;

namespace MessurePerformance
{
    public static class AutoResetEventVsMonitorPulseWait
    {
        static AutoResetEventVsMonitorPulseWait()
        {
            Matrix<double>.MatrixOperations = new DoubleMatrixOperations();
            Matrix<Matrix<double>>.MatrixOperations = new TiledMatrixOperations<double>();          
        }

        public static MessureResults Start(int threads, int tilesize, string filename)
        {

            var mc = new MessureContext
            {
                Description = "Matrix Inversion - AutoResetEvent vs Monitor Pulse-Wait",
                CollectAndWait = true,
                Replays = 1,
                AutoSaveToFile = true,
                Data = { { "tilesize", tilesize }, { "threads", threads} },
                Reset = (x) =>
                            {
                                GC.Collect();
                                x.Data["btm"] = BlockTridiagonalMatrix<double>.DeSerializeFromFile(Path.GetFullPath(filename));
                            }
            };

            Console.WriteLine(mc);

            var m = Messure.Init(mc)
                .Messure(NonPipelined)
                .Messure(NonPipelinedSlim)
                .Messure(Pipelined)
                .Messure(PipelinedSlim)
                .Start();

            return m;
        }

        public static void NonPipelined(MessureContext context)
        {
            var tileSize = (int)context.Data["tilesize"];
            var threads = (int) context.Data["threads"];
            var btm = (BlockTridiagonalMatrix<double>)context.Data["btm"];

            BlockTridiagonalMatrix<double> result;
            var sf = new NonSlim.StigsFormulae<double>(btm, tileSize, out result);
            var nonpl = new NonSlim.NonPipelinedStigsFormulae(sf);
            var pm = new NonSlim.ProcessManager(nonpl, threads);
            pm.Start();
            pm.Join();
        }

        public static void Pipelined(MessureContext context)
        {
            var tileSize = (int)context.Data["tilesize"];
            var threads = (int)context.Data["threads"];
            var btm = (BlockTridiagonalMatrix<double>)context.Data["btm"];

            BlockTridiagonalMatrix<double> result;
            var sf = new NonSlim.StigsFormulae<double>(btm, tileSize, out result);
            var nonpl = new NonSlim.PipelinedStigsFormulae(sf);
            var pm = new NonSlim.ProcessManager(nonpl, threads);
            pm.Start();
            pm.Join();
        }

        public static void NonPipelinedSlim(MessureContext context)
        {
            var tileSize = (int)context.Data["tilesize"];
            var threads = (int)context.Data["threads"];
            var btm = (BlockTridiagonalMatrix<double>)context.Data["btm"];

            BlockTridiagonalMatrix<double> result;
            var sf = new BlockTridiagonalMatrixInverse<double>(btm, tileSize, out result);
            var nonpl = new NonPipelinedBlockTridiagonalMatrixInverse(sf);
            var pm = new Manager(nonpl, threads);
            pm.Start();
            pm.Join();
        }

        public static void PipelinedSlim(MessureContext context)
        {
            var tileSize = (int)context.Data["tilesize"];
            var threads = (int)context.Data["threads"];
            var btm = (BlockTridiagonalMatrix<double>)context.Data["btm"];

            BlockTridiagonalMatrix<double> result;
            var sf = new BlockTridiagonalMatrixInverse<double>(btm, tileSize, out result);
            var nonpl = new PipelinedBlockTridiagonalMatrixInverse(sf);
            var pm = new Manager(nonpl, threads);
            pm.Start();
            pm.Join();
        }

    }
}