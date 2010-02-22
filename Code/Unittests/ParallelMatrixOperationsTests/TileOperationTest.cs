using TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.MatrixOperations;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.OperationResults;

namespace ParallelMatrixOperationsTests
{
    
    
    /// <summary>
    ///This is a test class for TileOperationTest and is intended
    ///to contain all TileOperationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TileOperationTest
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
        public void Tile_GENDATA_Test()
        {
            var matrixSize = 100;
            var minBlockSize = 100;
            var maxBlockSize = 200;
            var data = BlockTridiagonalMatrix<double>.CreateBlockTridiagonalMatrix<double>(matrixSize, minBlockSize, maxBlockSize,
                                                                            Matrix<double>.CreateNewRandomDoubleMatrix);

            var tileSize = 30;

            TiledBlockTridiagonalMatrix<double> expected = data.Tile(tileSize);
            OperationResult<double>[][] actual;
            var tileProducer = new TileOperation<double>(data, tileSize, out actual);
            var pm = new Manager(tileProducer);
            pm.Start();
            pm.Join();

            for (int i = 1; i <= expected.Size; i++)
            {
                if (i > 1)
                {
                    MatrixHelpers.Compare(expected[i, i - 1], actual[i][0].Data);
                    MatrixHelpers.IsDone(actual[i][0]);
                }

                MatrixHelpers.Compare(expected[i, i], actual[i][1].Data);
                MatrixHelpers.IsDone(actual[i][1]);

                if (i < expected.Size)
                {
                    MatrixHelpers.Compare(expected[i, i + 1], actual[i][2].Data);
                    MatrixHelpers.IsDone(actual[i][2]);
                }
            }



        }

    }
}
