using TiledMatrixInversion.Math;

namespace TiledMatrixInversion.BlockMatrixInverter
{
    public class TiledSingleThreadedBlockMatrixInverter<T>
    {
        public void Invert(TiledBlockTridiagonalMatrix<T> tbtm)
        {
            var N = tbtm.Size;
            
            // the following arrays are used as one-indexed, so item 0 is always ignored
            var cl = new Matrix<Matrix<T>>[N];
            var cr = new Matrix<Matrix<T>>[N + 1];
            var dl = new Matrix<Matrix<T>>[N + 1];
            var dr = new Matrix<Matrix<T>>[N + 1];

            // perform upwards sweep
            // calculate the last dr, cr, outside the loop, their calculations differ from the rest
            dr[N] = tbtm[N, N];
            
            // only needed if N > 1 
            if (N > 1)
                cr[N] = tbtm[N - 1, N].MinusMatrixInverseMatrixMultiply(dr[N]);

            // calculate dr, cr for N-1 down to 2
            for (int i = N - 1; i > 1; i--)
            {
                dr[i] = tbtm[i, i].PlusMultiply(cr[i + 1], tbtm[i + 1, i]);
                cr[i] = tbtm[i - 1, i].MinusMatrixInverseMatrixMultiply(dr[i]);
            }

            // calculates dr_11
            // if N is 1 the calculation is already performed
            if (N > 1)
            {
                dr[1] = tbtm[1, 1].PlusMultiply(cr[2], tbtm[2, 1]);
            }

            // perform downward sweep
            // calculate the first dl, cl, outside of the loop, their calculations differ from the rest
            dl[1] = tbtm[1, 1];
            
            // only needed if N > 1 
            if (N > 1)
                cl[1] = tbtm[2, 1].MinusMatrixInverseMatrixMultiply(dl[1]);

            // calculate dl, cl, for 2 to N-2
            for (int i = 2; i <= N-1; i++)
            {
                dl[i] = tbtm[i, i].PlusMultiply(cl[i - 1], tbtm[i - 1, i]);
                cl[i] = tbtm[i + 1, i].MinusMatrixInverseMatrixMultiply(dl[i]);
            }

            // if N is 1 the calculation is already performed
            if (N > 1)
            {
                dl[N] = tbtm[N, N].PlusMultiply(cl[N - 1], tbtm[N - 1, N]);
            }

            // the final calculation in calculating the inverse of the btm
            // calculate the diagonal element then left and right neighbour            
            for (int i = 1; i <= N; i++)
            {
                // diagonal
                tbtm[i, i] = tbtm[i, i].MinusPlusPlus(dl[i], dr[i]).Inverse();
                
                // left neighbour
                if (i > 1)
                {
                    tbtm[i, i - 1] = tbtm[i, i]*cl[i - 1];
                }

                // right neighbour
                if (i < N)
                {
                    tbtm[i, i + 1] = tbtm[i, i]*cr[i + 1];
                }
            }            
        }
    }
}