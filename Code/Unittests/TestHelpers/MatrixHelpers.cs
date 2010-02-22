using Microsoft.VisualStudio.TestTools.UnitTesting;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.OperationResults;

namespace TestHelpers
{
    public static class MatrixHelpers
    {
        #region tiling / untiling

        public static Matrix<T> Untile<T>(Matrix<Matrix<T>> matrix)
        {
            var tbtm = new TiledBlockTridiagonalMatrix<T>(1);
            tbtm[1, 1] = matrix;
            var btm = tbtm.Untile();
            return btm[1, 1];
        }

        public static Matrix<Matrix<T>> Tile<T>(Matrix<T> matrix, int tileSize)
        {
            var btm = new BlockTridiagonalMatrix<T>(1);
            btm[1, 1] = matrix;
            var tbtm = btm.Tile(tileSize);
            return tbtm[1, 1];
        }

        #endregion

        #region compare btm

        public static void Compare<T>(BlockTridiagonalMatrix<T> expected, BlockTridiagonalMatrix<T> actual)
        {
            Assert.AreEqual(expected.Size, actual.Size, "BTM size mismatch.");

            for (int i = 1; i <= expected.Size; i++)
            {
                if (i > 1)
                {
                    Compare(expected[i, i - 1], actual[i, i - 1]);
                }

                Compare(expected[i, i], actual[i, i]);

                if (i < expected.Size)
                {
                    Compare(expected[i, i + 1], actual[i, i + 1]);
                }
            }
        }

        public static void Compare(BlockTridiagonalMatrix<double> expected, BlockTridiagonalMatrix<double> actual, double delta)
        {
            Assert.AreEqual(expected.Size, actual.Size, "BTM size mismatch.");

            for (int i = 1; i <= expected.Size; i++)
            {
                if (i > 1)
                {
                    Compare(expected[i, i - 1], actual[i, i - 1], delta);
                }

                Compare(expected[i, i], actual[i, i], delta);

                if (i < expected.Size)
                {
                    Compare(expected[i, i + 1], actual[i, i + 1], delta);
                }
            }
        }

        #endregion

        #region compare Matrix<T>

        public static void CompareDimensions<T>(Matrix<T> expected, Matrix<T> actual)
        {
            Assert.AreEqual(expected.Rows, actual.Rows, "Number of rows in Matrix<T> do not match.");
            Assert.AreEqual(expected.Columns, actual.Columns, "Number of columns in Matrix<T> do not match.");
        }

        public static void CompareDimensions<T>(Matrix<Matrix<T>> expected, Matrix<Matrix<T>> actual)
        {
            Assert.AreEqual(expected.Rows, actual.Rows, "Number of rows in Matrix<Matrix<T>> do not match.");
            Assert.AreEqual(expected.Columns, actual.Columns, "Number of columns in Matrix<Matrix<T>> do not match.");

            for (int i = 1; i <= expected.Rows; i++)
            {
                for (int j = 1; j <= expected.Columns; j++)
                {
                    Assert.AreEqual(expected[i, j].Rows, actual[i, j].Rows, "Number of rows in Matrix<T> do not match in block [{0}, {1}]", i, j);
                    Assert.AreEqual(expected[i, j].Columns, actual[i, j].Columns, "Number of columns in Matrix<T> do not match in block [{0}, {1}]", i, j);
                }
            }
        }

        public static void Compare<T>(Matrix<T> expected, Matrix<T> actual)
        {
            Compare(expected, actual, string.Empty);
        }
        public static void Compare<T>(Matrix<T> expected, Matrix<T> actual, string message)
        {
            Assert.AreEqual(expected.Rows, actual.Rows, "Rows size mismatch.");
            Assert.AreEqual(expected.Columns, actual.Columns, "Columns size mismatch.");

            for (int i = 1; i <= expected.Rows; i++)
            {
                for (int j = 1; j <= expected.Columns; j++)
                {
                    Assert.AreEqual(expected[i, j], actual[i, j], "Position [{0}, {1}]. {2}", i, j, message);
                }
            }
        }

