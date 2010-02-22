using System;
using System.Diagnostics;
using System.Globalization;

namespace TiledMatrixInversion.Math.MatrixOperations
{
    public sealed class DoubleMatrixOperations : IMatrixOperations<double>
    {
        public void NotNaNOrInfinity(Matrix<double> actual)
        {
            for (int i = 1; i <= actual.Rows; i++)
            {
                for (int j = 1; j <= actual.Columns; j++)
                {
                    if (double.IsNaN(actual[i, j])) 
                        Console.WriteLine("Actual is NaN: Position [{0}, {1}].", i, j);
                    if (double.IsInfinity(actual[i, j])) 
                        Console.WriteLine("Actual is IsInfinity: Position [{0}, {1}].", i, j);
                }
            }
        }

        public Matrix<double> Clone(Matrix<double> a)
        {
            var res = new Matrix<double>(a.Rows, a.Columns);

            Buffer.BlockCopy(a.Data, 0, res.Data, 0, Buffer.ByteLength(a.Data));
            if (!ReferenceEquals(null, a.LU))
            {
                res.LU = a.LU.Clone();
            }
            res.IsLUFactorized = a.IsLUFactorized;
            return res;
        }

        public Matrix<double> Addition(Matrix<double> a, Matrix<double> b)
        {
            Debug.Assert(a.Rows == b.Rows && a.Columns == b.Columns, "Matrix A's and matrix B's rows and/or columns are not equal.");

            var result = new Matrix<double>(a.Rows, a.Columns);
            var dr = result.Data;
            var da = a.Data;
            var db = b.Data;

            for (int i = 0, length = dr.Length; i < length; i++)
            {
                dr[i] = da[i] + db[i];
            }

            return result;
        }
        public Matrix<double> Subtraction(Matrix<double> a, Matrix<double> b)
        {
            Debug.Assert(a.Rows == b.Rows && a.Columns == b.Columns, "Matrix A's and matrix B's rows and/or columns are not equal.");

            var result = new Matrix<double>(a.Rows, a.Columns);
            var dr = result.Data;
            var da = a.Data;
            var db = b.Data;

            for (int i = 0, length = dr.Length; i < length; i++)
            {
                dr[i] = da[i] - db[i];
            }

            return result;
        }

        public Matrix<double> Multiply(Matrix<double> a, Matrix<double> b)
        {
            Debug.Assert(a.Columns == b.Rows, "The number of columns in matrix A is not equal to the number of rows in matrix B.");

            var da = a.Data;
            var arows = a.Rows;
            var acols = a.Columns;
            var db = b.Data;
            var brows = b.Rows;

            var result = new Matrix<double>(arows, b.Columns);
            var rrows = result.Rows;
            var rcols = result.Columns;
            var dr = result.Data;

            for (int j = 0, jj = 0; j < rcols * brows; j += brows, jj += rrows)
            {
                for (int i = 0; i < rrows; i++)
                {
                    double s = 0;
                    for (int k = 0, kk = 0; k < acols; k++, kk += arows)
                    {
                        s += da[i + kk] * db[k + j];
                    }
                    dr[i + jj] = s;                    
                }
            }

            return result;
        }

        public Matrix<double> ScalarMultiply(double c, Matrix<double> a)
        {
            var result = new Matrix<double>(a.Rows, a.Columns);

            var dr = result.Data;
            var da = a.Data;

            for (int i = 0, length = dr.Length; i < length; i++)
            {
                dr[i] = c * da[i];
            }

            return result;
        }

        public Matrix<double> UnaryMinus(Matrix<double> a)
        {
            var result = new Matrix<double>(a.Rows, a.Columns);

            var dr = result.Data;
            var da = a.Data;

            for (int i = 0, length = dr.Length; i < length; i++)
            {
                dr[i] = -da[i];
            }

            return result;
        }

        #region Optimized/combined matrix ops

