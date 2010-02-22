using System;
using System.Diagnostics;
using System.Globalization;

namespace TiledMatrixInversion.Math.MatrixOperations
{
    public sealed class TiledMatrixOperations<T> : IMatrixOperations<Matrix<T>>
    {
        public Matrix<Matrix<T>> Addition(Matrix<Matrix<T>> a, Matrix<Matrix<T>> b)
        {
            Debug.Assert(a.Rows == b.Rows && a.Columns == b.Columns, "Matrix A's and matrix B's rows and/or columns are not equal.");

            var result = new Matrix<Matrix<T>>(a.Rows, a.Columns);
            var dr = result.Data;
            var da = a.Data;
            var db = b.Data;

            for (int i = 0, length = dr.Length; i < length; i++)
            {
                dr[i] = da[i] + db[i];
            }

            return result;
        }

        public Matrix<Matrix<T>> Multiply(Matrix<Matrix<T>> a, Matrix<Matrix<T>> b)
        {
            Debug.Assert(a.Columns == b.Rows, "The number of columns in matrix A is not equal to the number of rows in matrix B.");

            var result = new Matrix<Matrix<T>>(a.Rows, b.Columns);
            for (int i = 1; i <= result.Rows; i++)
            {
                for (int j = 1; j <= result.Columns; j++)
                {
                    var s = a[i, 1] * b[1, j];
                    for (int k = 2; k <= a.Columns; k++)
                    {
                        s += a[i, k] * b[k, j];
                    }
                    result[i, j] = s;
                }
            }

            return result;
        }

        public Matrix<Matrix<T>> ScalarMultiply(Matrix<T> c, Matrix<Matrix<T>> a)
        {
            throw new System.NotImplementedException();
        }

        public Matrix<Matrix<T>> UnaryMinus(Matrix<Matrix<T>> a)
        {
            var res = new Matrix<Matrix<T>>(a.Rows, a.Columns);
            for (int i = 0, len = a.Data.Length; i < len; i++)
            {
                res.Data[i] = -a.Data[i];
            }
            return res;
        }

        public Matrix<Matrix<T>> Subtraction(Matrix<Matrix<T>> a, Matrix<Matrix<T>> b)
        {
            throw new System.NotImplementedException();
        }

        public Matrix<Matrix<T>> Inverse(Matrix<Matrix<T>> a)
        {
            Debug.Assert(a.Columns == a.Rows);

            // initialize r as the identity matrix
            var r = new Matrix<Matrix<T>>(a.Rows, a.Columns);
            for (int i = 1; i <= a.Rows; i++)
            {
                for (int j = 1; j <= a.Columns; j++)
                {
                    // sets diagonal to identity matrix, the rest is set to zero.
                    r[i, j] = i == j ? new Matrix<T>(a[i, j].Rows, a[i, j].Columns, true)
                                     : new Matrix<T>(a[i, j].Rows, a[i, j].Columns, default(T));
                }
            }

            var N = a.Rows;
            // Initialize f to zero matrix
            var f = new Matrix<Matrix<T>>(a.Rows, a.Columns);
            for (int i = 1; i <= a.Rows; i++)
            {
                for (int j = 1; j <= a.Columns; j++)
                {
                    f[i, j] = new Matrix<T>(a[i, j].Rows, a[i, j].Columns, default(T));
                }
            }

            var l = a.GetL();

            // calculate f = l^-1*r
            for (int i = 1; i <= N; i++)
            {
                for (int k = 0; k <= N - i - 1; k++)
                {
                    f[k + i, i] = l[k + i, k + i].Inverse() * r[k + i, i];
                    for (int j = i + k + 1; j <= N; j++)
                    {
                        r[j, i] = r[j, i] - l[j, k + i] * f[k + i, i];
                    }
                }
                f[N, i] = l[N, N].Inverse() * r[N, i];
            }

            var g = new Matrix<Matrix<T>>(a.Rows, a.Columns);
            var u = a.GetU();

            // calculate g = u^-1*f
            for (int i = 1; i <= N; i++)
            {
                for (int k = 0; k <= N - 2; k++)
                {
                    g[N - k, i] = u[N - k, N - k].Inverse() * f[N - k, i];
                    for (int j = N - 1 - k; j >= 1; j--)
                    {
                        f[j, i] = f[j, i] - u[j, N - k] * g[N - k, i];
                    }
                }
                g[1, i] = u[1, 1].Inverse() * f[1, i];
            }

            return g;
        }

        public Matrix<Matrix<T>> LFactor(Matrix<Matrix<T>> a)
        {
            ComputeLU(a);
            var result = a.LU.GetLowerTriangleWithFixedDiagonal();
            return result;
        }

        public Matrix<Matrix<T>> UFactor(Matrix<Matrix<T>> a)
        {
            ComputeLU(a);
            return a.LU.GetUpperTriangle();
        }

        public Matrix<Matrix<T>> LUFactor(Matrix<Matrix<T>> a)
        {
            ComputeLU(a);
            return a.LU;
        }

