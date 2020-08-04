using System;
using System.Globalization;
using System.Threading;

namespace SimpleADTSConsole.Scripts.Steps
{
    internal class StepToAim:Sheduller.IStep
    {
        private string parameter = "PS";
        private string unit = "MMHG";
        private string unitCommandFormat = "UNIT:PRES {0}";
        private string aimCommandFormat = "SOUR:PRES {0},{1}";

        private readonly int _callAfter;
        private int _callNumber = 0;
        private readonly IADTSConsoleModel _adts;
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
        public StepToAim(string name, IADTSConsoleModel adts, PeriodDescriptor period, double aim, double rate, int callAfter = 1)
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
            _adts.SendReceve(string.Format(unitCommandFormat, unit));
            if (cancel.WaitHandle.WaitOne(_period.Period))
                return false;

            var aim = _aim.ToString("F0");
            _adts.SendReceve(string.Format("SOUR:RATE {0},{1}", parameter, _rate.ToString("F0")));
            if (cancel.WaitHandle.WaitOne(_period.Period))
                return false;

            _adts.SendReceve(string.Format(aimCommandFormat, parameter, aim));
            if (cancel.WaitHandle.WaitOne(_period.Period))
                return false;

            _adts.SendReceve("STAT:OPER:EVEN?");

            return true;
        }

        public bool IsEnd(CancellationToken cancel)
        {
            _callNumber++;
            if (_callNumber < _callAfter)
                return false;
            _callNumber = 0;

            string answer;
            _adts.SendReceve("STAT:OPER:EVEN?", out answer);
            if (cancel.WaitHandle.WaitOne(_period.Period))
                return false;

            int intVal;
            if (!int.TryParse(answer, NumberStyles.Any, CultureInfo.InvariantCulture, out intVal))
                return false;
            var registerEvents = (ADTSStatus)intVal;
            return (registerEvents & ADTSStatus.StableAtAimValue) != 0;
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
