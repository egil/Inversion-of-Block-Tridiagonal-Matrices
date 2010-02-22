using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.Math.MatrixOperations;

namespace TiledMatrixInversion.Tests.MathTests
{
    /// <summary>
    ///This is a test class for DoubleMatrixOperationsTest and is intended
    ///to contain all DoubleMatrixOperationsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DoubleMatrixOperationsTest
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
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            Matrix<double>.MatrixOperations = new DoubleMatrixOperations();
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

        [TestMethod]
        public void PlusTest()
        {
            var m1 = new Matrix<double>(2, 3);
            m1[1, 1] = 1;
            m1[1, 2] = 3;
            m1[1, 3] = 1;
            m1[2, 1] = 1;
            m1[2, 2] = 0;
            m1[2, 3] = 0;

            var m2 = new Matrix<double>(2, 3);
            m2[1, 1] = 0;
            m2[1, 2] = 0;
            m2[1, 3] = 5;
            m2[2, 1] = 7;
            m2[2, 2] = 5;
            m2[2, 3] = 0;

            var res = m1 + m2;

            Assert.AreEqual(1, res[1, 1]);
            Assert.AreEqual(3, res[1, 2]);
            Assert.AreEqual(6, res[1, 3]);
            Assert.AreEqual(8, res[2, 1]);
            Assert.AreEqual(5, res[2, 2]);
            Assert.AreEqual(0, res[2, 3]);
            MatrixHelpers.NotNaNOrInfinity(res);
        }

        [TestMethod]
        public void ScalarMultiplyTest()
        {
            var m1 = new Matrix<double>(2, 3);
            m1[1, 1] = 1;
            m1[1, 2] = 8;
            m1[1, 3] = -3;
            m1[2, 1] = 4;
            m1[2, 2] = -2;
            m1[2, 3] = 5;

            var res = 2 * m1;

            Assert.AreEqual(2, res[1, 1]);
            Assert.AreEqual(16, res[1, 2]);
            Assert.AreEqual(-6, res[1, 3]);
            Assert.AreEqual(8, res[2, 1]);
            Assert.AreEqual(-4, res[2, 2]);
            Assert.AreEqual(10, res[2, 3]);
            MatrixHelpers.NotNaNOrInfinity(res);
        }

        [TestMethod]
        public void MultiplyTest()
        {
            var m1 = new Matrix<double>(2, 3);
            m1[1, 1] = 1;
            m1[1, 2] = 0;
            m1[1, 3] = 2;
            m1[2, 1] = -1;
            m1[2, 2] = 3;
            m1[2, 3] = 1;

            var m2 = new Matrix<double>(3, 2);
            m2[1, 1] = 3;
            m2[1, 2] = 1;
            m2[2, 1] = 2;
            m2[2, 2] = 1;
            m2[3, 1] = 1;
            m2[3, 2] = 0;

            var res = m1 * m2;

            Assert.AreEqual(5, res[1, 1]);
            Assert.AreEqual(1, res[1, 2]);
            Assert.AreEqual(4, res[2, 1]);
            Assert.AreEqual(2, res[2, 2]);
            MatrixHelpers.NotNaNOrInfinity(res);
        }

        [TestMethod]
        public void UnaryMinusTest()
        {
            var A = new Matrix<double>(3, 2);
            A[1, 1] = 1;
            A[1, 2] = 2;
            A[2, 1] = 3;
            A[2, 2] = 4;
            A[3, 1] = 5;
            A[3, 2] = 6;

            var zeroMatrix = new Matrix<double>(3, 2, 0.0);
            var result = A + (-A);

            MatrixHelpers.Compare(zeroMatrix, result);
        }

        [TestMethod()]
        public void InvertPivotTest()
        {
            var matrix = new Matrix<double>(new double[,] { { 5, 2, 1 }, { 1, 4, 3 }, { 1, 10, 2 } });

            var res = matrix.Inverse();
            double delta = 0.0001;
            Assert.AreEqual(0.2157, res[1, 1], delta);
            Assert.AreEqual(-0.0588, res[1, 2], delta);
            Assert.AreEqual(-0.0196, res[1, 3], delta);
            Assert.AreEqual(-0.0098, res[2, 1], delta);
            Assert.AreEqual(-0.0882, res[2, 2], delta);
            Assert.AreEqual(0.1373, res[2, 3], delta);
            Assert.AreEqual(-0.0588, res[3, 1], delta);
            Assert.AreEqual(0.4706, res[3, 2], delta);
            Assert.AreEqual(-0.1765, res[3, 3], delta);
            MatrixHelpers.NotNaNOrInfinity(res);
        }

