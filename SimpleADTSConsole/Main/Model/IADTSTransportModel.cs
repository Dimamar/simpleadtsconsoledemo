using System;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole
{
    public interface IADTSTransportModel
    {
        bool Open();
        void Close();
        bool Send(string msg, bool NeedAnswer = false);
        string Read();
        bool SendReceive(string msg);
        bool SendReceive(string msg, out string answer);

        event Action<string> LogUpdate;
        event Action<string> LogErrorUpdate;
    }
}