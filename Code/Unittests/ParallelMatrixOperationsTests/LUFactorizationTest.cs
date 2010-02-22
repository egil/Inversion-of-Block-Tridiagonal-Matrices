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
    ///This is a test class for LUFactorizationTest and is intended
    ///to contain all LUFactorizationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LUFactorizationTest
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
        public void LUFactorizationTest1()
        {
            var tilesize = 3;
            var data = new Matrix<double>(new double[,] { { -38, 12, -82, 82, 22, 76, 31, -16, -98, -4 }, { 91, 45, -70, 72, 14, -44, -50, -9, -77, 27 }, { -1, -14, 41, 42, 16, 24, -80, -50, 57, 8 }, { 63, 60, 91, 18, 9, 65, 43, -22, 27, 69 }, { -23, -35, 29, -59, 99, 86, 25, 45, -93, 99 }, { -63, 21, 70, 12, 60, 20, 94, -81, -76, 29 }, { -26, 90, -32, -62, -95, -61, 12, -38, -72, 44 }, { 30, 80, -1, -33, -20, -48, -2, -18, -2, 92 }, { 10, 19, 52, -68, -25, 77, 50, 87, -32, -31 }, { 22, 88, -13, -67, 51, 9, 10, 33, -74, 67 } });
            var expected = new Matrix<double>(new[,] { { -38.0, 12.0, -82.0, 82.0, 22.0, 76.0, 31.0, -16.0, -98.0, -4.0 }, { -2.394736842, 73.73684211, -266.3684211, 268.3684211, 66.68421053, 138.0, 24.23684211, -47.31578947, -311.6842105, 17.42105263 }, { 0.02631578947, -0.1941470378, -8.556745182, 91.94503926, 28.36759458, 48.79229122, -76.11027837, -58.76516774, -0.9336188437, 11.48750892 }, { -1.657894737, 1.083511777, -28.47647648, 2481.437771, 781.0296964, 1430.907908, -2099.218719, -1670.684017, 175.6536537, 370.6162829 }, { 0.6052631579, -0.5731620271, 8.652902903, -0.3024071353, 114.6319794, 129.6181627, 43.88460920, 30.82711952, -151.1323330, 124.0828476 }, { 1.657894737, 0.01498929336, -24.53503504, 0.8575289547, 0.4254581878, -193.1400191, -43.65660907, -76.02563456, -18.08836624, -53.38937364 }, { 0.6842105263, 1.109207709, -37.34634635, 1.216242346, -0.6500337040, 0.5174042843, -274.2617440, -77.90259604, 3.388384044, 113.9524693 }, { -0.7894736842, 1.213418986, -30.09084084, 0.9965178558, -0.07199327008, 0.5376637949, 0.6512651720, 17.18626768, 92.33795168, 7.471516923 }, { -0.2631578947, 0.3004996431, -12.90965966, 0.4271368435, -0.05792166940, -0.4232014503, 0.1858191029, 2.098618759, -262.0299723, -99.55347626 }, { -0.5789473684, 1.287651677, -33.01676677, 1.076245628, 0.6446478934, 0.7108764350, 0.9266959550, 2.842842124, 0.3992832735, -106.4678849 } });
            
            // start test
            var a = new OperationResult<double>(MatrixHelpers.Tile(data, tilesize));

            OperationResult<double> actualTiled;
            var luFactProducer = new LUFactorization<double>(a, out actualTiled);
            var pm = new Manager(luFactProducer);
            pm.Start();
            pm.Join();

            MatrixHelpers.IsDone(actualTiled);

            var actual = MatrixHelpers.Untile(actualTiled.Data);

            var diff = 1.0E-6;
            MatrixHelpers.Diff(expected, actual, diff);

            var delta = 0.000001;

            // test dimensions of tiled data
            MatrixHelpers.CompareDimensions(data, actual);

            // test data in untiled output
            MatrixHelpers.Compare(expected, actual, delta);
        }

        [TestMethod()]
        public void LUFactorization_Compare_Parallel_Against_DoubleOperationVersion()
        {
            var tilesize = 3;
            var data = new Matrix<double>(new double[,] { { -38, 12, -82, 82, 22, 76, 31, -16, -98, -4 }, { 91, 45, -70, 72, 14, -44, -50, -9, -77, 27 }, { -1, -14, 41, 42, 16, 24, -80, -50, 57, 8 }, { 63, 60, 91, 18, 9, 65, 43, -22, 27, 69 }, { -23, -35, 29, -59, 99, 86, 25, 45, -93, 99 }, { -63, 21, 70, 12, 60, 20, 94, -81, -76, 29 }, { -26, 90, -32, -62, -95, -61, 12, -38, -72, 44 }, { 30, 80, -1, -33, -20, -48, -2, -18, -2, 92 }, { 10, 19, 52, -68, -25, 77, 50, 87, -32, -31 }, { 22, 88, -13, -67, 51, 9, 10, 33, -74, 67 } });
            var expected = new Matrix<double>(new double[,] { { -38, 12, -82, 82, 22, 76, 31, -16, -98, -4 }, { 91, 45, -70, 72, 14, -44, -50, -9, -77, 27 }, { -1, -14, 41, 42, 16, 24, -80, -50, 57, 8 }, { 63, 60, 91, 18, 9, 65, 43, -22, 27, 69 }, { -23, -35, 29, -59, 99, 86, 25, 45, -93, 99 }, { -63, 21, 70, 12, 60, 20, 94, -81, -76, 29 }, { -26, 90, -32, -62, -95, -61, 12, -38, -72, 44 }, { 30, 80, -1, -33, -20, -48, -2, -18, -2, 92 }, { 10, 19, 52, -68, -25, 77, 50, 87, -32, -31 }, { 22, 88, -13, -67, 51, 9, 10, 33, -74, 67 } })
                .GetLU();

            // start test
            var a = new OperationResult<double>(MatrixHelpers.Tile(data, tilesize));

            OperationResult<double> actualTiled;
            var luFactProducer = new LUFactorization<double>(a, out actualTiled);
            var pm = new Manager(luFactProducer);
            pm.Start();
            pm.Join();

            MatrixHelpers.IsDone(actualTiled);

            var actual = MatrixHelpers.Untile(actualTiled.Data);

            var delta = 1.0E-11;
            MatrixHelpers.Diff(expected, actual, delta);

            // test dimensions of tiled data
            MatrixHelpers.CompareDimensions(data, actual);

            // test data in untiled output
            MatrixHelpers.Compare(expected, actual, delta);
        }

        [TestMethod()]
        public void LUFactorizationInplace_GENDATA_Test()
        {
            var tileSize = 30;
            var data = MatrixHelpers.Tile(Matrix<double>.CreateNewRandomDoubleMatrix(200, 200), tileSize);
            var clonedData = data.Clone();
            var opData = new OperationResult<double>(clonedData);

            Matrix<Matrix<double>> expected = data.GetLU();
            OperationResult<double> actual;
            var inplace = true;
            var luFactProducer = new LUFactorization<double>(opData, out actual, inplace);
            var pm = new Manager(luFactProducer);
            pm.Start();
            pm.Join();

            Assert.IsTrue(actual.Completed);

            var diff = 0.0;
            MatrixHelpers.Diff(expected, actual.Data, diff);

            // test data in untiled output
            MatrixHelpers.Compare(expected, actual.Data);

        }

        [TestMethod()]
        public void LUFactorization_GENDATA_Test2()
        {
            var tileSize = 30;
            var data = MatrixHelpers.Tile(Matrix<double>.CreateNewRandomDoubleMatrix(200, 200), tileSize);
            var opData = new OperationResult<double>(data);

            Matrix<Matrix<double>> expected = data.GetLU();
            OperationResult<double> actual;
            var luFactProducer = new LUFactorization<double>(opData, out actual);
            var pm = new Manager(luFactProducer);
            pm.Start();
            pm.Join();

            Assert.IsTrue(actual.Completed);

            var diff = 0.0;
            MatrixHelpers.Diff(expected, actual.Data, diff);

            var delta = 0.0000001;

            // test dimensions of tiled data
            MatrixHelpers.CompareDimensions(data, actual.Data);

            // test data in untiled output
            MatrixHelpers.Compare(expected, actual.Data, delta);
        }
    }
}