        [TestMethod]
        public void GetLTest()
        {
            var m1 = new Matrix<double>(3, 3, (i, j) => i + j * 3);
            var m2 = m1.GetL();
            var expected = new Matrix<double>(new double[3, 3] { { 1, 0, 0 }, { 1.25, 1, 0 }, { 1.5, 2, 1 } });
            MatrixHelpers.Compare(expected, m2);
        }

        [TestMethod]
        public void GetUTest()
        {
            var m1 = new Matrix<double>(3, 3, (i, j) => i + j * 3);
            var m2 = m1.GetU();
            var expected = new Matrix<double>(new double[3, 3] { { 4, 7, 10 }, { 0, -0.75, -1.5 }, { 0, 0, 0 } });
            MatrixHelpers.Compare(expected, m2);
        }

        [TestMethod]
        public void GetLUTest()
        {
            var m1 = new Matrix<double>(3, 3, (i, j) => i + j * 3);
            var m2 = m1.GetLU();
            var expected = new Matrix<double>(new double[3, 3] { { 4, 7, 10 }, { 1.25, -0.75, -1.5 }, { 1.5, 2, 0 } });
            MatrixHelpers.Compare(expected, m2);
        }

        [TestMethod]
        public void GetLU_MAPLE_Test()
        {
            var data = new Matrix<double>(new double[,] { { -38, 12, -82, 82, 22, 76, 31, -16, -98, -4 }, { 91, 45, -70, 72, 14, -44, -50, -9, -77, 27 }, { -1, -14, 41, 42, 16, 24, -80, -50, 57, 8 }, { 63, 60, 91, 18, 9, 65, 43, -22, 27, 69 }, { -23, -35, 29, -59, 99, 86, 25, 45, -93, 99 }, { -63, 21, 70, 12, 60, 20, 94, -81, -76, 29 }, { -26, 90, -32, -62, -95, -61, 12, -38, -72, 44 }, { 30, 80, -1, -33, -20, -48, -2, -18, -2, 92 }, { 10, 19, 52, -68, -25, 77, 50, 87, -32, -31 }, { 22, 88, -13, -67, 51, 9, 10, 33, -74, 67 } });
            var expected = new Matrix<double>(new[,] { { -38.0, 12.0, -82.0, 82.0, 22.0, 76.0, 31.0, -16.0, -98.0, -4.0 }, { -2.394736842, 73.73684211, -266.3684211, 268.3684211, 66.68421053, 138.0, 24.23684211, -47.31578947, -311.6842105, 17.42105263 }, { 0.02631578947, -0.1941470378, -8.556745182, 91.94503926, 28.36759458, 48.79229122, -76.11027837, -58.76516774, -0.9336188437, 11.48750892 }, { -1.657894737, 1.083511777, -28.47647648, 2481.437771, 781.0296964, 1430.907908, -2099.218719, -1670.684017, 175.6536537, 370.6162829 }, { 0.6052631579, -0.5731620271, 8.652902903, -0.3024071353, 114.6319794, 129.6181627, 43.88460920, 30.82711952, -151.1323330, 124.0828476 }, { 1.657894737, 0.01498929336, -24.53503504, 0.8575289547, 0.4254581878, -193.1400191, -43.65660907, -76.02563456, -18.08836624, -53.38937364 }, { 0.6842105263, 1.109207709, -37.34634635, 1.216242346, -0.6500337040, 0.5174042843, -274.2617440, -77.90259604, 3.388384044, 113.9524693 }, { -0.7894736842, 1.213418986, -30.09084084, 0.9965178558, -0.07199327008, 0.5376637949, 0.6512651720, 17.18626768, 92.33795168, 7.471516923 }, { -0.2631578947, 0.3004996431, -12.90965966, 0.4271368435, -0.05792166940, -0.4232014503, 0.1858191029, 2.098618759, -262.0299723, -99.55347626 }, { -0.5789473684, 1.287651677, -33.01676677, 1.076245628, 0.6446478934, 0.7108764350, 0.9266959550, 2.842842124, 0.3992832735, -106.4678849 } });

            var actual = data.GetLU();

            var delta = 0.000001;

            MatrixHelpers.Compare(expected, actual, delta);

        }

