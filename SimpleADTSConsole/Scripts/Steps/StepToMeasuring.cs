using System.Threading;

namespace SimpleADTSConsole.Scripts.Steps
{
    internal class StepToMeasuring:Sheduller.IStep
    {
        private readonly IADTSConsoleModel _adts;

        /// <summary>
        /// Перевести ADTS в режим измерения
        /// </summary>
        /// <param name="name"></param>
        /// <param name="adts"></param>
        public StepToMeasuring(string name, IADTSConsoleModel adts)
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

        public static bool Run(IADTSConsoleModel adts, CancellationToken cancel)
        {
            adts.SendReceve("SOUR:STAT OFF");
            return true;
        }
    }
}
