using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace SimpleADTSConsole.Scripts.Steps
{
    internal class StepToAim:Sheduller.IStep
    {
        private readonly int _callAfter;
        private int _callNumber = 0;
        private readonly ADTSConsoleModel _adts;
        private readonly PeriodDescriptor _period;
        private readonly double _aim;
        private readonly double _rate;

        /// <summary>
        /// Установить для ADTS цель
        /// </summary>
        /// <param name="name"></param>
        /// <param name="adts"></param>
        /// <param name="period"></param>
        /// <param name="aim"></param>
        /// <param name="rate"></param>
        /// <param name="callAfter"></param>
        public StepToAim(string name, ADTSConsoleModel adts, PeriodDescriptor period, double aim, double rate, int callAfter = 1)
        {
            Name = name;
            _adts = adts;
            _period = period;
            _aim = aim;
            _rate = rate;
            _callAfter = callAfter;
        }

        public string Name { get; }

        public bool Run(CancellationToken cancel)
        {
            return true;
        }

        public bool IsEnd(CancellationToken cancel)
        {
            return true;
        }

        public class PeriodDescriptor
        {
            public PeriodDescriptor(TimeSpan period)
            {
                Period = period;
            }

            public TimeSpan Period { get; set; }
        }
    }
}
