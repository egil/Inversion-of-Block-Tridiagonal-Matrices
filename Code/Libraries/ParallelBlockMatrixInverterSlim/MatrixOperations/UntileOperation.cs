using System;
using System.Collections.Generic;
using TiledMatrixInversion.Math;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.Enumerators;
using TiledMatrixInversion.ParallelBlockMatrixInverterSlim.OperationResults;
using TiledMatrixInversion.Resources;

namespace TiledMatrixInversion.ParallelBlockMatrixInverterSlim.MatrixOperations
{
    public class UntileOperation<T> : IProducer<Action>
    {
        private readonly BlockTridiagonalMatrix<T> _result;
        private readonly OperationResult<T>[][] _input;
        private readonly OperationEnumerator<AbstractOperation> _gen;

        public UntileOperation(OperationResult<T>[][] input, BlockTridiagonalMatrix<T> result)
        {
            _input = input;
            _result = result;
            _gen = new OperationEnumerator<AbstractOperation>(OperationGenerator(), Constants.MAX_QUEUE_LENGTH);
        }

        public bool IsCompleted { get { return _gen.Completed; } }

        public bool TryGetNext(out Action action)
        {
            var op = _gen.Find(x => _input[x.I][x.J].Completed);
            action = GenerateAction(op);
            return action != null;
        }

        private Action GenerateAction(AbstractOperation op)
        {
            if (op == null)
                return null;

            return () =>
                       {
                           _result[op.I, op.I + op.J - 1] =
                               TiledBlockTridiagonalMatrix<T>.UntileMatrix(_input[op.I][op.J].Data);
                       };
        }

        private IEnumerable<AbstractOperation> OperationGenerator()
        {
            var length = _input.GetLength(0) - 1;
            for (int i = 1; i <= length; i++)
            {
                if (i > 1)
                {
                    yield return new AbstractOperation(i); // { I = i, J = 0, OP = OpType.Op };
                }

                yield return new AbstractOperation(i, 1); // { I = i, J = 1, OP = OpType.Op };

                if (i < length)
                {
                    yield return new AbstractOperation(i, 2); // { I = i, J = 2, OP = OpType.Op };
                }
            }
        }

    }
}