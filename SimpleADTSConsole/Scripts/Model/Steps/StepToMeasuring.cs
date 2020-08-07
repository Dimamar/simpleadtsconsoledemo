using System.Threading;

namespace SimpleADTSConsole.Scripts.Steps
{
    internal class StepToMeasuring:Scheduler.IStep
    {
        private readonly IADTSTransportModel _adts;

        /// <summary>
        /// Перевести ADTS в режим измерения
        /// </summary>
        /// <param name="name"></param>
        /// <param name="adts"></param>
        public StepToMeasuring(string name, IADTSTransportModel adts)
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

        public static bool Run(IADTSTransportModel adts, CancellationToken cancel)
        {
            adts.SendReceive("SOUR:STAT OFF");
            return true;
        }
    }
}
