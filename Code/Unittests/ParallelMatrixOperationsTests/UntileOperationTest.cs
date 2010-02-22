using Microsoft.VisualStudio.TestTools.UnitTesting;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.MatrixOperations;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.OperationResults;
using TiledMatrixInversion.Resources;

namespace ParallelMatrixOperationsTests
{
    
    
    /// <summary>
    ///This is a test class for TileOperationTest and is intended
    ///to contain all TileOperationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class UntileOperationTest
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

        [TestMethod()]
        public void Untile_GENDATA_Test()
        {
            var matrixSize = 100;
            var minBlockSize = 100;
            var maxBlockSize = 200;
            var tileSize = 30;
            var data = BlockTridiagonalMatrix<double>.CreateBlockTridiagonalMatrix<double>(matrixSize,
                                                                            minBlockSize,
                                                                            maxBlockSize,
                                                                            Matrix<double>.CreateNewRandomDoubleMatrix).Tile(tileSize);
            var tbtmData = Helpers.Init<OperationResult<double>>(data.Size + 1, 3);
            for (int i = 1; i <= data.Size; i++)
            {
                // tile the block to the left of the diagonal
                if (i > 1)
                {
                    tbtmData[i][0] = new OperationResult<double>(data[i, i - 1]);
                }

                // tile the block on the diagonal
                tbtmData[i][1] = new OperationResult<double>(data[i, i]);

                // tile the block to the right of the diagonal
                if (i < data.Size)
                {
                    tbtmData[i][2] = new OperationResult<double>(data[i, i + 1]);
                }
            }

            BlockTridiagonalMatrix<double> expected = data.Untile();
            BlockTridiagonalMatrix<double> actual = new BlockTridiagonalMatrix<double>(expected.Size);
            
            var untileProducer = new UntileOperation<double>(tbtmData, actual);
            var pm = new Manager(untileProducer);
            pm.Start();
            pm.Join();

            for (int i = 1; i <= expected.Size; i++)
            {
                if (i > 1)
                {
                    Assert.AreEqual(expected[i, i - 1], actual[i, i - 1]);
                }

                Assert.AreEqual(expected[i, i], actual[i, i]);

                if (i < expected.Size)
                {
                    Assert.AreEqual(expected[i, i + 1], actual[i, i + 1]);
                }
            }
        }

    }
}
