using System;
using System.Collections.Generic;
using System.Threading;

namespace SimpleADTSConsole.Main.Model
{
    public interface IConsoleModel : IDisposable
    {
        void DoDisconnect();
        bool DoConnect(CancellationToken cancel);
        event Action<IEnumerable<string>> LogUpdated;
    }
}