using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using TiledMatrixInversion.ParallelBlockMatrixInverter;

namespace TiledMatrixInversion.ParallelBlockMatrixInverter
{
    public class PipelinedStigsFormulae : IProducer<Action>
    {
        private static TraceSource Log = new TraceSource("PipelinedStigsFormulae");
        private readonly object _lock = new object();
        private static int MAX_QUEUE_LENGTH = 2;
        private List<IProducer<Action>> _activeProducers;
        private readonly IProducer<IProducer<Action>> _formulaProducer;

        public PipelinedStigsFormulae(IProducer<IProducer<Action>> formulaProducer)
        {
            _formulaProducer = formulaProducer;
            _activeProducers = new List<IProducer<Action>>(MAX_QUEUE_LENGTH);
        }

        #region Implementation of IProducer<Action>

        public bool IsCompleted
        {
            get
            {
                return _formulaProducer.IsCompleted && _activeProducers.All(x => x.IsCompleted);
            }
        }

        public bool TryGetNext(out Action action)
        {
            lock (_lock)
            {
                FillQueue();

                action = null;

                for (int i = 0; i < _activeProducers.Count; i++)
                {
                    if (_activeProducers[i].TryGetNext(out action))
                    {
                        return true;
                    }

                    if (_activeProducers[i].IsCompleted)
                        _activeProducers.Remove(_activeProducers[i]);
                }

                return false;
            }
        }

        void FillQueue()
        {            
            // generate data for queue
            if (_activeProducers.Count < MAX_QUEUE_LENGTH)
            {
                Log.TraceEvent(TraceEventType.Start, 0, "Add more producers to queue, count = {0}", _activeProducers.Count);
                
                IProducer<Action> producer;
                while (_activeProducers.Count < MAX_QUEUE_LENGTH && _formulaProducer.TryGetNext(out producer))
                {
                    _activeProducers.Add(producer);
                }
                
                Log.TraceEvent(TraceEventType.Stop, 0, "Add more producers to queue, count = {0}", _activeProducers.Count);
            }
        }

        #endregion
    }
}