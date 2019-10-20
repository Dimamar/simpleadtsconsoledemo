using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace SimpleADTSConsole.Scripts.Steps
{
    internal class StepToMeasuring:Sheduller.IStep
    {
        private readonly ADTSConsoleModel _adts;

        /// <summary>
        /// Перевести ADTS в режим измерения
        /// </summary>
        /// <param name="name"></param>
        /// <param name="adts"></param>
        public StepToMeasuring(string name, ADTSConsoleModel adts)
        {
            Name = name;
            _adts = adts;
        }

        public string Name { get; }

        public bool Run(CancellationToken cancel)
        {
            return Run(_adts, cancel);
        }

        public bool IsEnd(CancellationToken cancel)
        {
            return true;
        }

        public static bool Run(ADTSConsoleModel adts, CancellationToken cancel)
        {
            return true;
        }
    }
}
