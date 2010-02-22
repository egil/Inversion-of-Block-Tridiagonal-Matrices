using TiledMatrixInversion.Math;

namespace TiledMatrixInversion.Math
{
    public class TiledBlockTridiagonalMatrix<T> : BlockTridiagonalMatrix<Matrix<T>>
    {
        public TiledBlockTridiagonalMatrix(int size) : base(size) { }
        public BlockTridiagonalMatrix<T> Untile()
        {
            return Untile(this);
        }

        private static BlockTridiagonalMatrix<T> Untile(BlockTridiagonalMatrix<Matrix<T>> btm)
        {
            var res = new BlockTridiagonalMatrix<T>(btm.Size);

            for (int i = 1; i <= btm.Size; i++)
            {
                // tile the block to the left of the diagonal
                if (i > 1)
                {
                    res[i, i - 1] = UntileMatrix(btm[i, i - 1]);
                }

                // tile the block on the diagonal
                res[i, i] = UntileMatrix(btm[i, i]);

                // tile the block to the right of the diagonal
                if (i < btm.Size)
                {
                    res[i, i + 1] = UntileMatrix(btm[i, i + 1]);
                }
            }

            return res;
        }

        public static Matrix<T> UntileMatrix(Matrix<Matrix<T>> matrix)
        {
            // calculate size of untiled matrix
            var cols = 0;
            var rows = 0;

            for (int j = 1; j <= matrix.Columns; j++)
            {
                cols += matrix[1, j].Columns;
            }

            for (int i = 1; i <= matrix.Rows; i++)
            {
                rows += matrix[i, 1].Rows;
            }

            // create new matrix
            var res = new Matrix<T>(rows, cols);

            // copy matrix
            var row = 1;
            for (int i = 1; i <= matrix.Rows; i++)
            {
                var col = 1;
                for (int j = 1; j <= matrix.Columns; j++)
                {
                    var tile = matrix[i, j];
                    res.InsertMatrix(tile, row, col);
                    col += tile.Columns;
                }
                row += matrix[i, 1].Rows;
            }

            return res;
        }
    }
}