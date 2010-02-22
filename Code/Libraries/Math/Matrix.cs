using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using TiledMatrixInversion.Math.MatrixOperations;

namespace TiledMatrixInversion.Math
{

    [DebuggerDisplay("Rows = {_rows}, Columns = {_columns}")]
    [Serializable]
    public sealed class Matrix<T> : ICloneable, IEnumerable<T>, IEquatable<Matrix<T>>
    {
        // init MatrixOperations with default values for tiled and double/complex matrices 
        static Matrix()
        {
            Matrix<double>.MatrixOperations = new DoubleMatrixOperations();
            Matrix<Matrix<double>>.MatrixOperations = new TiledMatrixOperations<double>();
            Matrix<Complex>.MatrixOperations = new ComplexMatrixOperations();
            Matrix<Matrix<Complex>>.MatrixOperations = new TiledMatrixOperations<Complex>();
        }

        public static IMatrixOperations<T> MatrixOperations { get; set; }
        private readonly T[] _data;
        private readonly int _rows;
        private readonly int _columns;
        internal T[] Data { get { return _data; } }
        internal Matrix<T> LU { get; set; }
        internal bool IsLUFactorized { get; set; }

        #region properties

        /// <summary>
        /// One indexed get/set on matrix
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public T this[int row, int column]
        {
            get { return _data[row - 1 + (column - 1) * Rows]; }
            set { _data[row - 1 + (column - 1) * Rows] = value; }
        }
        public int Rows { get { return _rows; } }
        public int Columns { get { return _columns; } }

        #endregion

        #region ctors hell

        public Matrix(int rows, int columns, Func<int, int, T> value) : this(rows, columns, value, MatrixOperations) { }
        public Matrix(int rows, int columns, Func<int, int, T> value, IMatrixOperations<T> ops)
            : this(rows, columns, ops)
        {
            for (int i = 1; i <= rows; i++)
            {
                for (int j = 1; j <= columns; j++)
                {
                    this[i, j] = value(i, j);
                }
            }
        }

        public Matrix(int rows, int columns, bool initializeAsIdentityMatrix) : this(rows, columns, initializeAsIdentityMatrix, null) { }
        public Matrix(int rows, int columns, bool initializeAsIdentityMatrix, IMatrixOperations<T> ops)
            : this(rows, columns, ops)
        {
            if (initializeAsIdentityMatrix)
            {
                for (int i = 1; i <= rows && i <= columns; i++)
                {
                    this[i, i] = MatrixOperations.DefaultValue;
                }
            }
        }

        public Matrix(int rows, int columns) : this(rows, columns, MatrixOperations) { }
        public Matrix(int rows, int columns, IMatrixOperations<T> ops) : this(rows, columns, new T[rows * columns], ops) { }
        public Matrix(int rows, int columns, T value) : this(rows, columns, value, null) { }
        public Matrix(int rows, int columns, T value, IMatrixOperations<T> ops)
            : this(rows, columns, ops)
        {
            for (int i = 0, length = _data.Length; i < length; i++)
            {
                _data[i] = value;
            }
        }

        public Matrix(T[,] data) : this(data, MatrixOperations) { }
        public Matrix(T[,] data, IMatrixOperations<T> ops)
            : this(data.GetLength(0), data.GetLength(1), ops)
        {
            for (int i = 0, rows = Rows; i < rows; i++)
            {
                for (int j = 0, cols = Columns; j < cols; j++)
                {
                    _data[i + j * Rows] = data[i, j];
                }
            }
        }

        public Matrix(int rows, int columns, T[] data) : this(rows, columns, data, null) { }
        public Matrix(int rows, int columns, T[] data, IMatrixOperations<T> ops)
        {
            // assign ops to OPs if ops is not null, otherwise keep using existing OPs.
            MatrixOperations = ops ?? MatrixOperations;
            _data = data ?? new T[rows * columns];
            _rows = rows;
            _columns = columns;
        }

