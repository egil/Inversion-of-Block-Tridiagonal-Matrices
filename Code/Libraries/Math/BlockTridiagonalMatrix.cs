using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TiledMatrixInversion.Resources;

namespace TiledMatrixInversion.Math
{
    /// <summary>
    /// A tridiagonal matrix that contains block matrices.
    /// </summary>
    [Serializable]
    public class BlockTridiagonalMatrix<T> : ICloneable
    {
        private readonly Matrix<T>[][] _blocks;
        private readonly int _size;

        public BlockTridiagonalMatrix(int size)
        {
            _size = size;
            _blocks = Helpers.Init<Matrix<T>>(size, 3);
        }

        public int Size { get { return _size; } }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Since every row in the tridiagonal matrix has three elements
        /// centered around the diagonal, except for the first and last 
        /// row which have two elements.
        /// 
        /// Thw code checks to prevents access to elements other than
        /// diagonal elements and their neighbours.
        /// 
        /// Specifically, to prevent access to element [1,0] (already covered) 
        /// and [N,N+1], we have added an extra check "column <= size"
        /// </remarks>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public Matrix<T> this[int row, int column]
        {
            get
            {
                // NOTE: row and column is one-indexed and virtualColumn is zero-indexed
                var virtualColumn = column - row + 1;
                if (virtualColumn >= 0 && virtualColumn < 3 && column <= _size && column > 0)
                    return _blocks[row - 1][virtualColumn];
                throw new ArgumentOutOfRangeException("column", column, "Attempted to access a block of zeroes.");
            }
            set
            {
                // NOTE: row and column is one-indexed and virtualColumn is zero-indexed
                var virtualColumn = column - row + 1;
                if (virtualColumn < 0 || virtualColumn > 2 || column > _size || column <= 0)
                    throw new ArgumentOutOfRangeException("column", column, "Attempted to write to a block of zeroes.");

                _blocks[row - 1][virtualColumn] = value;
            }
        }

        public TiledBlockTridiagonalMatrix<T> Tile(int tileSize)
        {
            var result = new TiledBlockTridiagonalMatrix<T>(Size);
            return Tile(this, tileSize, result);
        }

        public void Tile(int tileSize, TiledBlockTridiagonalMatrix<T> result)
        {
            Tile(this, tileSize, result);
        }

        private static TiledBlockTridiagonalMatrix<T> Tile(BlockTridiagonalMatrix<T> btm, int tileSize, TiledBlockTridiagonalMatrix<T> result)
        {
            for (int i = 1; i <= btm.Size; i++)
            {
                // tile the block to the left of the diagonal
                if (i > 1)
                {
                    result[i, i - 1] = TileMatrix(btm[i, i - 1], tileSize);
                }

                // tile the block on the diagonal
                result[i, i] = TileMatrix(btm[i, i], tileSize);

                // tile the block to the right of the diagonal
                if (i < btm.Size)
                {
                    result[i, i + 1] = TileMatrix(btm[i, i + 1], tileSize);
                }
            }

            return result;
        }

        public static Matrix<Matrix<T>> TileMatrix(Matrix<T> bm, int tileSize)
        {
            // number of full tiles in block
            var r = bm.Rows / tileSize;
            var c = bm.Columns / tileSize;

            // flags indicating whether there exists a non-full row or column of tiles
            var rr = bm.Rows % tileSize > 0 ? 1 : 0;
            var cc = bm.Columns % tileSize > 0 ? 1 : 0;

            var res = new Matrix<Matrix<T>>(r + rr, c + cc);

            // creating all full sized tiles
            for (var i = 1; i <= r; i++)
            {
                for (var j = 1; j <= c; j++)
                {
                    res[i, j] = bm.ExtractSubMatrix(tileSize, tileSize, (i - 1) * tileSize + 1, (j - 1) * tileSize + 1);
                }
            }

            // creating possible non-full sized tiles
            if (rr == 1)
            {
                // create tiles for bottom row
                var tileHeight = bm.Rows % tileSize;
                for (var j = 1; j <= c; j++)
                {
                    res[r + 1, j] = bm.ExtractSubMatrix(tileHeight, tileSize, r * tileSize + 1, (j - 1) * tileSize + 1);
                }
            }
            if (cc == 1)
            {
                // create tiles for right most column
                var tileWidth = bm.Columns % tileSize;
                for (var i = 1; i <= r; i++)
                {
                    res[i, c + 1] = bm.ExtractSubMatrix(tileSize, tileWidth, (i - 1) * tileSize + 1, c * tileSize + 1);
                }
            }
            // create bottom right tile if non full
            if (rr == 1 && cc == 1)
            {
                res[r + 1, c + 1] = bm.ExtractSubMatrix(bm.Rows % tileSize, bm.Columns % tileSize, r * tileSize + 1,
                                                        c * tileSize + 1);
            }

            return res;
        }

        public static void SerializeToFile(BlockTridiagonalMatrix<T> btm, string fileName)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter binFormatter = new BinaryFormatter();
                binFormatter.Serialize(fileStream, btm);
            }
        }

        public static BlockTridiagonalMatrix<T> DeSerializeFromFile(string fileName)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var binFormatter = new BinaryFormatter();
                return (BlockTridiagonalMatrix<T>)binFormatter.Deserialize(fileStream);
            }
        }

        public static BlockTridiagonalMatrix<U> CreateBlockTridiagonalMatrix<U>(int matrixSize, int minBlockSize, int maxBlockSize, 
            Func<int, int, Random, Matrix<U>> randomMatrix)
        {
            var r = new Random();
            var btm = new BlockTridiagonalMatrix<U>(matrixSize);

            var n = r.Next(minBlockSize, maxBlockSize);
            var m = r.Next(minBlockSize, maxBlockSize);

            // first row
            btm[1, 1] = randomMatrix(n, n, r);

            if (matrixSize > 1)
            {
                btm[1, 2] = randomMatrix(n, m, r);

                // all rows in between first and last row
                for (int row = 2; row < matrixSize; row++)
                {
                    n = r.Next(minBlockSize, maxBlockSize);
                    m = btm[row - 1, row].Columns;
                    btm[row, row] = randomMatrix(m, m, r);
                    btm[row, row - 1] = randomMatrix(m, btm[row - 1, row - 1].Columns, r);
                    btm[row, row + 1] = randomMatrix(m, n, r);
                }

                // last row
                btm[matrixSize, matrixSize] = randomMatrix(btm[matrixSize - 1, matrixSize].Columns,
                                                           btm[matrixSize - 1, matrixSize].Columns, r);
                btm[matrixSize, matrixSize - 1] = randomMatrix(btm[matrixSize, matrixSize].Rows,
                                                               btm[matrixSize - 1, matrixSize - 1].Columns, r);
            }

            return btm;
        }

        #region Implementation of ICloneable

        public BlockTridiagonalMatrix<T> Clone()
        {
            var res = new BlockTridiagonalMatrix<T>(Size);
            for (int i = 1; i <= Size; i++)
            {
                if (i > 1)
                {
                    res[i, i - 1] = this[i, i - 1].Clone();
                }

                res[i, i] = this[i, i].Clone();

                if (i < Size)
                {
                    res[i, i + 1] = this[i, i + 1].Clone();
                }
            }
            return res;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

    }
}