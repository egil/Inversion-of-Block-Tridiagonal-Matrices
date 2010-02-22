using System;
using System.Collections.Generic;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.Enumerators;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.OperationResults;
using TiledMatrixInversion.Resources;

namespace TiledMatrixInversion.ParallelBlockMatrixInverterSlim.MatrixOperations
{
    public class TileOperation<T> : IProducer<Action>
    {
        private readonly OperationResult<T>[][] _result;
        private readonly BlockTridiagonalMatrix<T> _input;
        private readonly OperationEnumerator<Action> _gen;

        public TileOperation(BlockTridiagonalMatrix<T> input, int tileSize, out OperationResult<T>[][] result)
        {
            _input = input;
            _result = result = Helpers.Init<OperationResult<T>>(input.Size + 1, 3);
            _gen = new OperationEnumerator<Action>(ActionGenerator(tileSize), Constants.MAX_QUEUE_LENGTH);
        }

        public bool IsCompleted { get { return _gen.Completed; } }

        public bool TryGetNext(out Action action)
        {
            action = _gen.Find(x => true);
            return action != null;
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
                                         _result[locali][0] =
                                             new OperationResult<T>(
                                                 BlockTridiagonalMatrix<T>.TileMatrix(_input[locali, locali - 1],
                                                                                      tileSize), true /* completed */);
                                     };
                }
                yield return () =>
                                 {
                                     _result[locali][1] =
                                         new OperationResult<T>(
                                             BlockTridiagonalMatrix<T>.TileMatrix(_input[locali, locali], tileSize),
                                             true /* completed */);
                                 };
                if (i < _input.Size)
                {
                    yield return () =>
                                     {
                                         _result[locali][2] =
                                             new OperationResult<T>(
                                                 BlockTridiagonalMatrix<T>.TileMatrix(_input[locali, locali + 1],
                                                                                      tileSize), true /* completed */);

                                     };
                }
            }
        }
    }
}