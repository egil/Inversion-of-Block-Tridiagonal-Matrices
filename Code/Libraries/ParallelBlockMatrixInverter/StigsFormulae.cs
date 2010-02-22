using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.ParallelBlockMatrixInverter.Enumerators;
using TiledMatrixInversion.ParallelBlockMatrixInverter.MatrixOperations;
using TiledMatrixInversion.ParallelBlockMatrixInverter.OperationResults;

namespace TiledMatrixInversion.ParallelBlockMatrixInverter
{
    public class StigsFormulae<T> : IProducer<IProducer<Action>>
    {
        #region Fields

        private static TraceSource Log = new TraceSource("StigsFormulae");
        private readonly object _lock = new object();
        private readonly int _tileSize;
        private readonly BlockTridiagonalMatrix<T> _input;
        private readonly OperationEnumerator<AbstractOperation<OpType>> _gen;
        private readonly BlockTridiagonalMatrix<T> _result;
        private OperationResult<T>[,] _a;
        private readonly OperationResult<T>[,] _g;
        private readonly OperationResult<T>[] _glu;
        private readonly OperationResult<T>[] _gSum;
        private readonly OperationResult<T>[] _cl;
        private readonly OperationResult<T>[] _cr;
        private readonly OperationResult<T>[] _dl;
        private readonly OperationResult<T>[] _dr;
        private readonly OperationResult<T>[] _dllu;
        private readonly OperationResult<T>[] _drlu;
        private readonly int N;

        #endregion

        public StigsFormulae(BlockTridiagonalMatrix<T> data, int tileSize, out BlockTridiagonalMatrix<T> result)
        {
            _input = data;
            _tileSize = tileSize;

            N = data.Size;

            // the following arrays are used as one-indexed, so item 0 is always ignored
            _cl = new OperationResult<T>[N];
            _cr = new OperationResult<T>[N + 1];
            _dl = new OperationResult<T>[N + 1];
            _dr = new OperationResult<T>[N + 1];
            _dllu = new OperationResult<T>[N + 1];
            _drlu = new OperationResult<T>[N + 1];
            _g = new OperationResult<T>[N + 1,3];
            _glu = new OperationResult<T>[N + 1];
            _gSum = new OperationResult<T>[N + 1];

            _result = result = new BlockTridiagonalMatrix<T>(data.Size);

            _gen = new OperationEnumerator<AbstractOperation<OpType>>(FormulaeGenerator(N).GetEnumerator(), 1 /* max queue length */);
        }

        #region Implementation of IProducer<Action>

        public bool IsCompleted { get { return _gen.Completed; } }

        public bool TryGetNext(out IProducer<Action> producer)
        {
            lock (_lock)
            {
                var op = _gen.Find(IsRunnable);
                producer = GenerateProducer(op);
                return producer != null;
            }
        }

        #endregion

        #region Helper functions

