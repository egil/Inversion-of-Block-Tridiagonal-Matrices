using System;
using System.Collections.Generic;
using TiledMatrixInversion.BlockMatrixInverter;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.MatrixOperations;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.OperationResults;

namespace Experiments
{
    public static class MeasurementPackages
    {
        private static int _processorCount = Environment.ProcessorCount;

        public static int ProcessorCount
        {
            get
            {
                return _processorCount;
            }
            set
            {
                _processorCount = value;
            }
        }

        static MeasurementPackages()
        {
            TileSizeGenerator = DefaultTileSizeGenerator();
        }

        public static bool OnlyRunMaxProcessorCount { get; set; }
        public static IEnumerable<int> TileSizeGenerator { get; set; }

        private static IEnumerable<int> DefaultTileSizeGenerator()
        {            
            for (int tileSize = 10; tileSize <= 100; tileSize += 10)
                yield return tileSize;
            yield return 150;
            //for (int tileSize = 25; tileSize <= 40; tileSize += 5)
            //    yield return tileSize;
            //for (int tileSize = 50; tileSize <= 80; tileSize += 10)
            //    yield return tileSize;
        }

        private static IEnumerable<int> ThreadCountGenerator()
        {
            if (OnlyRunMaxProcessorCount)
                return new[] { _processorCount };
            if (_processorCount < 2)
                return new[] { 1 };
            if (_processorCount == 2)
                return new[] { 1, 2 };
            if (_processorCount == 4)
                return new[] { 1, 2, 4 };
            if (_processorCount >= 8)
                return new[] { 1, 2, 4, 6, 8 };
            throw new NotSupportedException("Unsupported number of processors");
        }

        #region Nested type: Parallel

        public static class ParallelNonPipelined
        {
            private static void Runner(IProducer<Action> producer, int threadCount)
            {
                var pm = new Manager(producer, threadCount);
                pm.Start();
                pm.Join();
            }
            public static IEnumerable<Measurement> BlockInverse()
            {
                foreach (int threadCount in ThreadCountGenerator())
                    foreach (int tileSize in TileSizeGenerator)
                        yield return new BlockTridiagonalMatrixMeasurement
                        {
                            TileSize = tileSize,
                            ThreadCount = threadCount,
                            Execute = (btm, ts, tc) =>
                            {
                                BlockTridiagonalMatrix<double> result;
                                var sf = new BlockTridiagonalMatrixInverse<double>(btm, ts, out result);
                                var producer = new NonPipelinedBlockTridiagonalMatrixInverse(sf);
                                Runner(producer, tc);
                            }
                        };
            }
        }

        public static class Parallel
        {
            private static void Runner(IProducer<Action> producer, int threadCount)
            {
                var pm = new Manager(producer, threadCount);
                pm.Start();
                pm.Join();
            }

            public static IEnumerable<Measurement> BlockInverse()
            {
                foreach (int threadCount in ThreadCountGenerator())
                    foreach (int tileSize in TileSizeGenerator)
                        yield return new BlockTridiagonalMatrixMeasurement
                        {
                            TileSize = tileSize,
                            ThreadCount = threadCount,
                            Execute = (btm, ts, tc) =>
                            {
                                BlockTridiagonalMatrix<double> result;
                                var sf = new BlockTridiagonalMatrixInverse<double>(btm, ts, out result);
                                var producer = new PipelinedBlockTridiagonalMatrixInverse(sf);
                                Runner(producer, tc);
                            }
                        };
            }

            public static IEnumerable<Measurement> LUFactorize()
            {
                foreach (int threadCount in ThreadCountGenerator())
                    foreach (int tileSize in TileSizeGenerator)
                        yield return new OneBlockMatrixMeasurement
                        {
                            TileSize = tileSize,
                            ThreadCount = threadCount,
                            Execute = (a, tc) =>
                            {
                                var opRes1 = new OperationResult<double>(a);

                                OperationResult<double> actual;
                                var producer = new LUFactorization<double>(opRes1,
                                                                           out actual);
                                Runner(producer, tc);
                            }
                        };
            }