        public Matrix<double> MinusMatrixInverseMatrixMultiply(Matrix<double> a, Matrix<double> d)
        {
            // possible optimization: reduce to one pass of the matrices (hard)
            return -a * d.Inverse();
        }

        public Matrix<double> MinusPlusPlus(Matrix<double> a, Matrix<double> b, Matrix<double> c)
        {
            var result = new Matrix<double>(a.Rows, a.Columns);

            var dr = result.Data;
            var da = a.Data;
            var db = b.Data;
            var dc = c.Data;

            for (int i = 0, length = dr.Length; i < length; i++)
            {
                dr[i] = -da[i] + db[i] + dc[i];
            }

            return result;
        }

        public Matrix<double> PlusMultiply(Matrix<double> a, Matrix<double> b, Matrix<double> c)
        {
            Debug.Assert(b.Columns == c.Rows, "The number of columns in matrix B is not equal to the number of rows in matrix C.");
            Debug.Assert(a.Columns == c.Columns, "The number of columns in matrix A is not equal to the number of columns in matrix C.");
            Debug.Assert(a.Rows == b.Rows, "The number of rows in matrix A is not equal to the number of rows in matrix B.");
            
            var da = a.Data;
            var db = b.Data;
            var brows = b.Rows;
            var bcols = b.Columns;
            var dc = c.Data;
            var crows = c.Rows;

            var result = new Matrix<double>(brows, c.Columns);
            var rrows = result.Rows;
            var rcols = result.Columns;
            var dr = result.Data;

            for (int j = 0, jj = 0; j < rcols * crows; j += crows, jj += rrows)
            {
                for (int i = 0; i < rrows; i++)
                {
                    double s = 0;
                    for (int k = 0, kk = 0; k < bcols; k++, kk += brows)
                    {
                        s += db[i + kk] * dc[k + j];
                    }
                    dr[i + jj] = da[i + jj] + s;
                }
            }

            return result;
        }

        public double DefaultValue
        {
            get { return 1.0; }
        }

        public double FromString(string data, CultureInfo cultureInfo)
        {
            return double.Parse(data, cultureInfo.NumberFormat);
        }
        public double FromString(string data)
        {
            return double.Parse(data);
        }

        #endregion

        #region Inverse/LU Fact
        /// The inverse/LU fact code below is veeeery inspired by dnAnalytics AbstractLU and DenseLU classes,
        /// with we removed do not need in our project.

        public Matrix<double> LFactor(Matrix<double> a)
        {
            ComputeLU(a);
            var result = a.LU.GetLowerTriangle();
            for (int i = 1; i <= result.Rows; i++)
            {
                result[i, i] = 1;
            }
            return result;
        }

        public Matrix<double> LUFactor(Matrix<double> a)
        {
            ComputeLU(a);
            return a.LU;
        }

        public Matrix<double> UFactor(Matrix<double> a)
        {
            ComputeLU(a);
            return a.LU.GetUpperTriangle();
        }

        public Matrix<double> Inverse(Matrix<double> a)
        {
            ComputeLU(a);
            Debug.Assert(!IsSingular(a), "Matrix is Singular, can not calculate the inverse.");
            //if (IsSingular(a)) throw new ArgumentException("Matrix is Singular, can not calculate the inverse.");
            return ComputeInverse(a);
        }

        public static double Determinant(Matrix<double> matrix)
        {
            var a = matrix.GetLU();
            var N = a.Rows;

            var result = 1.0;

            for (int i = 1; i <= N; i++)
            {
                result = result * a[i, i];
            }

            return result;
        }

        public static bool IsSingular(Matrix<double> matrix)
        {
            return Determinant(matrix) == 0.0;
        }

