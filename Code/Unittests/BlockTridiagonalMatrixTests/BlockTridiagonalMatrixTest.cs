using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TiledMatrixInversion.Math;

namespace TiledMatrixInversion.Tests.BlockTridiagonalMatrixTests
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
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
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

            for (int i = 1; i <= 3; i++)
            {
                for (int j = 1; j <= 3; j++)
                {
                    var tile = block[i, j];
                    Assert.AreEqual(2, tile.Columns, "Unexpected amount of columns");
                    Assert.AreEqual(2, tile.Rows, "Unexpected amount of rows");

                    for (int k = 1; k <= 2; k++)
                    {
                        for (int l = 1; l <= 2; l++)
                        {
                            Assert.AreEqual(m[(i - 1) * tileSize + k, (j - 1) * tileSize + l], tile[k, l]);
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
            var bm11 = new Matrix<Matrix<double>>(1, 2);            

            var block11 = new Matrix<double>(4, 4);
            block11[1, 1] = 3;
            block11[1, 2] = 2;
            block11[1, 3] = 1;
            block11[1, 4] = 2;
            block11[2, 1] = 3;
            block11[2, 2] = 4;
            block11[2, 3] = 3;
            block11[2, 4] = 1;
            block11[3, 1] = 1;
            block11[3, 2] = 2;
            block11[3, 3] = 6;
            block11[3, 4] = 4;
            block11[4, 1] = 5;
            block11[4, 2] = 7;
            block11[4, 3] = 6;
            block11[4, 4] = 8;

            bm11[1, 1] = block11;

            var block12 = new Matrix<double>(4, 2);
            block12[1, 1] = 10;
            block12[1, 2] = 5;
            block12[2, 1] = 5;
            block12[2, 2] = 20;
            block12[3, 1] = 1;
            block12[3, 2] = 2;
            block12[4, 1] = 3;
            block12[4, 2] = 4;
            bm11[1, 2] = block12;

            btm[1, 1] = bm11;

            var res = btm.Untile();
            var item = res[1, 1];
            for(int i = 1; i<= block11.Rows;i++)
            {
                for(int j = 1; j<= block11.Columns;j++)
                {
                    Assert.AreEqual(block11[i, j], item[i, j]);                    
                }
            }

            for (int i = 1; i <= block12.Rows; i++)
            {
                for (int j = 1; j <= block12.Columns; j++)
                {
                    Assert.AreEqual(block12[i, j], item[i, j + block11.Columns]);
                }
            }
        }
    }
}