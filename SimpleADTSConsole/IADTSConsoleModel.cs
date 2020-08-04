using System;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole
{
    public interface IADTSConsoleModel
    {
        bool Open();
        void Close();
        bool Send(string msg, bool NeedAnswer = false);
        string Read();
        IDisposable Subscribe(IObserver<CommandAction> observer);
        bool SendReceve(string msg);
        bool SendReceve(string msg, out string answer);

        event Action<string> LogUpdate;
        event Action<string> LogErrorUpdate;
    }
}