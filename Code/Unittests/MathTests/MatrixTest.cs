using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.Math.MatrixOperations;

namespace TiledMatrixInversion.Tests.MathTests
{
    /// <summary>
    ///This is a test class for MatrixTest and is intended
    ///to contain all MatrixTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MatrixTest
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

        [TestMethod]
        public void ItemAccessorTest()
        {
            var m = new Matrix<double>(3, 2);
            m[1, 1] = 1;
            m[1, 2] = 2;
            m[2, 1] = 3;
            m[2, 2] = 4;
            m[3, 1] = 5;
            m[3, 2] = 6;

            Assert.AreEqual(1, m[1, 1]);
            Assert.AreEqual(2, m[1, 2]);
            Assert.AreEqual(3, m[2, 1]);
            Assert.AreEqual(4, m[2, 2]);
            Assert.AreEqual(5, m[3, 1]);
            Assert.AreEqual(6, m[3, 2]);
        }

        [TestMethod]
        public void CtorArrayAccessorTest()
        {
            var m = new Matrix<double>(new double[3, 2] { { 1, 2 }, { 3, 4 }, { 5, 6 } });

            Assert.AreEqual(1, m[1, 1]);
            Assert.AreEqual(2, m[1, 2]);
            Assert.AreEqual(3, m[2, 1]);
            Assert.AreEqual(4, m[2, 2]);
            Assert.AreEqual(5, m[3, 1]);
            Assert.AreEqual(6, m[3, 2]);
        }

        [TestMethod]
        public void CtorConstantAccessorTest()
        {
            var m = new Matrix<double>(2, 2, 42);

            Assert.AreEqual(42, m[1, 1]);
            Assert.AreEqual(42, m[1, 2]);
            Assert.AreEqual(42, m[2, 1]);
            Assert.AreEqual(42, m[2, 2]);
        }

        [TestMethod]
        public void EnumeratorTest()
        {
            var m = new Matrix<double>(3, 2);
            m[1, 1] = 1;
            m[1, 2] = 2;
            m[2, 1] = 3;
            m[2, 2] = 4;
            m[3, 1] = 5;
            m[3, 2] = 6;

            double expected = 1 + 2 + 3 + 4 + 5 + 6;
            double result = 0;
            foreach (var d in m)
            {
                result += d;
            }
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void CloneTest()
        {
            var original = new Matrix<double>(3, 2);
            original[1, 1] = 1;
            original[1, 2] = 2;
            original[2, 1] = 3;
            original[2, 2] = 4;
            original[3, 1] = 5;
            original[3, 2] = 6;

            var clone = original.Clone();

            Assert.AreEqual(1, clone[1, 1]);
            Assert.AreEqual(2, clone[1, 2]);
            Assert.AreEqual(3, clone[2, 1]);
            Assert.AreEqual(4, clone[2, 2]);
            Assert.AreEqual(5, clone[3, 1]);
            Assert.AreEqual(6, clone[3, 2]);
        }

        [TestMethod]
        public void EqualsTest()
        {
            var m1 = new Matrix<double>(3, 2);
            m1[1, 1] = 1;
            m1[1, 2] = 2;
            m1[2, 1] = 3;
            m1[2, 2] = 4;
            m1[3, 1] = 5;
            m1[3, 2] = 6;

            var m2 = new Matrix<double>(3, 2);
            m2[1, 1] = 1;
            m2[1, 2] = 2;
            m2[2, 1] = 3;
            m2[2, 2] = 4;
            m2[3, 1] = 5;
            m2[3, 2] = 6;

            var m3 = new Matrix<double>(3, 2, 42);
            var m4 = new Matrix<double>(2, 2, 42);

            Assert.IsTrue(m1 == m2);
            Assert.IsFalse(m1 != m2);
            Assert.IsFalse(m1 == m3);
            Assert.IsTrue(m1 != m3);
            Assert.IsFalse(m1 == m4);
            Assert.IsTrue(m1 != m4);
            Assert.IsTrue(m2 == m2);
            Assert.IsFalse(m2 != m2);
        }

        [TestMethod]
        public void ExtractMatrixTest()
        {
            var m1 =
                new Matrix<double>(new double[,]
                                       {
                                           {15, -93, -89, 34, -81, -14, 11, 90, -79, -85},
                                           {-58, 12, 95, -55, 11, 37, 61, -41, 75, -85},
                                           {75, 82, 77, 54, -76, -97, 28, -79, -85, 19},
                                           {-31, -4, -84, 79, 82, -92, -48, 9, -19, 25},
                                           {-30, 14, -63, -99, -29, 73, -63, 45, 57, 17},
                                           {-50, 78, 96, -32, 29, 44, 27, -10, 83, 81},
                                           {98, -75, 69, -9, 35, 92, 58, -5, -45, 89},
                                           {5, -3, 72, 69, -70, 73, 2, 47, 68, 92},
                                           {-23, 19, 42, 31, -43, -39, 54, -54, 58, -2},
                                           {19, 69, 55, -66, -23, 62, 47, -72, -43, -46}
                                       });
            var actual = m1.ExtractSubMatrix(3, 5, 5, 1);
            var expected =
                new Matrix<double>(new double[,]
                                       {{-30, 14, -63, -99, -29}, {-50, 78, 96, -32, 29}, {98, -75, 69, -9, 35}});


            MatrixHelpers.Compare(expected, actual);            
        }

        [TestMethod]
        public void InsertMatrixTest()
        {
            var m1 = new Matrix<double>(new double[,]
                                       {
                                           {15, -93, -89, 34, -81, -14, 11, 90, -79, -85},
                                           {-58, 12, 95, -55, 11, 37, 61, -41, 75, -85},
                                           {75, 82, 77, 54, -76, -97, 28, -79, -85, 19},
                                           {-31, -4, -84, 79, 82, -92, -48, 9, -19, 25},
                                           {-30, 14, -63, -99, -29, 73, -63, 45, 57, 17},
                                           {-50, 78, 96, -32, 29, 44, 27, -10, 83, 81},
                                           {98, -75, 69, -9, 35, 92, 58, -5, -45, 89},
                                           {5, -3, 72, 69, -70, 73, 2, 47, 68, 92},
                                           {-23, 19, 42, 31, -43, -39, 54, -54, 58, -2},
                                           {19, 69, 55, -66, -23, 62, 47, -72, -43, -46}
                                       });
            var m2 = new Matrix<double>(new double[,] { { -30, 14, -63, -99, -29 }, { -50, 78, 96, -32, 29 }, { 98, -75, 69, -9, 35 } });

            m1.InsertMatrix(m2, 8, 3);

            var expected = new Matrix<double>(new double[,]
                                       {
                                           {15, -93, -89, 34, -81, -14, 11, 90, -79, -85},
                                           {-58, 12, 95, -55, 11, 37, 61, -41, 75, -85},
                                           {75, 82, 77, 54, -76, -97, 28, -79, -85, 19},
                                           {-31, -4, -84, 79, 82, -92, -48, 9, -19, 25},
                                           {-30, 14, -63, -99, -29, 73, -63, 45, 57, 17},
                                           {-50, 78, 96, -32, 29, 44, 27, -10, 83, 81},
                                           {98, -75, 69, -9, 35, 92, 58, -5, -45, 89},
                                           {5, -3, -30, 14, -63, -99, -29, 47, 68, 92},
                                           {-23, 19, -50, 78, 96, -32, 29, -54, 58, -2},
                                           {19, 69, 98, -75, 69, -9, 35, -72, -43, -46}
                                       });

            MatrixHelpers.Compare(expected, m1);
        }
    }
}