        private IProducer<Action> GenerateProducer(AbstractOperation<OpType> op)
        {
            if(op == null)
                return null;

            //Debug.WriteLine(Thread.CurrentThread.Name + " [ StigsFormulae: " + op + "]");
            Log.TraceEvent(TraceEventType.Information, 0, "{0} : {1}", Thread.CurrentThread.Name, op);
            switch (op.OP)
            {
                case OpType.Tile:
                    return new TileOperation<T>(_input, _tileSize, out _a);

                case OpType.cL:
                    // calculate cl_i = -a_i+1,i * (dl_i,i)^-1
                    return new NegateMatrixInverseMatrixMultiply<T>(_a[op.I + 1, 0], _dllu[op.I], out _cl[op.I]);

                case OpType.cR:
                    // calculate cr_i = -a_i-1,i * (dr_i,i)^-1
                    return new NegateMatrixInverseMatrixMultiply<T>(_a[op.I - 1, 2], _drlu[op.I], out _cr[op.I]);

                case OpType.dL:
                    // if op.I == 1, then we only create a 
                    // copy of _a[1, 1] and put that into _dr[1]
                    if (op.I == 1)
                        return new SimpleProducer(() => { _dl[1] = new OperationResult<T>(_a[1, 1].Data.Clone()); });
                    else
                        // calculate dl_i,i = a_i,i + cl_i-1 * a_i-1,i
                        return new PlusMultiply<T>(_a[op.I, 1], _cl[op.I - 1], _a[op.I - 1, 2], out _dl[op.I]);

                case OpType.dLLU:
                    // LU factorize dl_i and save that into dllu_i
                    return new LUFactorization<T>(_dl[op.I], out _dllu[op.I], false /* inplace */);

                case OpType.dR:
                    // if op.I == N, then we only create a 
                    // copy of _a[N, N] and put that into _dr[N]
                    if (op.I == N)
                        return new SimpleProducer(() =>
                            {
                                _dr[N] = new OperationResult<T>(_a[N, 1].Data.Clone());
                            });
                    else
                        // calculate dr_i,i = a_i,i + cr_i+1 * a_i+1,i
                        return new PlusMultiply<T>(
                            _a[op.I, 1],
                            _cr[op.I + 1],
                            _a[op.I + 1, 0],
                            out _dr[op.I]);

                case OpType.dRLU:
                    // LU factorize dr_i and save that into drlu_i
                    return new LUFactorization<T>(_dr[op.I], out _drlu[op.I], false /* inplace */);

                case OpType.gSum_ii:
                    // calculate gsum_i,i = -a_i,i + dl_i + dr_i
                    return new MinusPlusPlus<T>(_a[op.I, 1], _dl[op.I], _dr[op.I], out _gSum[op.I]);

                case OpType.g:
                    // if diagonal element, calculate the inverse.
                    if (op.I == op.J)
                    {
                        return new Inverse<T>(_glu[op.I], out _g[op.I, 1]);
                    }

                    // calculate the right immediate neighbour
                    // g_i,j = g_i,i * cr_i+1
                    if (op.I < op.J)
                    {
                        return new Multiply<T>(_g[op.I, 1], _cr[op.I + 1], out _g[op.I, 2]);
                    }

                    // calculate the left immediate neighbour
                    // g_i,j = g_i,i * cl_i-1
                    // I > J
                    return new Multiply<T>(_g[op.I, 1], _cl[op.I - 1], out _g[op.I, 0]);

                case OpType.gLU_ii:
                    // LU factorize dr_i and save that into drlu_i                    
                    return new LUFactorization<T>(_gSum[op.I], out _glu[op.I], true /* inplace */);

                case OpType.Untile:
                    return new UntileOperation<T>(_g, _result);
                default:
                    Debug.Fail("Should not happen!");
                    return null;
            }
        }

        private bool IsRunnable(AbstractOperation<OpType> op)
        {
            switch (op.OP)
            {
                case OpType.Tile:
                    return true;

                case OpType.cL:
                    return _dllu[op.I] != null && _dllu[op.I].Completed &&
                           _a[op.I + 1, 0] != null && _a[op.I + 1, 0].Completed;

                case OpType.cR:

                    return _drlu[op.I] != null && _drlu[op.I].Completed &&
                           _a[op.I - 1, 2] != null && _a[op.I - 1, 2].Completed;

                case OpType.dL:
                    if (op.I == 1)
                        return _a[op.I, 1] != null && _a[op.I, 1].Completed;
                    return _a[op.I, 1] != null && _a[op.I, 1].Completed &&
                           _cl[op.I - 1] != null && _cl[op.I - 1].Completed &&
                           _a[op.I - 1, 2] != null && _a[op.I - 1, 2].Completed;

                case OpType.dLLU:
                    return _dl[op.I] != null && _dl[op.I].Completed;

                case OpType.dR:
                    if (op.I == N)
                        return _a[op.I, 1] != null && _a[op.I, 1].Completed;
                    return _a[op.I, 1] != null && _a[op.I, 1].Completed &&
                           _cr[op.I + 1] != null && _cr[op.I + 1].Completed &&
                           _a[op.I + 1, 0] != null && _a[op.I + 1, 0].Completed;

                case OpType.dRLU:
                    return _dr[op.I] != null && _dr[op.I].Completed;

                case OpType.gSum_ii:
                    return _a[op.I, 1] != null && _a[op.I, 1].Completed &&
                           _dl[op.I] != null && _dl[op.I].Completed &&
                           _dr[op.I] != null && _dr[op.I].Completed;

                case OpType.g:
                    if (op.I == op.J)
                        return _glu[op.I] != null && _glu[op.I].Completed;
                    else
                    {
                        var ret = _g[op.I, 1] != null && _g[op.I, 1].Completed;
                        if (op.I < op.J)
                        {
                            for (int i = op.I + 1; i <= op.J; i++)
                            {
                                ret = ret && _cr[i] != null && _cr[i].Completed;
                            }
                        }
                        else
                        {
                            for (int i = op.I - 1; i >= op.J; i--)
                            {
                                ret = ret && _cl[i] != null && _cl[i].Completed;
                            }
                        }
                        return ret;
                    }

                case OpType.gLU_ii:
                    return _gSum[op.I] != null && _gSum[op.I].Completed;

                case OpType.Untile:
                    bool retval = true;
                    var length = _g.GetLength(0) - 1;
                    for (int i = 1; i <= length; i++)
                    {
                        if (i > 1)
                            retval = retval && _g[i, 0] != null && _g[i, 0].Completed;

                        retval = retval && _g[i, 1] != null && _g[i, 1].Completed;

                        if (i < length)
                            retval = retval && _g[i, 2] != null && _g[i, 2].Completed;
                    }
                    return retval;

                default:
                    Debug.Fail("StigsFormler.IsRunnable: Should not happen!");
                    return false;
            }
        }

