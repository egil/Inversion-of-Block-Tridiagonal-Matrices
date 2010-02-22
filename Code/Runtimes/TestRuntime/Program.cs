using System;
using System.Collections.Generic;
using System.Diagnostics;
using TestHelpers;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.Math.MatrixOperations;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.Enumerators;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.MatrixOperations;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.OperationResults;
using TiledMatrixInversion.Resources;
using Slim = TiledMatrixInversion.ParallelBlockMatrixInverterSlim;

namespace TestRuntime
{
    class Program
    {
        static void Main(string[] args)
        {
            var tileSize = 40;

            var d1 =
                Matrix<double>.DeSerializeFromFile(
                    @"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\m1500x1500-a.mat");

            var d2 =
                Matrix<double>.DeSerializeFromFile(
                    @"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\m1500x1500-b.mat");

            // prepare data
            var data1 = MatrixHelpers.Tile(d1, tileSize);
            var data2 = MatrixHelpers.Tile(d2, tileSize);

            var opData1 = new OperationResult<double>(data1);
            var opData2 = new OperationResult<double>(data2.GetLU());
            OperationResult<double> actual;
            var mProducer = new MinusMatrixInverseMatrixMultiply<double>(opData1, opData2, out actual);
            var pm = new Manager(mProducer);
            pm.Start();
            pm.Join();

            NotNaNOrInfinity(actual.Data);

            ////foreach (var rowActionGenerator in AbstractOperationGenerator(5, 5))
            ////{
            ////    AbstractOperation op;
            ////    while (!rowActionGenerator.Completed)
            ////    {
            ////        op = rowActionGenerator.Find((x) => true);
            ////        Console.WriteLine(op);
            ////    }
            ////    Console.WriteLine();
            //////}

            //for (int i = 1; i <= 5; i++)
            //{
            //    foreach (var s in G_ColumnAbstractActionGenerator(i, 5))
            //    {
            //        Console.WriteLine(s);
            //    }
            //    Console.WriteLine();
            //}

        }

        private static void NotNaNOrInfinity(Matrix<double> actual)
        {
            for (int i = 1; i <= actual.Rows; i++)
            {
                for (int j = 1; j <= actual.Columns; j++)
                {
                    if (double.IsNaN(actual[i, j])) Console.WriteLine("Actual is NaN: Position [{0}, {1}].", i, j);
                    if (double.IsInfinity(actual[i, j])) Console.WriteLine("Actual is IsInfinity: Position [{0}, {1}].", i, j);
                }
            }
        }

        public static void NotNaNOrInfinity(Matrix<Matrix<double>> actual)
        {
            for (int i = 1; i <= actual.Rows; i++)
            {
                for (int j = 1; j <= actual.Columns; j++)
                {
                    NotNaNOrInfinity(actual[i, j], string.Format("Block [{0}, {1}]", i, j));
                }
            }
        }

        private static void NotNaNOrInfinity(Matrix<double> actual, string message)
        {
            for (int i = 1; i <= actual.Rows; i++)
            {
                for (int j = 1; j <= actual.Columns; j++)
                {
                    if (double.IsNaN(actual[i, j])) Console.WriteLine("Actual is NaN: Position [{0}, {1}]. {2}", i, j, message);
                    if (double.IsInfinity(actual[i, j])) Console.WriteLine("Actual is IsInfinity: Position [{0}, {1}]. {2}", i, j, message);
                }
            }
        }

        private static IEnumerable<AbstractOperation<OpType>> F_ColumnAbstractActionGenerator(int j, int N)
        {
            for (int step = 2 * (j - 1) + 1; step <= 2 * (N - 1) + 1; step++)
            {
                int sweep = System.Math.Max(j - 1, step - N);
                for (int i = System.Math.Min(step - (j - 1), N); i >= step / 2 + 1; i--)
                {
                    sweep++;
                    if (i == sweep)
                    {
                        yield return new AbstractOperation<OpType>(i, j, sweep - (j - 1), OpType.F);
                    }
                    else
                    {
                        yield return new AbstractOperation<OpType>(i, j, sweep - (j - 1) - 1, OpType.R);
                    }
                }
            }
        }


