using System;
using System.Globalization;
using TiledMatrixInversion.Math;

namespace TiledMatrixInversion.Math.MatrixOperations
{
    public sealed class ComplexMatrixOperations : IMatrixOperations<Complex>
    {
        public Matrix<Complex> Addition(Matrix<Complex> a, Matrix<Complex> b)
        {
            throw new System.NotImplementedException();
        }

        public Matrix<Complex> Multiply(Matrix<Complex> a, Matrix<Complex> b)
        {
            throw new System.NotImplementedException();
        }

        public Matrix<Complex> ScalarMultiply(Complex c, Matrix<Complex> a)
        {
            throw new System.NotImplementedException();
        }

        public Matrix<Complex> UnaryMinus(Matrix<Complex> a)
        {
            throw new System.NotImplementedException();
        }

        public Matrix<Complex> Subtraction(Matrix<Complex> a, Matrix<Complex> b)
        {
            throw new System.NotImplementedException();
        }

        public Matrix<Complex> Inverse(Matrix<Complex> a)
        {
            throw new System.NotImplementedException();
        }

        public Matrix<Complex> LFactor(Matrix<Complex> a)
        {
            throw new System.NotImplementedException();
        }

        public Matrix<Complex> UFactor(Matrix<Complex> a)
        {
            throw new System.NotImplementedException();
        }

        public Matrix<Complex> LUFactor(Matrix<Complex> a)
        {
            throw new System.NotImplementedException();
        }

        public Matrix<Complex> GetUpperTriangle(Matrix<Complex> a)
        {
            throw new System.NotImplementedException();
        }

        public Matrix<Complex> GetLowerTriangle(Matrix<Complex> a)
        {
            throw new System.NotImplementedException();
        }

        public Matrix<Complex> GetLowerTriangleWithFixedDiagonal(Matrix<Complex> a)
        {
            throw new System.NotImplementedException();
        }

        public Matrix<Complex> MinusMatrixInverseMatrixMultiply(Matrix<Complex> a, Matrix<Complex> d)
        {
            throw new System.NotImplementedException();
        }

        public Matrix<Complex> MinusPlusPlus(Matrix<Complex> a, Matrix<Complex> b, Matrix<Complex> c)
        {
            throw new System.NotImplementedException();
        }

        public Matrix<Complex> PlusMultiply(Matrix<Complex> a, Matrix<Complex> b, Matrix<Complex> c)
        {
            throw new System.NotImplementedException();
        }

        public Matrix<Complex> Clone(Matrix<Complex> a)
        {
            throw new System.NotImplementedException();
        }

        public Complex DefaultValue
        {
            get { throw new System.NotImplementedException(); }
        }

        public Complex FromString(string data)
        {
            throw new System.NotImplementedException();
        }

        public Complex FromString(string data, CultureInfo cultureInfo)
        {
            throw new System.NotImplementedException();
        }

        public void NotNaNOrInfinity(Matrix<Complex> actual)
        {
            throw new NotImplementedException();
        }
    }
}