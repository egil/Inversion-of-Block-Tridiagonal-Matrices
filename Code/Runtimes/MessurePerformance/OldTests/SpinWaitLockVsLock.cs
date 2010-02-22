using System;
using System.IO;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.Math.MatrixOperations;
using TiledMatrixInversion.ParallelBlockMatrixInverter;
using Slim = TiledMatrixInversion.ParallelBlockMatrixInverterSlim;
using TiledMatrixInversion.Performance;

namespace MessurePerformance
{
    public static class SpinWaitLockVsLock
    {
        static SpinWaitLockVsLock()
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
            var sf = new StigsFormulae<double>(btm, tileSize, out result);
            var nonpl = new NonPipelinedStigsFormulae(sf);
            var pm = new ProcessManager(nonpl, threads);
            pm.Start();
            pm.Join();
        }

        public static void Pipelined(MessureContext context)
        {
            var tileSize = (int)context.Data["tilesize"];
            var threads = (int)context.Data["threads"];
            var btm = (BlockTridiagonalMatrix<double>)context.Data["btm"];

            BlockTridiagonalMatrix<double> result;
            var sf = new StigsFormulae<double>(btm, tileSize, out result);
            var nonpl = new PipelinedStigsFormulae(sf);
            var pm = new ProcessManager(nonpl, threads);
            pm.Start();
            pm.Join();
        }

        public static void NonPipelinedSlim(MessureContext context)
        {
            var tileSize = (int)context.Data["tilesize"];
            var threads = (int)context.Data["threads"];
            var btm = (BlockTridiagonalMatrix<double>)context.Data["btm"];

            BlockTridiagonalMatrix<double> result;
            var sf = new Slim.BlockTridiagonalMatrixInverse<double>(btm, tileSize, out result);
            var nonpl = new Slim.NonPipelinedBlockTridiagonalMatrixInverse(sf);
            var pm = new Slim.Manager(nonpl, threads);
            pm.Start();
            pm.Join();
        }

        public static void PipelinedSlim(MessureContext context)
        {
            var tileSize = (int)context.Data["tilesize"];
            var threads = (int)context.Data["threads"];
            var btm = (BlockTridiagonalMatrix<double>)context.Data["btm"];

            BlockTridiagonalMatrix<double> result;
            var sf = new Slim.BlockTridiagonalMatrixInverse<double>(btm, tileSize, out result);
            var nonpl = new Slim.PipelinedBlockTridiagonalMatrixInverse(sf);
            var pm = new Slim.Manager(nonpl, threads);
            pm.Start();
            pm.Join();
        }

    }
}