using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace SimpleADTSConsole.Scripts.Steps
{
    internal class StepToGround : Sheduller.IStep
    {

        /// <summary>
        /// Перевести ADTS в режим снижения до давления земли
        /// </summary>
        /// <param name="name"></param>
        /// <param name="adts"></param>
        public StepToGround(string name, ADTSConsoleModel adts)
        {
            Name = name;
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
    }
}