        #endregion

        #region matrix methods

        public Matrix<T> Inverse()
        {
            return MatrixOperations.Inverse(this);
        }

        public Matrix<T> GetL()
        {
            return MatrixOperations.LFactor(this);
        }

        public Matrix<T> GetU()
        {
            return MatrixOperations.UFactor(this);
        }

        public Matrix<T> GetLU()
        {
            return MatrixOperations.LUFactor(this);
        }

        public Matrix<T> MinusMatrixInverseMatrixMultiply(Matrix<T> b)
        {
            return MatrixOperations.MinusMatrixInverseMatrixMultiply(this, b);
        }

        public Matrix<T> MinusPlusPlus(Matrix<T> a, Matrix<T> b)
        {
            return MatrixOperations.MinusPlusPlus(this, a, b);
        }

        public Matrix<T> PlusMultiply(Matrix<T> a, Matrix<T> b)
        {
            return MatrixOperations.PlusMultiply(this, a, b);
        }

        public Matrix<T> GetLowerTriangle()
        {
            return MatrixOperations.GetLowerTriangle(this);
        }

        public Matrix<T> GetUpperTriangle()
        {
            return MatrixOperations.GetUpperTriangle(this);
        }

        public Matrix<T> GetLowerTriangleWithFixedDiagonal()
        {
            return MatrixOperations.GetLowerTriangleWithFixedDiagonal(this);
        }

        /// <summary>
        /// Returns the sub matrix of size rows by columns having the top left corner at positoin (startRow, startColumn)
        /// </summary>
        /// <remarks>This function expects parameters startRow and startColumn as one indexed.</remarks>
        /// <param name="numOfRows">Number of rows to extract</param>
        /// <param name="numOfCols">Number of columns to extract</param>
        /// <param name="startRow">Start row</param>
        /// <param name="startColumn">Start column </param>
        /// <returns></returns>
        public Matrix<T> ExtractSubMatrix(int numOfRows, int numOfCols, int startRow, int startColumn)
        {
            // update startCol/startRow to fix offset issues
            startColumn--;
            startRow--;
            var res = new Matrix<T>(numOfRows, numOfCols);

            for (var i = 1; i <= numOfRows; i++)
            {
                for (var j = 1; j <= numOfCols; j++)
                {
                    res[i, j] = this[i + startRow, j + startColumn];
                }
            }

            return res;
        }

        /// <summary>
        /// Copies the content of a (sub) matrix into this matrix.
        /// It is assumed that the inserted matrix is not larger than this matrix.
        /// </summary>
        /// <remarks>This function expects parameters startRow and startColumn as one indexed.</remarks>
        /// <param name="other"></param>
        /// <param name="startRow"></param>
        /// <param name="startColumn"></param>
        public void InsertMatrix(Matrix<T> other, int startRow, int startColumn)
        {
            // update startCol/startRow to fix offset issues
            startColumn--;
            startRow--;

            for (var i = 1; i <= other.Rows; i++)
            {
                for (var j = 1; j <= other.Columns; j++)
                {
                    this[i + startRow, j + startColumn] = other[i, j];
                }
            }
        }

        #endregion

        #region operators

        public static Matrix<T> operator +(Matrix<T> a, Matrix<T> b)
        {
            return MatrixOperations.Addition(a, b);
        }
        public static Matrix<T> operator -(Matrix<T> a, Matrix<T> b)
        {
            return MatrixOperations.Subtraction(a, b);
        }
        public static Matrix<T> operator -(Matrix<T> a)
        {
            return MatrixOperations.UnaryMinus(a);
        }
        public static Matrix<T> operator *(Matrix<T> a, Matrix<T> b)
        {
            return MatrixOperations.Multiply(a, b);
        }
        public static Matrix<T> operator *(T c, Matrix<T> a)
        {
            return MatrixOperations.ScalarMultiply(c, a);
        }
        public static Matrix<T> operator *(Matrix<T> a, T c)
        {
            return MatrixOperations.ScalarMultiply(c, a);
        }