            public static IEnumerable<Measurement> Multiply()
            {
                foreach (int threadCount in ThreadCountGenerator())
                    foreach (int tileSize in TileSizeGenerator)
                        yield return new TwoBlockMatrixMeasurement
                        {
                            TileSize = tileSize,
                            ThreadCount = threadCount,
                            Execute = (a, b, tc) =>
                            {
                                var opRes1 = new OperationResult<double>(a);
                                var opRes2 = new OperationResult<double>(b);

                                OperationResult<double> actual;
                                var producer = new Multiply<double>(opRes1, opRes2,
                                                                    out actual);
                                Runner(producer, tc);
                            }
                        };
            }

            public static IEnumerable<Measurement> MinusPlusPlus()
            {
                foreach (int threadCount in ThreadCountGenerator())
                    foreach (int tileSize in TileSizeGenerator)
                        yield return new ThreeBlockMatrixMeasurement
                        {
                            TileSize = tileSize,
                            ThreadCount = threadCount,
                            Execute = (a, b, c, tc) =>
                            {
                                var opRes1 = new OperationResult<double>(a);
                                var opRes2 = new OperationResult<double>(b);
                                var opRes3 = new OperationResult<double>(c);

                                OperationResult<double> actual;
                                var producer = new MinusPlusPlus<double>(opRes1, opRes2,
                                                                         opRes3,
                                                                         out actual);
                                Runner(producer, tc);
                            }
                        };
            }

            public static IEnumerable<Measurement> PlusMultiply()
            {
                foreach (int threadCount in ThreadCountGenerator())
                    foreach (int tileSize in TileSizeGenerator)
                        yield return new ThreeBlockMatrixMeasurement
                        {
                            TileSize = tileSize,
                            ThreadCount = threadCount,
                            Execute = (a, b, c, tc) =>
                            {
                                var opRes1 = new OperationResult<double>(a);
                                var opRes2 = new OperationResult<double>(b);
                                var opRes3 = new OperationResult<double>(c);

                                OperationResult<double> actual;
                                var producer = new PlusMultiply<double>(opRes1, opRes2,
                                                                        opRes3,
                                                                        out actual);
                                Runner(producer, tc);
                            }
                        };
            }

            public static IEnumerable<Measurement> MinusMatrixInverseMatrixMultiply()
            {
                foreach (int threadCount in ThreadCountGenerator())
                    foreach (int tileSize in TileSizeGenerator)
                        yield return new TwoBlockMatrixMeasurement
                        {
                            TileSize = tileSize,
                            ThreadCount = threadCount,
                            Execute = (a, b, tc) =>
                            {
                                var opRes1 = new OperationResult<double>(a);
                                var opRes2 = new OperationResult<double>(b);

                                OperationResult<double> actual;
                                var producer =
                                    new MinusMatrixInverseMatrixMultiply<double>(
                                        opRes1, opRes2, out actual);
                                Runner(producer, tc);
                            }
                        };
            }

            public static IEnumerable<Measurement> Inverse()
            {
                foreach (int threadCount in ThreadCountGenerator())
                    foreach (int tileSize in TileSizeGenerator)
                        yield return new OneBlockMatrixMeasurement
                        {
                            TileSize = tileSize,
                            ThreadCount = threadCount,
                            Execute = (a, tc) =>
                            {
                                var opRes1 = new OperationResult<double>(a);

                                OperationResult<double> actual;
                                var producer = new Inverse<double>(opRes1, out actual);
                                Runner(producer, tc);
                            }
                        };
            }
        }

        #endregion

        #region Nested type: SingleThreaded

        public static class SingleThreaded
        {
            public static IEnumerable<Measurement> BlockInverse()
            {
                yield return new BlockTridiagonalMatrixMeasurement
                {
                    Execute = (btm, ts, _) =>
                    {
                        var inverter =
                            new SingleThreadedBlockMatrixInverter<double>();
                        inverter.Invert(btm);
                    }
                };
            }

            public static IEnumerable<Measurement> LUFactorize()
            {
                yield return new OneMatrixMeasurement
                {
                    Execute = (a) => { Matrix<double> res = a.GetLU(); }
                };
            }

