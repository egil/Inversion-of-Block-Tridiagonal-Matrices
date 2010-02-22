using System;
using System.Collections.Generic;
using System.Text;

namespace TiledMatrixInversion.Performance
{
    [Serializable]
    public class MessureContext
    {
        [NonSerialized]
        private Action<MessureContext> _reset;
        public Action<MessureContext> Reset
        {
            get { return _reset; }
            set { _reset = value; }
        }

        [NonSerialized]
        private Dictionary<string, object> _data;        
        public Dictionary<string, object> Data
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// Description of the performance messurement
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// How many interations each test should perform.
        /// </summary>
        public int Iterations { get; set; }
        /// <summary>
        /// If true, the  performance messurement will not run before Start is called.
        /// </summary>
        public bool CollectAndWait { get; set; }
        /// <summary>
        /// Specifies how many times the performance messurement is performed. Only used with CollectAndWait = true
        /// </summary>
        public int Replays { get; set; }
        /// <summary>
        /// If true, the results from the performance messurement is automatically saved to a file after completion.
        /// </summary>
        public bool AutoSaveToFile { get; set; }
        /// <summary>
        /// If AutoSaveToFile = true, this file name is used when saving the performance messurement results. 
        /// If AutoSaveFileName is null or empty, an file name is generated. 
        /// </summary>
        public string AutoSaveFileName { get; set; }

        public MessureContext()
        {
            Data = new Dictionary<string, object>();
            Replays = 1;
            Reset = (mc) => {  };
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Description);
            sb.AppendLine("Iterations: " + Iterations);
            sb.AppendLine("CollectAndWait: " + CollectAndWait);
            sb.AppendLine("Replays: " + Replays);
            sb.AppendLine("AutoSaveToFile: " + AutoSaveToFile);
            sb.AppendLine("AutoSaveFileName: " + AutoSaveFileName);
            foreach (var pair in _data)
            {                
                sb.AppendLine(pair.Key + ": " + pair.Value);
            }
            return sb.ToString();
        }
    }
    
}