        private static void ComputeLU(Matrix<double> matrix)
        {
            if (!matrix.IsLUFactorized)
            {
                matrix.LU = new Matrix<double>(matrix.Rows, matrix.Columns);
                Buffer.BlockCopy(matrix.Data, 0, matrix.LU.Data, 0, Buffer.ByteLength(matrix.Data));

                var data = matrix.LU.Data;

                int order = matrix.Rows;

                double[] LUcolj = new double[order];

                // Outer loop.
                for (int j = 0; j < order; j++)
                {
                    int indexj = j * order;
                    int indexjj = indexj + j;
                    // Make a copy of the j-th column to localize references.
                    for (int i = 0; i < order; i++)
                    {
                        LUcolj[i] = data[indexj + i];
                    }

                    // Apply previous transformations.
                    for (int i = 0; i < order; i++)
                    {
                        // Most of the time is spent in the following dot product.
                        int kmax = System.Math.Min(i, j);
                        double s = 0.0;
                        for (int k = 0; k < kmax; k++)
                        {
                            s += data[k * order + i] * LUcolj[k];
                        }

                        data[indexj + i] = LUcolj[i] -= s;
                    }

                    // Compute multipliers.
                    if (j < order & data[indexjj] != 0.0)
                    {
                        for (int i = j + 1; i < order; i++)
                        {
                            data[indexj + i] /= data[indexjj];  
                        }
                    }
                }
                matrix.IsLUFactorized = true;
            }
        }

        private static Matrix<double> ComputeInverse(Matrix<double> matrix)
        {
            int order = matrix.Rows;
            
            Matrix<double> inverse = new Matrix<double>(order, order);
            for (int i = 0; i < order; i++)
            {
                inverse.Data[i + order * i] = 1.0;
            }

            int columns = inverse.Columns;
            double[] factor = matrix.LU.Data;
            double[] data = inverse.Data;

            // Solve L*Y = B(piv,:)
            for (int k = 0; k < order; k++)
            {
                int korder = k * order;
                for (int i = k + 1; i < order; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        int index = j * order;
                        data[i + index] -= data[k + index] * factor[i + korder];
                    }
                }
            }
            // Solve U*X = Y;
            for (int k = order - 1; k >= 0; k--)
            {
                int korder = k + k * order;
                for (int j = 0; j < columns; j++)
                {
                    data[k + j * order] /= factor[korder];
                    
                }
                korder = k * order;
                for (int i = 0; i < k; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        int index = j * order;
                        data[i + index] -= data[k + index] * factor[i + korder];
                    }
                }
            }

            return inverse;
        }

        /// <summary>
        /// Returns a new matrix containing the lower triangle of this matrix.
        /// </summary>
        /// <returns>The lower triangle of this matrix.</returns>        
        public Matrix<double> GetLowerTriangle(Matrix<double> a)
        {
            var res = new Matrix<double>(a.Rows, a.Columns);

            // possible optimization: multiplications in nested for loops
            for (int j = 0; j < a.Columns; j++)
            {
                for (int i = 0; i < a.Rows; i++)
                {
                    if (i >= j)
                    {
                        res.Data[i + j * a.Rows] = a.Data[i + j * a.Rows];
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Returns a new matrix containing the lower triangle of this matrix.
        /// </summary>
        /// <returns>The lower triangle of this matrix.</returns>        
        public Matrix<double> GetLowerTriangleWithFixedDiagonal(Matrix<double> a)
        {
            var result = a.GetLowerTriangle();
            for (int i = 1; i <= result.Rows; i++)
            {
                result[i, i] = 1;
            }
            return result;
        }

        /// <summary>
        /// Returns a new matrix containing the upper triangle of this matrix.
        /// </summary>
        /// <returns>The upper triangle of this matrix.</returns>   
        public Matrix<double> GetUpperTriangle(Matrix<double> a)
        {
            var res = new Matrix<double>(a.Rows, a.Columns);
            
            // possible optimization: multiplications in nested for loops
            for (int j = 0; j < a.Columns; j++)
            {
                for (int i = 0; i < a.Rows; i++)
                {
                    if (i <= j)
                    {
                        res.Data[i + j * a.Rows] = a.Data[i + j * a.Rows];
                    }
                }
            }
            return res;
        }

        #endregion
    }
}