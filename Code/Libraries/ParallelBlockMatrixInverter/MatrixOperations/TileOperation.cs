using System;
using System.Collections.Generic;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.ParallelBlockMatrixInverter.Enumerators;
using TiledMatrixInversion.ParallelBlockMatrixInverter.OperationResults;
using TiledMatrixInversion.Resources;

namespace TiledMatrixInversion.ParallelBlockMatrixInverter.MatrixOperations
{
    public class TileOperation<T> : IProducer<Action>
    {
        
        private readonly object _lock = new object();
        private readonly OperationResult<T>[,] _result;
        private readonly BlockTridiagonalMatrix<T> _input;
        private readonly UnsortedOperationEnumerator<Action> _gen;


        public TileOperation(BlockTridiagonalMatrix<T> input, int tileSize, out OperationResult<T>[,] result)
        {
            _input = input;
            _result = result = new OperationResult<T>[input.Size + 1, 3];
            _gen = new UnsortedOperationEnumerator<Action>(ActionGenerator(tileSize).GetEnumerator(), Constants.MAX_QUEUE_LENGTH);
        }

        public bool IsCompleted { get { return _gen.Completed; } }

        public bool TryGetNext(out Action action)
        {
            lock (_lock)
            {
                action = _gen.Find(x => true);
                return action != null;
            }
        }

        private IEnumerable<Action> ActionGenerator(int tileSize)
        {
            for (int i = 1; i <= _input.Size; i++)
            {
                var locali = i;
                if (i > 1)
                {
                    yield return () =>
                                     {
                                         //Debug.WriteLine(Thread.CurrentThread.Name + " tiling BTM[" + locali + ", " + (locali - 1) + "]");
                                         _result[locali, 0] =
                                             new OperationResult<T>(
                                                 BlockTridiagonalMatrix<T>.TileMatrix(_input[locali, locali - 1],
                                                                                      tileSize),
                                                 true /* completed */);
                                     };
                }
                yield return () =>
                                 {
                                     //Debug.WriteLine(Thread.CurrentThread.Name + " tiling BTM[" + locali + ", " + locali + "]");
                                     _result[locali, 1] =
                                         new OperationResult<T>(
                                             BlockTridiagonalMatrix<T>.TileMatrix(_input[locali, locali], tileSize),
                                             true /* completed */);
                                 };
                if (i < _input.Size)
                {
                    yield return () =>
                                     {
                                         //Debug.WriteLine(Thread.CurrentThread.Name + " tiling BTM[" + locali + ", " + (locali + 1) + "]");
                                         _result[locali, 2] =
                                             new OperationResult<T>(
                                                 BlockTridiagonalMatrix<T>.TileMatrix(_input[locali, locali + 1],
                                                                                      tileSize)
                                                 , true /* completed */);

                                     };
                }
            }
        }
    }

}