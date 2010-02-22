using System;
using System.Configuration;
using System.IO;
using TiledMatrixInversion.Math;

namespace Experiments
{
    public static class MeasurementDataSets
    {
        public static bool ReadFromDisk { get; set; }
        private static int _rows;
        public static int Rows
        {
            get { return _rows; }
            set { _rows = value; }
        }

        private static int _columns;
        public static int Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }

        private static int _btmSize;
        public static int BtmSize
        {
            get { return _btmSize; }
            set { _btmSize = value; }
        }

        private static int _btmMinBlockSize;
        public static int BtmMinBlockSize
        {
            get { return _btmMinBlockSize; }
            set { _btmMinBlockSize = value; }
        }

        private static int _btmMaxBlockSize;
        public static int BtmMaxBlockSize
        {
            get { return _btmMaxBlockSize; }
            set { _btmMaxBlockSize = value; }
        }

        public static string Matrix1FileName { get; set; }
        public static string Matrix2FileName { get; set; }
        public static string Matrix3FileName { get; set; }
        public static string BTMFileName { get; set; }

        private static Matrix<double> matrix1;
        private static Matrix<double> matrix2;
        private static Matrix<double> matrix3;
        private static BlockTridiagonalMatrix<double> btm;

        static MeasurementDataSets()
        {
            //bool.TryParse(ConfigurationManager.AppSettings["GetDataFromDisk"], out ReadFromDisk);
            //if (!ReadFromDisk)
            //{
                
            //    if (!int.TryParse(ConfigurationManager.AppSettings["MatrixRows"], out _rows))
            //        throw new ArgumentException("MatrixRows is not set");
            //    if (!int.TryParse(ConfigurationManager.AppSettings["MatrixColumns"], out _columns))
            //        throw new ArgumentException("MatrixColumns is not set");
            //    if (!int.TryParse(ConfigurationManager.AppSettings["BTMSize"], out _btmSize))
            //        throw new ArgumentException("BTMSize is not set");
            //    if (!int.TryParse(ConfigurationManager.AppSettings["BTMMinBlockSize"], out _btmMinBlockSize))
            //        throw new ArgumentException("BTMMinBlockSize is not set");
            //    if (!int.TryParse(ConfigurationManager.AppSettings["BTMMaxBlockSize"], out _btmMaxBlockSize))
            //        throw new ArgumentException("BTMMaxBlockSize is not set");
            //}
            //else
            //{
            //    Matrix1FileName = ConfigurationManager.AppSettings["Matrix1FileName"] ?? "matrix1.mat";
            //    Matrix2FileName = ConfigurationManager.AppSettings["Matrix2FileName"] ?? "matrix2.mat";
            //    Matrix3FileName = ConfigurationManager.AppSettings["Matrix3FileName"] ?? "matrix3.mat";
            //}
        }

        public static Matrix<double> Matrix1
        {
            get
            {
                if(matrix1 == null)
                    if (File.Exists(Matrix1FileName))
                        matrix1 = Matrix<double>.DeSerializeFromFile(Matrix1FileName);
                    else
                        matrix1 = Matrix<double>.CreateNewRandomDoubleMatrix(Rows, Columns);

                return matrix1;
            }
        }
        public static Matrix<double> Matrix2
        {
            get
            {
                if (matrix2 == null)
                    if (File.Exists(Matrix2FileName))
                        return Matrix<double>.DeSerializeFromFile(Matrix2FileName);
                    else
                        matrix2 = Matrix<double>.CreateNewRandomDoubleMatrix(Rows, Columns);

                return matrix2;
            }
        }
        public static Matrix<double> Matrix3
        {
            get
            {
                if (matrix3 == null)
                    if (File.Exists(Matrix3FileName))
                        return Matrix<double>.DeSerializeFromFile(Matrix3FileName);
                    else
                        matrix3 = Matrix<double>.CreateNewRandomDoubleMatrix(Rows, Columns);

                return matrix3;
            }
        }
        public static BlockTridiagonalMatrix<double> BTM
        {
            get
            {
                if (btm == null)
                    if (File.Exists(BTMFileName))
                        return BlockTridiagonalMatrix<double>.DeSerializeFromFile(BTMFileName);
                    else
                        btm = BlockTridiagonalMatrix<double>.CreateBlockTridiagonalMatrix<double>(BtmSize, BtmMinBlockSize, BtmMaxBlockSize, Matrix<double>.CreateNewRandomDoubleMatrix);

                return btm;
            }
        }
        #region tiling / untiling

        public static Matrix<T> Untile<T>(Matrix<Matrix<T>> matrix)
        {
            var tbtm = new TiledBlockTridiagonalMatrix<T>(1);
            tbtm[1, 1] = matrix;
            var btm = tbtm.Untile();
            return btm[1, 1];
        }

        public static Matrix<Matrix<T>> Tile<T>(Matrix<T> matrix, int tileSize)
        {
            var btm = new BlockTridiagonalMatrix<T>(1);
            btm[1, 1] = matrix;
            var tbtm = btm.Tile(tileSize);
            return tbtm[1, 1];
        }

        #endregion
    }
}