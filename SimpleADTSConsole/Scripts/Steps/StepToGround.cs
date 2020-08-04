using System.Threading;

namespace SimpleADTSConsole.Scripts.Steps
{
    internal class StepToGround : Sheduller.IStep
    {
        private readonly IADTSConsoleModel _adts;

        /// <summary>
        /// Перевести ADTS в режим снижения до давления земли
        /// </summary>
        /// <param name="name"></param>
        /// <param name="adts"></param>
        public StepToGround(string name, IADTSConsoleModel adts)
        {
            Name = name;
            _adts = adts;
        }

        public string Name { get; }

        public bool Run(CancellationToken cancel)
        {
            _adts.SendReceve("SOUR:GTGR");
            return true;
        }

        public bool IsEnd(CancellationToken cancel)
        {
            return true;
        }
    }
}