        [TestMethod]
        public void MinusPlusPlusTest()
        {
            var target = new Matrix<double>(3, 3, (i, j) => i + j * 3);
            var a = new Matrix<double>(3, 3, (i, j) => i + j * 3);
            var b = new Matrix<double>(3, 3, (i, j) => i + j * 3);
            var expected = new Matrix<double>(3, 3, (i, j) => i + j * 3);
            var actual = target.MinusPlusPlus(a, b);
            MatrixHelpers.Compare(expected, actual);
        }

        [TestMethod]
        public void MinusMatrixInverseMatrixMultiplyTest()
        {
            var a = new Matrix<double>(new double[,] { { -38, 12, -82, 82, 22, 76, 31, -16, -98, -4 }, { 91, 45, -70, 72, 14, -44, -50, -9, -77, 27 }, { -1, -14, 41, 42, 16, 24, -80, -50, 57, 8 }, { 63, 60, 91, 18, 9, 65, 43, -22, 27, 69 }, { -23, -35, 29, -59, 99, 86, 25, 45, -93, 99 }, { -63, 21, 70, 12, 60, 20, 94, -81, -76, 29 }, { -26, 90, -32, -62, -95, -61, 12, -38, -72, 44 }, { 30, 80, -1, -33, -20, -48, -2, -18, -2, 92 }, { 10, 19, 52, -68, -25, 77, 50, 87, -32, -31 }, { 22, 88, -13, -67, 51, 9, 10, 33, -74, 67 } });
            var b = new Matrix<double>(new double[,] { { -79, -85, -29, 13, -25, -94, -19, -97, 10, 10 }, { 75, -85, 9, 32, 78, 27, -55, -38, -44, -61 }, { -85, 19, 81, 48, 23, 18, 71, -36, 26, -26 }, { -19, 25, 35, -60, -67, 18, -50, -69, -3, -20 }, { 57, 17, 80, 51, 28, 63, -17, 69, -62, -78 }, { 83, 81, 20, 20, -81, 86, 35, -15, -83, -4 }, { -45, 89, 39, -46, -36, -51, -26, 2, 9, 5 }, { 68, 92, -35, 35, -88, 51, -86, -88, 88, -91 }, { 58, -2, 26, -54, 91, 38, 50, 99, 95, -44 }, { -43, -46, -74, -17, -62, -38, -94, -59, 63, -38 } });
            var expected = new Matrix<double>(new[,] { { 2.970808035, -2.416185265, -2.287513610, 2.617111284, 1.904307121, -2.445005400, -1.788873399, 1.706944161, 0.6313281680, -3.962688136 }, { 0.1860432129, -0.8568608820, 0.4188523180, 1.391592550, 0.4241081558, -0.3004109753, -1.009110966, -0.3036403502, 0.5888620283, 0.1895771107 }, { -0.4004819239, 1.451590912, 1.033444135, -2.152159152, -1.032099629, 3.037912112, 2.279667490, -1.647458609, 1.032132255, 3.049398549 }, { -1.026904975, 1.960786004, 1.377093830, -2.539779662, -0.8840945835, 2.730145421, 2.585932727, -1.890532609, 0.6437112743, 4.442047066 }, { 2.288260337, -0.6779437672, -0.4428435948, -0.8820336606, 0.9647052426, 0.6695260252, 0.5629396166, 0.8753009597, 0.9096539589, -0.06304727948 }, { 0.9481080934, -1.603555856, -1.547436172, 0.6243000965, 1.398792739, -1.168848291, -1.205367763, 0.8522971322, 0.1526796021, -0.8076024779 }, { 0.2631531612, -1.000542562, -0.4496435159, 1.179477692, 1.047992231, -2.033187259, -2.112127722, 0.9264612318, -0.2345958784, -1.641874515 }, { -0.4984797243, 0.6153050891, 1.021393064, -0.6755114662, 0.1448758470, 1.042052335, 0.2099556424, -0.8524540446, 0.4065022862, 2.150107165 }, { 0.4327185429, 0.02810376016, -0.3942056983, -0.1787359942, -0.2064567426, -1.128483057, -0.2321896894, 0.9340760272, -0.6865966657, -1.313082405 }, { 1.957915079, -1.671542581, -0.6307747868, 0.9952308120, 1.630490333, -1.000869257, -1.833335784, 0.9359865444, 0.2836150991, -1.183066086 } });
            var actual = a.MinusMatrixInverseMatrixMultiply(b);
            double delta = 0.000000001;

            MatrixHelpers.Compare(expected, actual, delta);
        }