            public static IEnumerable<Measurement> Multiply()
            {
                yield return new TwoMatrixMeasurement
                {
                    Execute = (a, b) => { Matrix<double> res = a * b; }
                };
            }

            public static IEnumerable<Measurement> MinusPlusPlus()
            {
                yield return new ThreeMatrixMeasurement
                {
                    Execute = (a, b, c) => { Matrix<double> res = a.MinusPlusPlus(b, c); }
                };
            }

            public static IEnumerable<Measurement> PlusMultiply()
            {
                yield return new ThreeMatrixMeasurement
                {
                    Execute = (a, b, c) => { Matrix<double> res = a.PlusMultiply(b, c); }
                };
            }

            public static IEnumerable<Measurement> MinusMatrixInverseMatrixMultiply()
            {
                yield return new TwoMatrixMeasurement
                {
                    Execute =
                        (a, b) => { Matrix<double> res = a.MinusMatrixInverseMatrixMultiply(b); }
                };
            }

            public static IEnumerable<Measurement> Inverse()
            {
                yield return new OneMatrixMeasurement
                {
                    Execute = (a) => { Matrix<double> res = a.Inverse(); }
                };
            }
        }

        #endregion

        #region Nested type: SingleThreadedTiled

        public static class SingleThreadedTiled
        {
            public static IEnumerable<Measurement> BlockInverse()
            {
                foreach (int tileSize in TileSizeGenerator)
                {
                    yield return new BlockTridiagonalMatrixMeasurement
                    {
                        TileSize = tileSize,
                        Execute = (btm, ts, _) =>
                        {
                            TiledBlockTridiagonalMatrix<double> tbtm = btm.Tile(ts);
                            var inverter =
                                new TiledSingleThreadedBlockMatrixInverter<double>();
                            inverter.Invert(tbtm);
                        }
                    };
                }
            }

            public static IEnumerable<Measurement> LUFactorize()
            {
                foreach (int tileSize in TileSizeGenerator)
                {
                    yield return new OneBlockMatrixMeasurement
                    {
                        TileSize = tileSize,
                        Execute = (a, _) => { Matrix<Matrix<double>> res = a.GetLU(); }
                    };
                }
            }

            public static IEnumerable<Measurement> Multiply()
            {
                foreach (int tileSize in TileSizeGenerator)
                {
                    yield return new TwoBlockMatrixMeasurement
                    {
                        TileSize = tileSize,
                        Execute = (a, b, _) => { Matrix<Matrix<double>> res = a * b; }
                    };
                }
            }

            public static IEnumerable<Measurement> MinusPlusPlus()
            {
                foreach (int tileSize in TileSizeGenerator)
                {
                    yield return new ThreeBlockMatrixMeasurement
                    {
                        TileSize = tileSize,
                        Execute =
                            (a, b, c, _) => { Matrix<Matrix<double>> res = a.MinusPlusPlus(b, c); }
                    };
                }
            }

            public static IEnumerable<Measurement> PlusMultiply()
            {
                foreach (int tileSize in TileSizeGenerator)
                {
                    yield return new ThreeBlockMatrixMeasurement
                    {
                        TileSize = tileSize,
                        Execute =
                            (a, b, c, _) => { Matrix<Matrix<double>> res = a.PlusMultiply(b, c); }
                    };
                }
            }

            public static IEnumerable<Measurement> MinusMatrixInverseMatrixMultiply()
            {
                foreach (int tileSize in TileSizeGenerator)
                {
                    yield return new TwoBlockMatrixMeasurement
                    {
                        TileSize = tileSize,
                        Execute =
                            (a, b, _) => { Matrix<Matrix<double>> res = a.MinusMatrixInverseMatrixMultiply(b); }
                    };
                }
            }

            public static IEnumerable<Measurement> Inverse()
            {
                foreach (int tileSize in TileSizeGenerator)
                {
                    yield return new OneBlockMatrixMeasurement
                    {
                        TileSize = tileSize,
                        Execute = (a, _) => { Matrix<Matrix<double>> res = a.Inverse(); }
                    };
                }
            }
        }

        #endregion
    }
}