        public Matrix<Matrix<T>> MinusMatrixInverseMatrixMultiply(Matrix<Matrix<T>> a, Matrix<Matrix<T>> d)
        {
            var c = new Matrix<Matrix<T>>(a.Rows, d.Columns);
            var b = Clone(a);
            var u = d.GetU();
            var l = d.GetL();
            var M = a.Rows;
            var N = b.Columns;

            // b = au^-1
            for (int i = 1; i <= M; i++)
            {
                for (int k = 1; k <= N - 1; k++)
                {
                    b[i, k] = a[i, k] * u[k, k].Inverse();

                    for (int j = k + 1; j <= N; j++)
                    {
                        a[i, j] = a[i, j] - b[i, k] * u[k, j];
                    }
                }
                b[i, N] = a[i, N] * u[N, N].Inverse();
            }


            // c = bl^-1
            for (int i = 1; i <= M; i++)
            {
                for (int k = 0; k <= N - 2; k++)
                {
                    c[i, N - k] = b[i, N - k] * l[N - k, N - k].Inverse();

                    for (int j = N - 1 - k; j >= 1; j--)
                    {
                        b[i, j] = b[i, j] - c[i, N - k] * l[N - k, j];
                    }
                }
                c[i, 1] = b[i, 1] * l[1, 1].Inverse();
            }

            // minus and return
            return -c;
        }

        public Matrix<Matrix<T>> MinusPlusPlus(Matrix<Matrix<T>> a, Matrix<Matrix<T>> b, Matrix<Matrix<T>> c)
        {
            // todo: optimize baby!!!!
            return -a + b + c;
        }

        public Matrix<Matrix<T>> PlusMultiply(Matrix<Matrix<T>> a, Matrix<Matrix<T>> b, Matrix<Matrix<T>> c)
        {
            // todo: optimize baby!!!!
            return a + b * c;
        }

        public Matrix<T> DefaultValue
        {
            get { throw new NotSupportedException(); }
        }

        public Matrix<T> FromString(string data)
        {
            throw new System.NotImplementedException();
        }

        public Matrix<T> FromString(string data, CultureInfo cultureInfo)
        {
            throw new System.NotImplementedException();
        }

        public void NotNaNOrInfinity(Matrix<Matrix<T>> actual)
        {
            throw new NotImplementedException();
        }

        private static void ComputeLU(Matrix<Matrix<T>> matrix)
        {
            if (!matrix.IsLUFactorized)
            {
                matrix.LU = matrix.Clone();

                // wrap matrix.lu in new matrix for convenience
                var a = matrix.LU;
                // try columns if things blow up
                var N = matrix.Rows;

                a[1, 1] = a[1, 1].GetLU();

                for (int k = 1; k <= N - 1; k++)
                {
                    for (int i = k + 1; i <= N; i++)
                    {
                        a[i, k] = a[i, k] * a[k, k].GetUpperTriangle().Inverse();
                        a[k, i] = a[k, k].GetLowerTriangleWithFixedDiagonal().Inverse() * a[k, i];
                    }
                    for (int i = k + 1; i <= N; i++)
                    {
                        for (int j = k + 1; j <= N; j++)
                        {
                            a[i, j] = a[i, j] - a[i, k] * a[k, j];
                        }
                    }
                    a[k + 1, k + 1] = a[k + 1, k + 1].GetLU();
                }

                matrix.IsLUFactorized = true;
            }
        }

        public Matrix<Matrix<T>> Clone(Matrix<Matrix<T>> a)
        {
            var res = new Matrix<Matrix<T>>(a.Rows, a.Columns);
            for (int i = 0, len = a.Data.Length; i < len; i++)
            {
                res.Data[i] = a.Data[i].Clone();
            }
            return res;
        }

        /// <summary>
        /// Returns a new matrix containing the lower triangle of this matrix.
        /// </summary>
        /// <returns>The lower triangle of this matrix.</returns>
        public Matrix<Matrix<T>> GetLowerTriangle(Matrix<Matrix<T>> a)
        {
            var res = new Matrix<Matrix<T>>(a.Rows, a.Columns);

            // todo: optimize multiplications in nested for loops
            for (int j = 0; j < a.Columns; j++)
            {
                for (int i = 0; i < a.Rows; i++)
                {
                    if (i > j)
                        res.Data[i + j * a.Rows] = a.Data[i + j * a.Rows].Clone();
                    else if (i == j)
                        res.Data[i + j * a.Rows] = a.Data[i + j * a.Rows].GetLowerTriangle();
                    else
                        res.Data[i + j * a.Rows] = new Matrix<T>(a.Data[i + j * a.Rows].Rows, a.Data[i + j * a.Rows].Columns);
                }
            }
            return res;
        }

        /// <summary>
        /// Returns a new matrix containing the lower triangle of this matrix.
        /// </summary>
        /// <returns>The lower triangle of this matrix.</returns>
        public Matrix<Matrix<T>> GetLowerTriangleWithFixedDiagonal(Matrix<Matrix<T>> a)
        {
            var result = a.GetLowerTriangle();
            for (int i = 1; i <= result.Rows; i++)
            {
                result[i, i] = result[i, i].GetLowerTriangleWithFixedDiagonal();
            }
            return result;
        }

        /// <summary>
        /// Returns a new matrix containing the upper triangle of this matrix.
        /// </summary>
        /// <returns>The upper triangle of this matrix.</returns>
        public Matrix<Matrix<T>> GetUpperTriangle(Matrix<Matrix<T>> a)
        {
            var res = new Matrix<Matrix<T>>(a.Rows, a.Columns);

            // todo: optimize multiplications in nested for loops
            for (int j = 0; j < a.Columns; j++)
            {
                for (int i = 0; i < a.Rows; i++)
                {
                    if (i < j)
                        res.Data[i + j * a.Rows] = a.Data[i + j * a.Rows].Clone();
                    else if (i == j)
                        res.Data[i + j * a.Rows] = a.Data[i + j * a.Rows].GetUpperTriangle();
                    else
                        res.Data[i + j * a.Rows] = new Matrix<T>(a.Data[i + j * a.Rows].Rows, a.Data[i + j * a.Rows].Columns, default(T));
                }
            }
            return res;
        }

    }
}