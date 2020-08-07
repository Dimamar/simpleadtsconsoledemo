using System;
using System.Threading;

namespace SimpleADTSConsole.Scripts.Steps
{
    internal class StepWait : Scheduler.IStep
    {
        private DateTime _startTime;
        private readonly TimeSpan _waitTimout;

        /// <summary>
        /// Ожидать
        /// </summary>
        /// <param name="name"></param>
        /// <param name="waitTimout"></param>
        public StepWait(string name, TimeSpan waitTimout)
        {
            Name = name;
            _waitTimout = waitTimout;
        }

        public string Name { get; }

        public bool Run(CancellationToken cancel)
        {
            _startTime = DateTime.Now;
            return true;
        }

        public bool IsEnd(CancellationToken cancel)
        {
            return (DateTime.Now - _startTime) > _waitTimout;
        }
    }
}
