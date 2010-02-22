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
    ///This is a test class for MinusMatrixInverseMatrixMultiplyTest and is intended
    ///to contain all MinusMatrixInverseMatrixMultiplyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MinusMatrixInverseMatrixMultiplyTest
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
        public void MinusMatrixInverseMatrixMultiply_GENDATA_Test1()
        {
            MinusMatrixInverseMatrixMultiply_GENDATA_Test(200);
        }

        //[TestMethod()]
        public void MinusMatrixInverseMatrixMultiply_GENDATA_LARGE_Test1()
        {
            MinusMatrixInverseMatrixMultiply_GENDATA_Test(1500);
        }

        static void MinusMatrixInverseMatrixMultiply_GENDATA_Test(int blockSize)
        {
            var tileSize = 30;
            var blockMatrix1 = MatrixHelpers.Tile(Matrix<double>.CreateNewRandomDoubleMatrix(blockSize, blockSize), tileSize);
            var blockMatrix2 = MatrixHelpers.Tile(Matrix<double>.CreateNewRandomDoubleMatrix(blockSize, blockSize), tileSize);
            var clonedBlockMatrix1 = blockMatrix1.Clone();
            var clonedBlockMatrix2 = blockMatrix2.Clone();

            // MinusMatrixInverseMatrix expects its second argument to be LU Factorized
            blockMatrix2 = blockMatrix2.GetLU();

            var opData1 = new OperationResult<double>(blockMatrix1);
            var opData2 = new OperationResult<double>(blockMatrix2);

            Matrix<Matrix<double>> expected = clonedBlockMatrix1.MinusMatrixInverseMatrixMultiply(clonedBlockMatrix2);
            OperationResult<double> actual;
            var nmimmProducer = new MinusMatrixInverseMatrixMultiply<double>(opData1, opData2, out actual);
            var pm = new Manager(nmimmProducer);
            pm.Start();
            pm.Join();

            MatrixHelpers.IsDone(actual);
            MatrixHelpers.Diff(expected, actual.Data);
            MatrixHelpers.Compare(expected, actual.Data);
        }

        [TestMethod]
        public void MinusMatrixInverseMatrixMultiply_MAPLE_Test2()
        {
            var tileSize = 5;
            var delta = 0.000000001;

            var a = new Matrix<double>(new double[,] { { -32, 9, 25, 50, -40, 3, 66, -11, 73 }, { -92, 71, 80, 26, 95, 31, -23, -70, -88 }, { -60, -59, 87, 16, 63, -45, -78, 25, -69 }, { 19, 57, -34, 22, -64, 75, -53, -61, -98 }, { 43, 64, -59, 51, -24, -84, -35, -79, -9 }, { -16, -74, -77, 91, 32, -88, -28, 39, -16 }, { -49, -91, -11, 78, 40, -21, -32, -98, 11 }, { 95, -30, -39, 90, -6, -65, 86, -76, 49 }, { 29, 17, 72, -90, -10, 41, 56, -14, -52 } });
            var b = new Matrix<double>(new double[,] { { 77, 63, -62, -66, -6, -56, 80, -14, -65 }, { 4, -53, -65, -36, 42, 15, 34, 78, -5 }, { -30, 82, -1, 68, -62, 3, 17, -75, -10 }, { 84, 60, 16, 63, 67, 45, 53, -42, 2 }, { 87, 95, 92, 79, -63, 64, -36, 62, -1 }, { -8, -41, 82, -89, -54, 51, 96, -61, -73 }, { -38, -38, -45, 40, -75, 86, 75, 68, 28 }, { -7, 37, -36, 65, -88, -61, -6, -99, -79 }, { 98, 8, -37, 39, 91, 86, -27, -63, -72 } });
            
            // MinusMatrixInverseMatrix expects its second argument to be LU Factorized
            b = b.GetLU();

            var expected = MatrixHelpers.Tile(new Matrix<double>(new[,] { { 0.01984508466, 0.9042399904, 0.3995515772, -0.7745771722, 0.2994725178, 0.1084556411, -0.6575886106, -0.05301668731, 0.5444903099 }, { 1.855372611, -5.371281536, -4.104553393, -0.1121640216, -0.7296221045, -1.180498074, 3.045677622, 0.1386335471, 0.2821013684 }, { 1.929588849, -4.235913487, -1.189842573, -0.4245090055, -0.7875419486, -0.8542334129, 2.365695716, -1.087624087, 0.7376953336 }, { -0.3015200144, 0.06444804897, -1.093050084, 1.295819257, -0.3278095393, -0.09936130892, -0.08586242003, 0.2752136949, -1.135640896 }, { -0.6678456488, 1.635971997, 0.7408829678, -0.03480275626, 0.3196134418, 0.6555494623, -0.4839099415, -0.3891630982, -0.1698423028 }, { 0.9195999547, -1.811264799, 0.7645929720, -0.9267010695, -0.04031193539, 0.2576350183, 0.4513746375, -1.641684280, 0.6576041476 }, { 0.8358290298, 0.4796609017, 0.9546484769, -0.7810311114, 0.6595774182, 0.08541865288, -0.1904549301, -0.9345738811, 0.06621834781 }, { -0.6369792740, 3.263015949, 3.301792682, -1.825297016, 0.8801089887, 0.6821136696, -1.785435645, -1.226781234, 0.4676272889 }, { -0.2239907182, -0.3318901401, -0.4804214610, 0.2182583930, -0.2667527668, -0.6941578781, 0.3641733215, 0.4656006742, -0.08591310567 } }), tileSize);
            
            var opData1 = new OperationResult<double>(MatrixHelpers.Tile(a, tileSize));
            var opData2 = new OperationResult<double>(MatrixHelpers.Tile(b, tileSize));

            OperationResult<double> actual;
            var nmimmProducer = new MinusMatrixInverseMatrixMultiply<double>(opData1, opData2, out actual);
            var pm = new Manager(nmimmProducer);
            pm.Start();
            pm.Join();
            
            MatrixHelpers.IsDone(actual);
            MatrixHelpers.Diff(expected, actual.Data, delta);
            MatrixHelpers.Compare(expected, actual.Data, delta);            
        }

    }
}
