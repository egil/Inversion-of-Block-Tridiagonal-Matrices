using System.Globalization;

namespace TiledMatrixInversion.Math.MatrixOperations
{
    public interface IMatrixOperations<T>
    {
        Matrix<T> Addition(Matrix<T> a, Matrix<T> b);
        /// <summary>
        /// Multiply two matrices.
        /// Assumes number of columns in matrix a is equal to the number of rows in matrix b.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        Matrix<T> Multiply(Matrix<T> a, Matrix<T> b);
        Matrix<T> ScalarMultiply(T c, Matrix<T> a);
        Matrix<T> UnaryMinus(Matrix<T> a);
        Matrix<T> Subtraction(Matrix<T> a, Matrix<T> b);
        Matrix<T> Inverse(Matrix<T> a);
        Matrix<T> LFactor(Matrix<T> a);
        Matrix<T> UFactor(Matrix<T> a);
        Matrix<T> LUFactor(Matrix<T> a);
        Matrix<T> GetUpperTriangle(Matrix<T> a);
        Matrix<T> GetLowerTriangle(Matrix<T> a);
        Matrix<T> GetLowerTriangleWithFixedDiagonal(Matrix<T> a);        
        Matrix<T> MinusMatrixInverseMatrixMultiply(Matrix<T> a, Matrix<T> d);
        Matrix<T> MinusPlusPlus(Matrix<T> a, Matrix<T> b, Matrix<T> c);
        Matrix<T> PlusMultiply(Matrix<T> a, Matrix<T> b, Matrix<T> c);
        Matrix<T> Clone(Matrix<T> a);
        T DefaultValue { get; }
        T FromString(string data);
        T FromString(string data, CultureInfo cultureInfo);
        void NotNaNOrInfinity(Matrix<T> actual);        
    }
}