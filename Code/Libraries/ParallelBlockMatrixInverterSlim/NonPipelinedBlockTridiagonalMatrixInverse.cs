using System;

namespace TiledMatrixInversion.ParallelBlockMatrixInverterSlim
{
    public class NonPipelinedBlockTridiagonalMatrixInverse : IProducer<Action>
    {
        private IProducer<Action> _producer;
        private readonly IProducer<IProducer<Action>> _formulaProducer;

        public NonPipelinedBlockTridiagonalMatrixInverse(IProducer<IProducer<Action>> formulaProducer)
        {
            _formulaProducer = formulaProducer;
            if (!_formulaProducer.TryGetNext(out _producer))
            {
                throw new ArgumentException("Nothing to produce = not supposed to happen!");
            }
        }

        public bool IsCompleted
        {
            get
            {
                /// legend: 
                ///     F = Not Completed
                ///     T = Completed
                ///     _ = Don't care (wildcard)
                /// 
                /// result table:
                /// 
                /// _producer    _formulaProducer    return value
                ///       .IsCompleted()
                /// ---------------------------------------------
                /// | T         | T                 | T
                /// | T         | F                 | F
                /// | F         | _                 | F
                return _formulaProducer.IsCompleted && _producer.IsCompleted;
            }
        }

        public bool TryGetNext(out Action action)
        {
            if (_producer.IsCompleted)
            {
                if (_formulaProducer.IsCompleted)
                {
                    action = null;
                    return false;
                }
                else
                {
                    IProducer<Action> tmp;
                    if (!_formulaProducer.TryGetNext(out tmp))
                    {
                        action = null;
                        return false;
                    }
                    else
                    {
                        _producer = tmp;
                    }
                }
            }

            return _producer.TryGetNext(out action);
        }
    }
}