        public static void Compare<T>(Matrix<Matrix<T>> expected, Matrix<Matrix<T>> actual)
        {
            Assert.AreEqual(expected.Rows, actual.Rows, "Number of rows in Matrix<Matrix<T>> do not match.");
            Assert.AreEqual(expected.Columns, actual.Columns, "Number of columns in Matrix<Matrix<T>> do not match.");

            for (int i = 1; i <= expected.Rows; i++)
            {
                for (int j = 1; j <= expected.Columns; j++)
                {
                    Assert.AreEqual(expected[i, j].Rows, actual[i, j].Rows, "Number of rows in Matrix<T> do not match in block [{0}, {1}]", i, j);
                    Assert.AreEqual(expected[i, j].Columns, actual[i, j].Columns, "Number of columns in Matrix<T> do not match in block [{0}, {1}]", i, j);

                    Compare(expected[i, j], actual[i, j], string.Format("Block [{0}, {1}]", i, j));
                }
            }
        }

        #endregion

        #region compare Matrix<double>

        public static void Compare(Matrix<double> expected, Matrix<double> actual, double delta)
        {
            Compare(expected, actual, delta, string.Empty);
        }
        public static void Compare(Matrix<double> expected, Matrix<double> actual, double delta, string message)
        {
            Assert.AreEqual(expected.Rows, actual.Rows, "Rows size mismatch.");
            Assert.AreEqual(expected.Columns, actual.Columns, "Columns size mismatch.");

            for (int i = 1; i <= expected.Rows; i++)
            {
                for (int j = 1; j <= expected.Columns; j++)
                {
                    Assert.IsFalse(double.IsNaN(actual[i, j]), "Actual is NaN: Position [{0}, {1}]. {2}", i, j, message);
                    Assert.IsFalse(double.IsInfinity(actual[i, j]), "Actual is Infinity: Position [{0}, {1}]. {2}", i, j, message);
                    Assert.IsFalse(double.IsNaN(expected[i, j]), "Expected is NaN: Position [{0}, {1}]. {2}", i, j, message);
                    Assert.IsFalse(double.IsInfinity(expected[i, j]), "Expected is Infinity: Position [{0}, {1}]. {2}", i, j, message);
                    Assert.AreEqual(expected[i, j], actual[i, j], delta, "Position [{0}, {1}]. {2}", i, j, message);
                }
            }
        }

        public static void Compare(Matrix<Matrix<double>> expected, Matrix<Matrix<double>> actual, double delta)
        {
            Assert.AreEqual(expected.Rows, actual.Rows, "Number of rows in Matrix<Matrix<T>> do not match.");
            Assert.AreEqual(expected.Columns, actual.Columns, "Number of columns in Matrix<Matrix<T>> do not match.");

            for (int i = 1; i <= expected.Rows; i++)
            {
                for (int j = 1; j <= expected.Columns; j++)
                {
                    Assert.AreEqual(expected[i, j].Rows, actual[i, j].Rows, "Number of rows in Matrix<T> do not match in block [{0}, {1}]", i, j);
                    Assert.AreEqual(expected[i, j].Columns, actual[i, j].Columns, "Number of columns in Matrix<T> do not match in block [{0}, {1}]", i, j);

                    Compare(expected[i, j], actual[i, j], delta, string.Format("Block [{0}, {1}]", i, j));
                }
            }
        }

        #endregion

        #region NotNaNOrInfinity

        public static void NotNaNOrInfinity(BlockTridiagonalMatrix<double> actual)
        {
            for (int i = 1; i <= actual.Size; i++)
            {
                if (i > 1)
                {
                    NotNaNOrInfinity(actual[i, i - 1], string.Format("Block [{0}, {1}]", i, i - 1));
                }

                NotNaNOrInfinity(actual[i, i], string.Format("Block [{0}, {1}]", i, i));

                if (i < actual.Size)
                {
                    NotNaNOrInfinity(actual[i, i + 1], string.Format("Block [{0}, {1}]", i, i + 1));
                }
            }
        }