        private static IEnumerable<AbstractOperation<OpType>> G_ColumnAbstractActionGenerator(int j, int N)
        {
            for (int step = 1; step <= 2 * (N - 1) + 1; step++)
            {
                int sweep = System.Math.Max(0, step - N);
                for (int i = System.Math.Max(N - (step - 1), 1); i <= N - (step / 2); i++)
                {
                    sweep++;
                    if (i == N - (sweep - 1))
                    {
                        yield return new AbstractOperation<OpType>(i, j, sweep, OpType.G);
                    }
                    else
                    {
                        yield return new AbstractOperation<OpType>(i, j, sweep - 1, OpType.H);
                    }
                }
            }
        }
        enum OpType
        {
            F,
            R,
            G,
            /// <summary>
            /// H refers to F in the right side of figure 9
            /// </summary>
            H
        }











        //private static IEnumerable<OperationEnumerator<AbstractOperation<OpType>>> AbstractOperationGenerator(int M, int N)
        //{
        //    for (int i = 1; i <= M; i++)
        //    {
        //        yield return new OperationEnumerator<AbstractOperation<OpType>>(B_RowActionGenerator(i, N), Constants.MAX_QUEUE_LENGTH);
        //    }

        //    for (int i = 1; i <= M; i++)
        //    {
        //        yield return new OperationEnumerator<AbstractOperation<OpType>>(C_RowActionGenerator(i, N), Constants.MAX_QUEUE_LENGTH);
        //    }
        //}

        //private static IEnumerable<AbstractOperation<OpType>> B_RowActionGenerator(int i, int N)
        //{
        //    for (int step = 1; step <= 2 * (N - 1) + 1; step++)
        //    {
        //        int sweep = Math.Max(0, step - N);
        //        for (int j = Math.Min(step, N); j >= step / 2 + 1; j--)
        //        {
        //            sweep++;
        //            if (j == sweep)
        //            {
        //                // { OP = OpType.Bb, I = i, J = j, K = j };
        //                yield return new AbstractOperation<OpType>(i, j, j, OpType.Bb);
        //            }
        //            else
        //            {
        //                // { OP = OpType.A, I = i, J = j, K = sweep };
        //                yield return new AbstractOperation<OpType>(i, j, sweep, OpType.A);
        //            }
        //        }
        //    }
        //}

        //private static IEnumerable<AbstractOperation<OpType>> C_RowActionGenerator(int i, int N)
        //{
        //    for (int step = 1; step <= 2 * (N - 1) + 1; step++)
        //    {
        //        int sweep = Math.Max(0, step - N);
        //        for (int j = Math.Max(N - (step - 1), 1); j <= N - (step / 2); j++)
        //        {
        //            sweep++;
        //            if (j == N - (sweep - 1))
        //            {
        //                yield return new AbstractOperation<OpType>(i, j, j, OpType.C);
        //            }
        //            else
        //            {
        //                yield return new AbstractOperation<OpType>(i, j, sweep, OpType.Bc);
        //            }
        //        }
        //    }
        //}

        //private static IEnumerable<AbstractOperation<OpType>> C_RowActionGenerator(int i, int N)
        //{
        //    // this does not generate the order described in figure 7.
        //    for (int k = 0; k <= N - 2; k++)
        //    {
        //        yield return new AbstractOperation<OpType>(i, N - k, k, OpType.C); // { OP = OpType.C, I = i, J = N - k, K = k };
        //        for (int j = N - 1 - k; j >= 1; j--)
        //        {
        //            yield return new AbstractOperation<OpType>(i, j, k + 1, OpType.Bc); // { OP = OpType.Bc, I = i, J = j, K = k + 1 };
        //        }
        //    }
        //    yield return new AbstractOperation<OpType>(i, 1, N - 1, OpType.C); // { OP = OpType.C, I = i, J = 1, K = N - 1 };
        //}

        //enum OpType
        //{
        //    A,
        //    /// <summary>
        //    /// This OpType describes the operation performed on line 3 and 8 in figure 6, the calculation of
        //    /// b = a*u^-1
        //    /// </summary>
        //    Bb,
        //    /// <summary>
        //    /// This OpType describes the operation performed on line 5 in figure 6, the calculation of
        //    /// c = b*l^-1
        //    /// </summary>
        //    Bc,
        //    C
        //}
    }
}
