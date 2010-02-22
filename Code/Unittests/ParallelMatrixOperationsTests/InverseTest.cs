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
    ///This is a test class for InverseTest and is intended
    ///to contain all InverseTest Unit Tests
    ///</summary>
    [TestClass()]
    public class InverseTest
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
        public void Inverse_GENDATA_Test1()
        {
            var tileSize = 30;
            var diff = 0.0;

            // prepare data
            var data = MatrixHelpers.Tile(Matrix<double>.CreateNewRandomDoubleMatrix(200, 200), tileSize);
            var clonedData = data.Clone();
            
            // the parallel version of Inverse expectes its data to be LU Factorized, the tiled version does not.
            data = data.GetLU();
            var opData1 = new OperationResult<double>(data);

            Matrix<Matrix<double>> expected = clonedData.Inverse();
            OperationResult<double> actual;
            var mProducer = new Inverse<double>(opData1, out actual);
            var pm = new Manager(mProducer);
            pm.Start();
            pm.Join();

            MatrixHelpers.IsDone(actual);
            MatrixHelpers.Diff(expected, actual.Data, diff);
            MatrixHelpers.Compare(expected, actual.Data);            
        }

        [TestMethod]
        public void Inverse_MAPLE_100x100Test()
        {
            var tilesize = 30;
            var delta = 1.0E-11;

            var data = Matrix<double>.ReadFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\Maple Test Data\a-100x100.csv");
            var expected = Matrix<double>.ReadFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\Maple Test Data\a-inverse-100x100.csv");

            var tiledData = MatrixHelpers.Tile(data, tilesize);
            tiledData = tiledData.GetLU();
            var opData1 = new OperationResult<double>(tiledData);

            OperationResult<double> actualOR;
            var mProducer = new Inverse<double>(opData1, out actualOR);
            var pm = new Manager(mProducer, 1);
            pm.Start();
            pm.Join();
            
            MatrixHelpers.IsDone(actualOR);

            var actual = MatrixHelpers.Untile(actualOR.Data);

            MatrixHelpers.Diff(expected, actual, delta);
            MatrixHelpers.Compare(expected, actual, delta);
        }

        //[TestMethod()]
        //public void Inverse_MAPLE_1000x1000Test()
        //{
        //    Assert.Inconclusive("Missing test data");
        //    var tilesize = 30;
        //    var delta = 1.0E-11;

        //    var data =
        //        Matrix<double>.ReadFromFile(
        //            @"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\Maple Test Data\b-1000x1000.csv");
        //    var expected =
        //        Matrix<double>.ReadFromFile(
        //            @"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\Maple Test Data\b-inverse-1000x1000.csv");

        //    var tiledData = MatrixHelpers.Tile(data, tilesize);
        //    tiledData = tiledData.GetLU();
        //    var opData1 = new OperationResult<double>(tiledData);

        //    OperationResult<double> actualOR;
        //    var mProducer = new Inverse<double>(opData1, out actualOR);
        //    var pm = new Manager(mProducer);
        //    pm.Start();
        //    pm.Join();

        //    MatrixHelpers.IsDone(actualOR);

        //    var actual = MatrixHelpers.Untile(actualOR.Data);

        //    MatrixHelpers.Diff(expected, actual, delta);
        //    MatrixHelpers.Compare(expected, actual, delta);
        //}

    }
}
