using System;

namespace TiledMatrixInversion.ParallelBlockMatrixInverter
{
    public class NonPipelinedStigsFormulae : IProducer<Action>
    {
        private readonly object _lock = new object();
        private IProducer<Action> _producer;
        private readonly IProducer<IProducer<Action>> _formulaProducer;

        public NonPipelinedStigsFormulae(IProducer<IProducer<Action>> formulaProducer)
        {
            _formulaProducer = formulaProducer;
            if(!_formulaProducer.TryGetNext(out _producer))
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
            lock (_lock)
            {                
                if(_producer.IsCompleted)
                {
                    if(_formulaProducer.IsCompleted)
                    {
                        action = null;
                        return false;
                    }
                    else
                    {
                        IProducer<Action> tmp;
                        if(!_formulaProducer.TryGetNext(out tmp))
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
}