        #endregion

        #region abstract operation representation

        private static IEnumerable<AbstractOperation<OpType>> FormulaeGenerator(int N)
        {
            Log.TraceEvent(TraceEventType.Start, 0);

            // start with tiling
            yield return new AbstractOperation<OpType>(OpType.Tile); // { OP = OpType.Tile };

            // perform upwards sweep
            // calculate dr, cr for N down to 2
            for (int i = N; i >= 2; i--)
            {
                yield return new AbstractOperation<OpType>(i, OpType.dR); // { OP = OpType.dR, I = i };
                yield return new AbstractOperation<OpType>(i, OpType.dRLU); // { OP = OpType.dRLU, I = i };
                yield return new AbstractOperation<OpType>(i, OpType.cR); // { OP = OpType.cR, I = i };
            }

            // calculates dr_11
            yield return new AbstractOperation<OpType>(1, OpType.dR); // { OP = OpType.dR, I = 1 };

            // perform downward sweep
            // calculate dl, cl, for 1 to N-1
            for (int i = 1; i <= N - 1; i++)
            {
                yield return new AbstractOperation<OpType>(i, OpType.dL); // { OP = OpType.dL, I = i };
                yield return new AbstractOperation<OpType>(i, OpType.dLLU); // { OP = OpType.dLLU, I = i };
                yield return new AbstractOperation<OpType>(i, OpType.cL); // { OP = OpType.cL, I = i };
            }

            // calculates dl_N
            yield return new AbstractOperation<OpType>(N, OpType.dL); // { OP = OpType.dL, I = N };

            // the final calculation in calculating the inverse of the btm
            // calculate the diagonal element then left and right neighbour            
            for (int i = 1; i <= N; i++)
            {
                // diagonal
                yield return new AbstractOperation<OpType>(i, OpType.gSum_ii); // { OP = OpType.gSum_ii, I = i };
                yield return new AbstractOperation<OpType>(i, OpType.gLU_ii); // { OP = OpType.gLU_ii, I = i };
                yield return new AbstractOperation<OpType>(i, i, OpType.g); // { OP = OpType.g, I = i, J = i };

                // left neighbour
                if (i > 1)
                {
                    yield return new AbstractOperation<OpType>(i, i - 1, OpType.g); // { OP = OpType.g, I = i, J = i - 1 };
                }

                // right neighbour
                if (i < N)
                {
                    yield return new AbstractOperation<OpType>(i, i + 1, OpType.g); // { OP = OpType.g, I = i, J = i + 1 };
                }
            }

            // finish with untiling
            yield return new AbstractOperation<OpType>(OpType.Untile); // { OP = OpType.Untile };

            Log.TraceEvent(TraceEventType.Stop, 0, "Generator IsComplete");
        }

        enum OpType
        {
            Tile,
            cL,
            cR,
            dL,
            dLLU,
            dR,
            dRLU,
            gSum_ii,
            g,
            gLU_ii,
            Untile
        }        

        #endregion
    }

}