using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using TiledMatrixInversion.BlockMatrixInverter;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim;
using System.Linq;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.MatrixOperations;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.OperationResults;

namespace MessurePerformance
{
    /// <summary>
    /// 2 45 "C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\ds100x100x300.dat"
    /// </summary>
    class Dummy
    {
        public Dummy()
        {

        }
    }

    internal static class ParallelTiledMatrixOperation<T>
    {
        static void Runner(IProducer<Action> producer, int threadCount)
        {
            var pm = new Manager(producer, threadCount);
            pm.Start();
            pm.Join();
        }

        public static void LUFactorization(MessureContext<T> profile)
        {
            var opRes1 = new OperationResult<T>(profile.A);

            OperationResult<T> actual;
            var producer = new LUFactorization<T>(opRes1, out actual);
            Runner(producer, profile.ThreadCount);
        }

        public static void Multiply(MessureContext<T> profile)
        {
            var opRes1 = new OperationResult<T>(profile.A);
            var opRes2 = new OperationResult<T>(profile.B);

            OperationResult<T> actual;
            var producer = new Multiply<T>(opRes1, opRes2, out actual);
            Runner(producer, profile.ThreadCount);
        }

        public static void MinusPlusPlus(MessureContext<T> profile)
        {
            var opRes1 = new OperationResult<T>(profile.A);
            var opRes2 = new OperationResult<T>(profile.B);
            var opRes3 = new OperationResult<T>(profile.C);

            OperationResult<T> actual;
            var producer = new MinusPlusPlus<T>(opRes1, opRes2, opRes3, out actual);
            Runner(producer, profile.ThreadCount);
        }

        public static void PlusMultiply(MessureContext<T> profile)
        {
            var opRes1 = new OperationResult<T>(profile.A);
            var opRes2 = new OperationResult<T>(profile.B);
            var opRes3 = new OperationResult<T>(profile.C);

            OperationResult<T> actual;
            var producer = new PlusMultiply<T>(opRes1, opRes2, opRes3, out actual);
            Runner(producer, profile.ThreadCount);
        }

        public static void MatrixInverse(MessureContext<T> profile)
        {
            var opRes1 = new OperationResult<T>(profile.A);
            
            OperationResult<T> actual;
            var producer = new Inverse<T>(opRes1, out actual);
            Runner(producer, profile.ThreadCount);
        }

        public static void MinusMatrixInverseMatrixMultiply(MessureContext<T> profile)
        {
            var opRes1 = new OperationResult<T>(profile.A);
            var opRes2 = new OperationResult<T>(profile.B);

            OperationResult<T> actual;
            var producer = new MinusMatrixInverseMatrixMultiply<T>(opRes1, opRes2, out actual);
            Runner(producer, profile.ThreadCount);
        }

        public static void BlockMatrixInverse(MessureContext<T> profile)
        {
            BlockTridiagonalMatrix<T> result;
            var sf = new BlockTridiagonalMatrixInverse<T>(profile.BTM, profile.TileSize, out result);
            var producer = new PipelinedBlockTridiagonalMatrixInverse(sf);
            Runner(producer, profile.ThreadCount);
        }
    }

    internal static class SingleThreadedTiledMatrixOperation<T>
    {
        public static void LUFactorization(MessureContext<T> profile)
        {
            var res = profile.A.GetLU();
        }

        public static void Multiply(MessureContext<T> profile)
        {
            var a = profile.A;
            var b = profile.B;
            var res = a * b;
        }

        public static void MinusPlusPlus(MessureContext<T> profile)
        {
            var res = profile.A.MinusPlusPlus(profile.B, profile.C);
        }

        public static void PlusMultiply(MessureContext<T> profile)
        {
            var res = profile.A.PlusMultiply(profile.B, profile.C);
        }

        public static void MatrixInverse(MessureContext<T> profile)
        {
            var res = profile.A.Inverse();
        }

        public static void MinusMatrixInverseMatrixMultiply(MessureContext<T> profile)
        {
            var res = profile.A.MinusMatrixInverseMatrixMultiply(profile.B);
        }

        public static void BlockMatrixInverse(MessureContext<T> profile)
        {
            var tbtm = profile.BTM.Tile(profile.TileSize);
            var inverter = new TiledSingleThreadedBlockMatrixInverter<T>();
            inverter.Invert(tbtm);
        }


    }

    internal class MessureContext<T>
    {
        internal Matrix<Matrix<T>> A { get; set; }
        internal Matrix<Matrix<T>> B { get; set; }
        internal Matrix<Matrix<T>> C { get; set; }
        internal BlockTridiagonalMatrix<T> BTM { get; set; }

        internal int TileSize { get; set; }
        internal int ThreadCount { get; set; }
    }
}