        public static void NotNaNOrInfinity(Matrix<double> actual)
        {
            NotNaNOrInfinity(actual, string.Empty);
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
                    Assert.IsFalse(double.IsNaN(actual[i, j]), "Actual is NaN: Position [{0}, {1}]. {2}", i, j, message);
                    Assert.IsFalse(double.IsInfinity(actual[i, j]), "Actual is Infinity: Position [{0}, {1}]. {2}", i, j, message);
                }
            }
        }

        #endregion

        #region Diff tester

        public static void Diff(Matrix<double> expected, Matrix<double> actual)
        {
            Diff(expected, actual, 0.0);
        }

        public static void Diff(Matrix<double> expected, Matrix<double> actual, double delta)
        {
            Diff(expected, actual, delta, string.Empty);
        }

        public static void Diff(Matrix<double> expected, Matrix<double> actual, double delta, string message)
        {
            var diff = 0.0;
            int diffi = 0, diffj = 0;
            for (int i = 1; i <= expected.Rows; i++)
            {
                for (int j = 1; j <= expected.Columns; j++)
                {
                    var x = expected[i, j] - actual[i, j];
                    if (x > diff)
                    {
                        diff = x;
                        diffi = i;
                        diffj = j;
                    }
                }
            }

            Assert.IsTrue(diff <= delta, "Diff {0} is greater than delta {1} at position [{2}, {3}] {4}", diff, delta, diffi, diffj, message);
        }

        public static void Diff(Matrix<Matrix<double>> expected, Matrix<Matrix<double>> actual)
        {
            Diff(expected, actual, 0.0);
        }

        public static void Diff(Matrix<Matrix<double>> expected, Matrix<Matrix<double>> actual, double delta)
        {
            Diff(expected, actual, delta, string.Empty);
        }

        public static void Diff(Matrix<Matrix<double>> expected, Matrix<Matrix<double>> actual, double delta, string message)
        {
            var diff = 0.0;
            int diffi = 0, diffj = 0, diffk = 0, diffl = 0;
            for (int i = 1; i <= expected.Rows; i++)
            {
                for (int j = 1; j <= expected.Columns; j++)
                {
                    for (int k = 1; k <= expected[i, j].Rows; k++)
                    {
                        for (int l = 1; l <= expected[i, j].Columns; l++)
                        {
                            var x = expected[i, j][k, l] - actual[i, j][k, l];
                            if (x > diff)
                            {
                                diff = x;
                                diffi = i;
                                diffj = j;
                                diffk = k;
                                diffl = l;
                            }
                        }
                    }
                }
            }

            Assert.IsTrue(diff <= delta, "Diff {0} is greater than delta {1} in block [{2}, {3}] at position [{4}, {5}] {6}",
                          diff, delta, diffi, diffj, diffk, diffl, message);
        }

        public static void Diff(BlockTridiagonalMatrix<double> expected, BlockTridiagonalMatrix<double> actual, double delta)
        {
            Assert.AreEqual(expected.Size, actual.Size, "BTM size mismatch.");

            for (int i = 1; i <= expected.Size; i++)
            {
                if (i > 1)
                {
                    Diff(expected[i, i - 1], actual[i, i - 1], delta,
                         string.Format("in TBTM block [{0}, {1}]", i, i - 1));
                }

                Diff(expected[i, i], actual[i, i], delta, string.Format("in TBTM block [{0}, {1}]", i, i));

                if (i < expected.Size)
                {
                    Diff(expected[i, i + 1], actual[i, i + 1], delta, string.Format("in TBTM block [{0}, {1}]", i, i + 1));
                }
            }
        }

        #endregion

        public static void IsDone<T>(OperationResult<T> actual)
        {
            Assert.IsTrue(actual.Completed, "Actual was not marked completed.");
            for (int i = 1; i <= actual.Rows; i++)
            {
                for (int j = 1; j <= actual.Columns; j++)
                {
                    Assert.IsTrue(actual[i, j], "Tile [{0}, {1}].", i, j);
                }
            }
        }
    }
}
