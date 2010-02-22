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
    public class BigTests
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

        [TestMethod()]
        public void InverseTest_1500x1500()
        {
            var tileSize = 40;

            // prepare data
            var data = MatrixHelpers.Tile(Matrix<double>.DeSerializeFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\m1500x1500-a.mat"), tileSize);
            Matrix<Matrix<double>> expected = MatrixHelpers.Tile(Matrix<double>.DeSerializeFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\m1500x1500-a-Inverse-result.mat"), tileSize);

            // the parallel version of Inverse expectes its data to be LU Factorized, the tiled version does not.
            data = data.GetLU();
            var opData1 = new OperationResult<double>(data);

            OperationResult<double> actual;
            var mProducer = new Inverse<double>(opData1, out actual);
            var pm = new Manager(mProducer);
            pm.Start();
            pm.Join();

            MatrixHelpers.IsDone(actual);
            MatrixHelpers.Diff(expected, actual.Data);
            MatrixHelpers.Compare(expected, actual.Data);              
        }

        [TestMethod()]
        public void LUFactTest_1500x1500()
        {
            var tileSize = 40;

            // prepare data
            var data = MatrixHelpers.Tile(Matrix<double>.DeSerializeFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\m1500x1500-a.mat"), tileSize);
            Matrix<Matrix<double>> expected = MatrixHelpers.Tile(Matrix<double>.DeSerializeFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\m1500x1500-a-LUFactorize-result.mat"), tileSize);

            var opData1 = new OperationResult<double>(data);
            OperationResult<double> actual;
            var mProducer = new LUFactorization<double>(opData1, out actual);
            var pm = new Manager(mProducer);
            pm.Start();
            pm.Join();

            MatrixHelpers.IsDone(actual);
            MatrixHelpers.Diff(expected, actual.Data);
            MatrixHelpers.Compare(expected, actual.Data);
        }

        [TestMethod()]
        public void MinusPlusPlus_2000x2000()
        {
            var tileSize = 40;

            // prepare data
            var data1 = MatrixHelpers.Tile(Matrix<double>.DeSerializeFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\m2000x2000-a.mat"), tileSize);
            var data2 = MatrixHelpers.Tile(Matrix<double>.DeSerializeFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\m2000x2000-b.mat"), tileSize);
            var data3 = MatrixHelpers.Tile(Matrix<double>.DeSerializeFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\m2000x2000-c.mat"), tileSize);
            Matrix<Matrix<double>> expected = MatrixHelpers.Tile(Matrix<double>.DeSerializeFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\m2000x2000-a_b_c-MinusPlusPlus-result.mat"), tileSize);

            var opData1 = new OperationResult<double>(data1);
            var opData2 = new OperationResult<double>(data2);
            var opData3 = new OperationResult<double>(data3);
            OperationResult<double> actual;
            var mProducer = new MinusPlusPlus<double>(opData1, opData2, opData3, out actual);
            var pm = new Manager(mProducer);
            pm.Start();
            pm.Join();

            MatrixHelpers.IsDone(actual);
            MatrixHelpers.Diff(expected, actual.Data);
            MatrixHelpers.Compare(expected, actual.Data);
        }

        [TestMethod()]
        public void Multiply_1500x1500()
        {
            var tileSize = 40;

            // prepare data
            var data1 = MatrixHelpers.Tile(Matrix<double>.DeSerializeFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\m1500x1500-a.mat"), tileSize);
            var data2 = MatrixHelpers.Tile(Matrix<double>.DeSerializeFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\m1500x1500-b.mat"), tileSize);
            Matrix<Matrix<double>> expected = MatrixHelpers.Tile(Matrix<double>.DeSerializeFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\m1500x1500-a_b-Multiply-result.mat"), tileSize);

            var opData1 = new OperationResult<double>(data1);
            var opData2 = new OperationResult<double>(data2);
            OperationResult<double> actual;
            var mProducer = new Multiply<double>(opData1, opData2, out actual);
            var pm = new Manager(mProducer);
            pm.Start();
            pm.Join();

            MatrixHelpers.IsDone(actual);
            MatrixHelpers.Diff(expected, actual.Data);
            MatrixHelpers.Compare(expected, actual.Data);
        }

        [TestMethod()]
        public void MinusMatrixInverseMatrixMultiply_1500x1500()
        {
            var tileSize = 40;

            // prepare data
            var data1 = MatrixHelpers.Tile(Matrix<double>.DeSerializeFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\m1500x1500-a.mat"), tileSize);
            var data2 = MatrixHelpers.Tile(Matrix<double>.DeSerializeFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\m1500x1500-b.mat"), tileSize);
            Matrix<Matrix<double>> expected = MatrixHelpers.Tile(Matrix<double>.DeSerializeFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\m1500x1500-a_b-MinusMatrixInverseMatrixMultiply-result.mat"), tileSize);
            
            var opData1 = new OperationResult<double>(data1);
            var opData2 = new OperationResult<double>(data2.GetLU());
            OperationResult<double> actual;
            var mProducer = new MinusMatrixInverseMatrixMultiply<double>(opData1, opData2, out actual);
            var pm = new Manager(mProducer);
            pm.Start();
            pm.Join();

            MatrixHelpers.IsDone(actual);
            MatrixHelpers.Diff(expected, actual.Data);
            MatrixHelpers.Compare(expected, actual.Data);
        }


        [TestMethod()]
        public void PlusMultiply1500x1500()
        {
            var tileSize = 40;

            // prepare data
            var data1 = MatrixHelpers.Tile(Matrix<double>.DeSerializeFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\m1500x1500-a.mat"), tileSize);
            var data2 = MatrixHelpers.Tile(Matrix<double>.DeSerializeFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\m1500x1500-b.mat"), tileSize);
            var data3 = MatrixHelpers.Tile(Matrix<double>.DeSerializeFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\m1500x1500-c.mat"), tileSize);
            Matrix<Matrix<double>> expected = MatrixHelpers.Tile(Matrix<double>.DeSerializeFromFile(@"C:\Users\eh\Documents\KU\Inversion-of-Block-Tridiagonal-Matrices\Dataset\m1500x1500-a_b_c-PlusMultiply-result.mat"), tileSize);

            var opData1 = new OperationResult<double>(data1);
            var opData2 = new OperationResult<double>(data2);
            var opData3 = new OperationResult<double>(data3);
            OperationResult<double> actual;
            var mProducer = new PlusMultiply<double>(opData1, opData2, opData3, out actual);
            var pm = new Manager(mProducer);
            pm.Start();
            pm.Join();

            MatrixHelpers.IsDone(actual);
            MatrixHelpers.Diff(expected, actual.Data);
            MatrixHelpers.Compare(expected, actual.Data);
        }
    }
}
