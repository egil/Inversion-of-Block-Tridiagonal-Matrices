using System;
using System.Diagnostics;
using System.Threading;

namespace TiledMatrixInversion.ParallelBlockMatrixInverterSlim
{
    public class PipelinedBlockTridiagonalMatrixInverse : IProducer<Action>
    {
        private static readonly TraceSource Log = new TraceSource("PipelinedBlockTridiagonalMatrixInverse");

        private IProducer<Action> _primaryProducer;
        private IProducer<Action> _secondaryProducer;
        private IProducer<IProducer<Action>> _formulaProducer;

        public PipelinedBlockTridiagonalMatrixInverse(IProducer<IProducer<Action>> formulaProducer)
        {
            _formulaProducer = formulaProducer;
        }

        #region Implementation of IProducer<Action>

        public bool IsCompleted
        {
            get
            {
                return _formulaProducer.IsCompleted && 
                    (_primaryProducer == null || _primaryProducer.IsCompleted) && 
                    (_secondaryProducer == null || _secondaryProducer.IsCompleted);
            }
        }

        public bool TryGetNext(out Action action)
        {
            action = null;

            // if primary producer is null or finished upgrade 
            // secondary producer to primary if one exists, otherwise create new
            if (_primaryProducer == null || _primaryProducer.IsCompleted)
            {
                // copy secondary producer to primary producer if it exists
                if (_secondaryProducer != null && !_secondaryProducer.IsCompleted)
                    _primaryProducer = _secondaryProducer;

                // else try to get a new producer for the primary producer
                else _formulaProducer.TryGetNext(out _primaryProducer); 
            }
            
            // try retriving work from primary producer
            if (_primaryProducer != null && _primaryProducer.TryGetNext(out action))
                return true;

            // if no work was not found in primary producer, see if a secondary is
            // available, and if not, try to get one.
            if (_secondaryProducer == null || _secondaryProducer.IsCompleted)
                _formulaProducer.TryGetNext(out _secondaryProducer);                

            // try retriving work from secondary producer
            if(_secondaryProducer != null && _secondaryProducer.TryGetNext(out action))
            {
                Log.TraceEvent(TraceEventType.Verbose, 1, "{0} : Found work in secondary producer.", Thread.CurrentThread.Name);
                Statistics.SecondaryProducerCount++;
                return true;
            }

            return false;
        }

        #endregion
    }
}