using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;
using TiledMatrixInversion.BlockMatrixInverter;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.Math.MatrixOperations;

namespace TiledMatrixInversion.Tests.MathTests
{
    /// <summary>
    ///This is a test class for SingleThreadedTiledBlockMatrixInverterTest and is intended
    ///to contain all SingleThreadedTiledBlockMatrixInverterTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TiledSingleThreadedBlockMatrixInverterTest
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

        [TestMethod()]
        public void TiledInvertTest()
        {
            var inverter = new TiledSingleThreadedBlockMatrixInverter<double>();
            var btm = new BlockTridiagonalMatrix<double>(3);

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
            btm[1, 1] = block11;

            var block12 = new Matrix<double>(4, 2);
            block12[1, 1] = 10;
            block12[1, 2] = 5;
            block12[2, 1] = 5;
            block12[2, 2] = 20;
            block12[3, 1] = 1;
            block12[3, 2] = 2;
            block12[4, 1] = 3;
            block12[4, 2] = 4;
            btm[1, 2] = block12;

            var block21 = new Matrix<double>(2, 4);
            block21[1, 1] = 20;
            block21[1, 2] = 1;
            block21[1, 3] = 3;
            block21[1, 4] = 4;
            block21[2, 1] = 2;
            block21[2, 2] = 10;
            block21[2, 3] = 1;
            block21[2, 4] = 2;
            btm[2, 1] = block21;

            var block22 = new Matrix<double>(2, 2);
            block22[1, 1] = 7;
            block22[1, 2] = 2;
            block22[2, 1] = 2;
            block22[2, 2] = 8;
            btm[2, 2] = block22;

            var block23 = new Matrix<double>(2, 3);
            block23[1, 1] = 9;
            block23[1, 2] = 5;
            block23[1, 3] = 2;
            block23[2, 1] = 6;
            block23[2, 2] = 10;
            block23[2, 3] = 2;
            btm[2, 3] = block23;

            var block32 = new Matrix<double>(3, 2);
            block32[1, 1] = 6;
            block32[1, 2] = 4;
            block32[2, 1] = 1;
            block32[2, 2] = 2;
            block32[3, 1] = 8;
            block32[3, 2] = 3;
            btm[3, 2] = block32;

            var block33 = new Matrix<double>(3, 3);
            block33[1, 1] = 10;
            block33[1, 2] = 2;
            block33[1, 3] = 4;
            block33[2, 1] = 3;
            block33[2, 2] = 20;
            block33[2, 3] = 6;
            block33[3, 1] = 7;
            block33[3, 2] = 8;
            block33[3, 3] = 30;
            btm[3, 3] = block33;

            var tiled = btm.Tile(3);

            inverter.Invert(tiled);

            btm = tiled.Untile();

            MatrixHelpers.NotNaNOrInfinity(btm);

            // random spot testing of some of 81 results.......
            double delta = 0.0001;
            Assert.AreEqual(-0.0065, btm[1, 1][1, 1], delta);
            Assert.AreEqual(0.0497, btm[1, 1][2, 1], delta);
            Assert.AreEqual(-0.0422, btm[1, 1][2, 2], delta);
            Assert.AreEqual(0.0645, btm[1, 1][2, 3], delta);

            Assert.AreEqual(0.0555, btm[1, 2][1, 1], delta);
            Assert.AreEqual(0.1474, btm[1, 2][2, 2], delta);
            Assert.AreEqual(0.0823, btm[1, 2][3, 2], delta);
            Assert.AreEqual(-0.0485, btm[1, 2][4, 1], delta);

            Assert.AreEqual(0.1288, btm[2, 1][1, 1], delta);
            Assert.AreEqual(0.0468, btm[2, 1][1, 3], delta);            
            Assert.AreEqual(-0.0635, btm[2, 1][2, 3], delta);
            Assert.AreEqual(0.0479, btm[2, 1][2, 4], delta);
            
            Assert.AreEqual(-0.0053, btm[2, 2][1, 1], delta);
            Assert.AreEqual(0.0146, btm[2, 2][1, 2], delta);
            Assert.AreEqual(-0.0086, btm[2, 2][2, 1], delta);
            Assert.AreEqual(-0.0381, btm[2, 2][2, 2], delta);            

            Assert.AreEqual(-0.0046, btm[2, 3][2, 3], delta);
            Assert.AreEqual(0.0028, btm[3, 2][2, 2], delta);
            Assert.AreEqual(-0.0124, btm[3, 3][1, 3], delta);

        }

    }
}