        public static bool operator ==(Matrix<T> a, Matrix<T> b)
        {
            if (!ReferenceEquals(null, a))
                return a.Equals(b);
            return ReferenceEquals(null, b);
        }

        public static bool operator !=(Matrix<T> a, Matrix<T> b)
        {
            if (!ReferenceEquals(null, a))
                return !a.Equals(b);
            return !ReferenceEquals(null, b);
        }

        #endregion

        #region helper methods

        public Matrix<T> Clone()
        {
            return MatrixOperations.Clone(this);
        }

        public void NotNaNOrInfinity()
        {
            MatrixOperations.NotNaNOrInfinity(this);
        }

        #endregion

        #region Implementation of ICloneable

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 1; i <= Rows; i++)
            {
                for (int j = 1; j <= Columns; j++)
                {
                    yield return this[i, j];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of IEquatable<Matrix<T>>

        public bool Equals(Matrix<T> other)
        {
            if (ReferenceEquals(null, other)) return false;            
            if (ReferenceEquals(this, other)) return true;
            if (Columns != other.Columns || Rows != other.Rows) return false;

            // Compare data
            for (int i = 0, len = _data.Length; i < len; i++)
            {
                if (!_data[i].Equals(other.Data[i]))
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Matrix<T>)) return false;
            return Equals((Matrix<T>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (_data != null ? _data.GetHashCode() : 0);
                result = (result * 397) ^ _rows;
                result = (result * 397) ^ _columns;
                return result;
            }
        }

        #endregion

        public override string ToString()
        {
            var sb = new StringBuilder("<table border=1>");

            for (int i = 1; i <= Rows; i++)
            {
                sb.Append("<tr>");
                for (int j = 1; j <= Columns; j++)
                {
                    sb.Append("<td>" + this[i, j] + "</td>");
                }
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table>");
            return sb.ToString();
        }
    
        public static Matrix<T> ReadFromFile(string filename)
        {
            filename = Path.GetFullPath(filename);
            string[] data = File.ReadAllLines(filename);
            var ci = new CultureInfo("en-US");

            var res = new Matrix<T>(data.Length, data.Length);

            for (int i = 0; i < data.Length; i++)
            {
                var numbers = data[i].Split('\t');
                
                for (int j = 0; j < numbers.Length; j++)
                {
                    res[i + 1, j + 1] = MatrixOperations.FromString(numbers[j], ci);
                }
            }

            return res;
        }

        public static Matrix<double> CreateNewRandomDoubleMatrix(int rows, int columns)
        {
            return CreateNewRandomDoubleMatrix(rows, columns, new Random());
        }

        public static Matrix<double> CreateNewRandomDoubleMatrix(int rows, int columns, Random r)
        {            
            var m = new Matrix<double>(rows, columns);
            /// by ensuring the diagonal element is always the largest
            /// in each row, we avoid dividing by zero in LU factorization
            for (int i = 1; i <= rows; i++)
            {
                var diagonalElement = 0.0;
                for (int j = 1; j <= columns; j++)
                {
                    m[i, j] = r.NextDouble() * Int32.MaxValue - (Int32.MaxValue / 2.0);
                    diagonalElement += System.Math.Abs(m[i, j]);
                }
                if (i <= columns)
                    m[i, i] = diagonalElement;
            }

            return m;
        }

        public static void SerializeToFile(Matrix<T> matrix, string fileName)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter binFormatter = new BinaryFormatter();
                binFormatter.Serialize(fileStream, matrix);
            }
        }

        public static Matrix<T> DeSerializeFromFile(string fileName)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var binFormatter = new BinaryFormatter();
                return (Matrix<T>)binFormatter.Deserialize(fileStream);
            }
        }
    }

}