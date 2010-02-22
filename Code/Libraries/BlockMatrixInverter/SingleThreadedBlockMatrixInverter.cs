using TiledMatrixInversion.Math;

namespace TiledMatrixInversion.BlockMatrixInverter
{
    public class SingleThreadedBlockMatrixInverter<T>
    {
        public void Invert(BlockTridiagonalMatrix<T> btm)
        {
            var N = btm.Size;

            // the following arrays are used as one-indexed, so item 0 is always ignored
            var cl = new Matrix<T>[N];
            var cr = new Matrix<T>[N + 1];
            var dl = new Matrix<T>[N + 1];
            var dr = new Matrix<T>[N + 1];

            // perform upwards sweep
            // calculate the last dr, cr, outside the loop, their calculations differ from the rest
            dr[N] = btm[N, N];

            // only needed if N > 1 
            if (N > 1)
                cr[N] = btm[N - 1, N].MinusMatrixInverseMatrixMultiply(dr[N]);

            // calculate dr, cr for N-1 down to 2
            for (int i = N - 1; i > 1; i--)
            {
                dr[i] = btm[i, i].PlusMultiply(cr[i + 1], btm[i + 1, i]);
                cr[i] = btm[i - 1, i].MinusMatrixInverseMatrixMultiply(dr[i]);
            }

            // calculates dr_11
            // if N is 1 the calculation is already performed
            if (N > 1)
            {
                dr[1] = btm[1, 1].PlusMultiply(cr[2], btm[2, 1]);
            }

            // perform downward sweep
            // calculate the first dl, cl, outside of the loop, their calculations differ from the rest
            dl[1] = btm[1, 1];
            
            // only needed if N > 1 
            if (N > 1)
                cl[1] = btm[2, 1].MinusMatrixInverseMatrixMultiply(dl[1]);

            // calculate dl, cl, for 2 to N-2
            for (int i = 2; i <= N-1; i++)
            {
                dl[i] = btm[i, i].PlusMultiply(cl[i - 1], btm[i - 1, i]);
                cl[i] = btm[i + 1, i].MinusMatrixInverseMatrixMultiply(dl[i]);
            }

            // if N is 1 the calculation is already performed
            if (N > 1)
            {
                dl[N] = btm[N, N].PlusMultiply(cl[N - 1], btm[N - 1, N]);
            }

            // the final calculation in calculating the inverse of the btm
            // calculate the diagonal element then left and right neighbour            

            // middle rows
            for (int i = 1; i <= N; i++)
            {
                // diagonal
                btm[i, i] = btm[i, i].MinusPlusPlus(dl[i], dr[i]).Inverse();
                
                // left neighbour
                if (i > 1)
                {
                    btm[i, i - 1] = btm[i, i]*cl[i - 1];
                }

                // right neighbour
                if (i < N)
                {
                    btm[i, i + 1] = btm[i, i]*cr[i + 1];
                }
            }
        }
    }
}