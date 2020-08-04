using System;
using System.Threading;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole.ForTest
{
    class MoqAdtsConsoleModel : IADTSConsoleModel
    {
        public event Action<string> LogUpdate;
        public event Action<string> LogErrorUpdate;

        public bool Send(string msg, bool answerIsNeed = false)
        {
            Console.WriteLine(msg + " need answer " + answerIsNeed);
            return true;
        }

        public string Read()
        {
            Thread.Sleep(TimeSpan.FromSeconds(120));
            return "Answer is Read";
        }

        public IDisposable Subscribe(IObserver<CommandAction> observer)
        {
            //throw new NotImplementedException();
            return null;
        }

        public bool SendReceve(string msg)
        {
            return true;
        }

        public bool SendReceve(string msg, out string answer)
        {
            answer = "";
            return true;
        }

        public bool Open()
        {
            return true;
        }

        public void Close()
        {

        }
    }
}
