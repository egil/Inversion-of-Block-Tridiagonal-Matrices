using TestHelpers;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.Math.MatrixOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.MatrixOperations;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.OperationResults;

namespace ParallelMatrixOperationsTests
{


    /// <summary>
    ///This is a test class for MultiplyTest and is intended
    ///to contain all MultiplyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MultiplyTest
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
        [TestInitialize()]
        public void MyTestInitialize()
        {
        }
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        [TestMethod()]
        public void Multiply_GENDATA_Test1()
        {
            var tileSize = 30;
            var blockMatrix1 = MatrixHelpers.Tile(Matrix<double>.CreateNewRandomDoubleMatrix(200, 200), tileSize);
            var blockMatrix2 = MatrixHelpers.Tile(Matrix<double>.CreateNewRandomDoubleMatrix(200, 200), tileSize);
            var clonedBlockMatrix1 = blockMatrix1.Clone();
            var clonedBlockMatrix2 = blockMatrix2.Clone();

            var opData1 = new OperationResult<double>(blockMatrix1);
            var opData2 = new OperationResult<double>(blockMatrix2);

            Matrix<Matrix<double>> expected = clonedBlockMatrix1 * clonedBlockMatrix2;
            OperationResult<double> actual;
            var mProducer = new Multiply<double>(opData1, opData2, out actual);
            var pm = new Manager(mProducer);
            pm.Start();
            pm.Join();

            MatrixHelpers.IsDone(actual);
            MatrixHelpers.Diff(expected, actual.Data);
            MatrixHelpers.Compare(expected, actual.Data);

        }

    }
}
