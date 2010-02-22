using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.Math.MatrixOperations;

namespace TiledMatrixInversion.Tests.MathTests
{
    /// <summary>
    ///This is a test class for BlockTridiagonalMatrixTest and is intended
    ///to contain all BlockTridiagonalMatrixTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BlockTridiagonalMatrixTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            Matrix<double>.MatrixOperations = new DoubleMatrixOperations();
            Matrix<Matrix<double>>.MatrixOperations = new TiledMatrixOperations<double>();
        }
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for Size
        ///</summary>
        [TestMethod()]
        public void BlockTridiagonalMatrix_SizeTest()
        {
            var size = 5;
            var target = new BlockTridiagonalMatrix<double>(size);
            var actual = target.Size;
            Assert.AreEqual(size, actual);
        }

        [TestMethod()]
        public void BlockTridiagonalMatrix_GetSetTest()
        {
            var btm = new BlockTridiagonalMatrix<double>(3);
            var m1 = new Matrix<double>(1, 1);
            var m2 = new Matrix<double>(1, 1);
            var m3 = new Matrix<double>(1, 1);
            var m4 = new Matrix<double>(1, 1);
            var m5 = new Matrix<double>(1, 1);
            var m6 = new Matrix<double>(1, 1);
            var m7 = new Matrix<double>(1, 1);
            btm[1, 1] = m1;
            btm[1, 2] = m2;
            btm[2, 1] = m3;
            btm[2, 2] = m4;
            btm[2, 3] = m5;
            btm[3, 2] = m6;
            btm[3, 3] = m7;
            Assert.AreEqual(m1, btm[1, 1]);
            Assert.AreEqual(m2, btm[1, 2]);
            Assert.AreEqual(m3, btm[2, 1]);
            Assert.AreEqual(m4, btm[2, 2]);
            Assert.AreEqual(m5, btm[2, 3]);
            Assert.AreEqual(m6, btm[3, 2]);
            Assert.AreEqual(m7, btm[3, 3]);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BlockTridiagonalMatrix_InvalidGetTest()
        {
            var btm = new BlockTridiagonalMatrix<double>(3);
            var x = btm[1, 3];
        }


        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BlockTridiagonalMatrix_InvalidSetTest()
        {
            var btm = new BlockTridiagonalMatrix<double>(3);
            btm[1, 3] = new Matrix<double>(1, 1);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BlockTridiagonalMatrix_InvalidGetTest2()
        {
            var btm = new BlockTridiagonalMatrix<double>(3);
            var x = btm[3, 1];
        }


        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BlockTridiagonalMatrix_InvalidSetTest2()
        {
            var btm = new BlockTridiagonalMatrix<double>(3);
            btm[3, 1] = new Matrix<double>(1, 1);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BlockTridiagonalMatrix_InvalidGetTest3()
        {
            var btm = new BlockTridiagonalMatrix<double>(3);
            var x = btm[2, 4];
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BlockTridiagonalMatrix_InvalidSetTest3()
        {
            var btm = new BlockTridiagonalMatrix<double>(3);
            btm[3, 4] = new Matrix<double>(1, 1);
        }


        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BlockTridiagonalMatrix_InvalidGetTest4()
        {
            var btm = new BlockTridiagonalMatrix<double>(3);
            var x = btm[1, 0];
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BlockTridiagonalMatrix_InvalidSetTest4()
        {
            var btm = new BlockTridiagonalMatrix<double>(3);
            btm[1, 0] = new Matrix<double>(1, 1);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BlockTridiagonalMatrix_InvalidGetTest5()
        {
            var btm = new BlockTridiagonalMatrix<double>(3);
            var x = btm[3, 4];
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BlockTridiagonalMatrix_InvalidSetTest5()
        {
            var btm = new BlockTridiagonalMatrix<double>(3);
            btm[3, 4] = new Matrix<double>(1, 1);
        }

        [TestMethod()]
        public void BlockTridiagonalMatrix_TileTest1()
        {
            var btm = new BlockTridiagonalMatrix<double>(1);
            var m = new Matrix<double>(6, 6);

            for (int i = 1; i <= 6; i++)
            {
                for (int j = 1; j <= 6; j++)
                {
                    m[i, j] = (i - 1) * 6 + j;
                }
            }

            btm[1, 1] = m;

            var tileSize = 2;
            var tiledBtm = btm.Tile(tileSize);
            var block = tiledBtm[1, 1];

            Assert.AreEqual(3, block.Columns, "Unexpected amount of columns");
            Assert.AreEqual(3, block.Rows, "Unexpected amount of rows");

            for (int i = 1; i <= block.Rows; i++)
            {
                for (int j = 1; j <= block.Columns; j++)
                {
                    var tile = block[i, j];
                    Assert.AreEqual(2, tile.Columns, "Unexpected amount of columns");
                    Assert.AreEqual(2, tile.Rows, "Unexpected amount of rows");

                    for (int k = 1; k <= tile.Rows; k++)
                    {
                        for (int l = 1; l <= tile.Columns; l++)
                        {
                            Assert.AreEqual(m[(i - 1) * tileSize + k, (j - 1) * tileSize + l], tile[k, l],
                                            string.Format("i = {0}, j = {1}, k = {2}, l = {3}", i, j, k, l));
                        }
                    }
                }
            }
        }

        [TestMethod()]
        public void BlockTridiagonalMatrix_TileTest2()
        {
            var btm = new BlockTridiagonalMatrix<double>(1);
            var m = new Matrix<double>(6, 6);

            for (int i = 1; i <= 6; i++)
            {
                for (int j = 1; j <= 6; j++)
                {
                    m[i, j] = (i - 1) * 6 + j;
                }
            }

            btm[1, 1] = m;

            var tileSize = 4;
            var tiledBtm = btm.Tile(tileSize);
            var block = tiledBtm[1, 1];

            Assert.AreEqual(2, block.Columns, "Unexpected amount of columns");
            Assert.AreEqual(2, block.Rows, "Unexpected amount of rows");

            var tile11 = block[1, 1];

            Assert.AreEqual(4, tile11.Columns, "Unexpected amount of columns");
            Assert.AreEqual(4, tile11.Rows, "Unexpected amount of rows");

            for (int k = 1; k <= 4; k++)
            {
                for (int l = 1; l <= 4; l++)
                {
                    Assert.AreEqual(m[k, l], tile11[k, l]);
                }
            }

            var tile12 = block[1, 2];

            Assert.AreEqual(2, tile12.Columns, "Unexpected amount of columns");
            Assert.AreEqual(4, tile12.Rows, "Unexpected amount of rows");

            for (int k = 1; k <= 4; k++)
            {
                for (int l = 1; l <= 2; l++)
                {
                    Assert.AreEqual(m[k, l + 4], tile12[k, l]);
                }
            }

            var tile21 = block[2, 1];

            Assert.AreEqual(4, tile21.Columns, "Unexpected amount of columns");
            Assert.AreEqual(2, tile21.Rows, "Unexpected amount of rows");

            for (int k = 1; k <= 2; k++)
            {
                for (int l = 1; l <= 4; l++)
                {
                    Assert.AreEqual(m[k + 4, l], tile21[k, l]);
                }
            }

            var tile22 = block[2, 2];

            Assert.AreEqual(2, tile22.Columns, "Unexpected amount of columns");
            Assert.AreEqual(2, tile22.Rows, "Unexpected amount of rows");

            for (int k = 1; k <= 2; k++)
            {
                for (int l = 1; l <= 2; l++)
                {
                    Assert.AreEqual(m[k + 4, l + 4], tile22[k, l]);
                }
            }

        }

        [TestMethod()]
        public void BlockTridiagonalMatrix_TileTest_TilesizeLargerThanMatrix()
        {
            var btm = new BlockTridiagonalMatrix<double>(1);
            var m = new Matrix<double>(6, 6);

            for (int i = 1; i <= 6; i++)
            {
                for (int j = 1; j <= 6; j++)
                {
                    m[i, j] = (i - 1) * 6 + j;
                }
            }

            btm[1, 1] = m;

            var tileSize = 400;
            var tiledBtm = btm.Tile(tileSize);
            var block = tiledBtm[1, 1];

            Assert.AreEqual(1, block.Columns, "Unexpected amount of columns");
            Assert.AreEqual(1, block.Rows, "Unexpected amount of rows");

            var tile11 = block[1, 1];

            Assert.AreEqual(6, tile11.Columns, "Unexpected amount of columns");
            Assert.AreEqual(6, tile11.Rows, "Unexpected amount of rows");

            for (int k = 1; k <= 6; k++)
            {
                for (int l = 1; l <= 6; l++)
                {
                    Assert.AreEqual(m[k, l], tile11[k, l]);
                }
            }
        }

        [TestMethod()]
        public void UntileMatrixTest()
        {
            var btm = new TiledBlockTridiagonalMatrix<double>(1);
            var bm11 = new Matrix<Matrix<double>>(2, 2);

            var block11 = new Matrix<double>(4, 4, (i, j) => i + j * 4);
            var block21 = new Matrix<double>(10, 4, (i, j) => i + 50 + j * 4);

            bm11[1, 1] = block11;
            bm11[2, 1] = block21;

            var block12 = new Matrix<double>(4, 42, (i, j) => i + 150 + j * 4);
            var block22 = new Matrix<double>(10, 42, (i, j) => i + 450 + j * 4);

            bm11[1, 2] = block12;
            bm11[2, 2] = block22;

            btm[1, 1] = bm11;

            var res = btm.Untile();
            var actual = res[1, 1];
            var expected = new Matrix<double>(block11.Rows + block21.Rows, block11.Columns + block12.Columns);
            expected.InsertMatrix(block11, 1, 1);
            expected.InsertMatrix(block12, 1, block11.Rows + 1);
            expected.InsertMatrix(block21, block11.Columns + 1, 1);
            expected.InsertMatrix(block22, block11.Columns + 1, block11.Rows + 1);

            Assert.AreEqual(expected.Rows, actual.Rows);
            Assert.AreEqual(expected.Columns, actual.Columns);

            for (int i = 1; i <= expected.Rows; i++)
            {
                for (int j = 1; j <= expected.Columns; j++)
                {
                    Assert.AreEqual(expected[i, j], actual[i, j], string.Format("i = {0}, j = {1}", i, j));
                }
            }
        }
    }
}