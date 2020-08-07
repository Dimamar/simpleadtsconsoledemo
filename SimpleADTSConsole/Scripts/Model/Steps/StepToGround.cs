using System.Threading;

namespace SimpleADTSConsole.Scripts.Steps
{
    internal class StepToGround : Scheduler.IStep
    {
        private readonly IADTSTransportModel _adts;

        /// <summary>
        /// Перевести ADTS в режим снижения до давления земли
        /// </summary>
        /// <param name="name"></param>
        /// <param name="adts"></param>
        public StepToGround(string name, IADTSTransportModel adts)
        {
            Name = name;
            _adts = adts;
        }

        public string Name { get; }

        public bool Run(CancellationToken cancel)
        {
            _adts.SendReceive("SOUR:GTGR");
            return true;
        }

        public bool IsEnd(CancellationToken cancel)
        {
            return true;
        }
    }
}