        [TestMethod]
        public void PlusMultiplyTest()
        {
            var target = new Matrix<double>(3, 3, (i, j) => i + j * 3);
            var a = new Matrix<double>(3, 3, (i, j) => i + j * 3);
            var b = new Matrix<double>(3, 3, (i, j) => i + j * 3);
            var expected = new Matrix<double>(new double[,] { { 115, 181, 247 }, { 131, 206, 281 }, { 147, 231, 315 } });
            var actual = target.PlusMultiply(a, b);

            MatrixHelpers.Compare(expected, actual);
        }

        [TestMethod]
        public void InverseTest()
        {
            var target = new Matrix<double>(new double[,] { { 57, -76, -32 }, { 27, -72, -74 }, { -93, -2, -4 } });
            var expected = new Matrix<double>(new double[,] { { -0.0004556401744, 0.0007810974419, -0.01080518128 }, { -0.02274946300, 0.01042765085, -0.01091583675 }, { 0.02196836555, -0.02337434095, 0.006678383128 } });
            var actual = target.Inverse();

            double delta = 0.00000000001;

            MatrixHelpers.Compare(expected, actual, delta);
        }

        [TestMethod]
        public void GetUpperTriangelTest()
        {
            var m1 = new Matrix<double>(3, 3, (i, j) => i + j * 3);
            var m2 = m1.GetUpperTriangle();

            Assert.AreEqual(m1.Rows, m2.Rows);
            Assert.AreEqual(m1.Columns, m2.Columns);

            for (int i = 1; i <= m1.Rows; i++)
            {
                for (int j = 1; j <= m1.Columns; j++)
                {
                    if (i <= j)
                    {
                        Assert.AreEqual(m1[i, j], m2[i, j]);
                    }
                    else
                    {
                        Assert.AreEqual(0, m2[i, j]);
                    }
                }
            }
        }

        [TestMethod]
        public void GetLowerTriangelTest()
        {
            var m1 = new Matrix<double>(3, 3, (i, j) => i + j * 3);
            var m2 = m1.GetLowerTriangle();

            Assert.AreEqual(m1.Rows, m2.Rows);
            Assert.AreEqual(m1.Columns, m2.Columns);

            for (int i = 1; i <= m1.Rows; i++)
            {
                for (int j = 1; j <= m1.Columns; j++)
                {
                    if (i >= j)
                    {
                        Assert.AreEqual(m1[i, j], m2[i, j]);
                    }
                    else
                    {
                        Assert.AreEqual(0, m2[i, j]);
                    }
                }
            }
        }

        [TestMethod]
        public void GetLowerTriangelWithFixedDiagonalTest()
        {
            var m1 = new Matrix<double>(3, 3, (i, j) => i + j * 3);
            var m2 = m1.GetLowerTriangleWithFixedDiagonal();

            Assert.AreEqual(m1.Rows, m2.Rows);
            Assert.AreEqual(m1.Columns, m2.Columns);

            for (int i = 1; i <= m1.Rows; i++)
            {
                for (int j = 1; j <= m1.Columns; j++)
                {
                    if (i > j)
                    {
                        Assert.AreEqual(m1[i, j], m2[i, j]);
                    }
                    else if (i == j)
                    {
                        Assert.AreEqual(1, m2[i, j]);
                    }
                    else
                    {
                        Assert.AreEqual(0, m2[i, j]);
                    }
                }
            }
        }

        [TestMethod]
        public void Inverse_MAPLE_100x100Test()
        {
            var data = Matrix<double>.ReadFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\Maple Test Data\a-100x100.csv");
            var expected = Matrix<double>.ReadFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\Maple Test Data\a-inverse-100x100.csv");

            var delta = 1.0E-11;

            var actual = data.Inverse();

            MatrixHelpers.Diff(expected, actual, delta);

            MatrixHelpers.Compare(expected, actual, delta);
        }
    }
}
