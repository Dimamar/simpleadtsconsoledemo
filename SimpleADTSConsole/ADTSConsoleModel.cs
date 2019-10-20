using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleADTSConsole
{
    public class ADTSConsoleModel:IObservable<CommandAction>
    {
        private int id;
        private string _lastCmd = string.Empty;
        zGPIBDataTransfer _transfer;
        private NLog.Logger _logger;

        private List<IObserver<CommandAction>> _observers = new List<IObserver<CommandAction>>();

        public ADTSConsoleModel()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _transfer = new zGPIBDataTransfer();
            _transfer.ToLogAction = OnLogUpdate;
        }

        public bool Open()
        {
            id = _transfer.Open(0, 1);
            return id == 0;
        }

        public void Close()
        {
            _transfer.Clear(id);
        }

        public bool Send(string msg)
        {
            _lastCmd = msg;
            return _transfer.Send(id, msg);
        }

        public string Read()
        {
            var answer = _transfer.Receive(id) as string;
            OnNext(new CommandAction() {Command = _lastCmd, Answer = answer, Timestamp = DateTime.Now});
            return answer;
        }

        public bool SendReceve(string msg)
        {
            if (!Send(msg))
                return false;
            Read();
            return true;
        }

        public bool SendReceve(string msg, out string answer)
        {
            answer = string.Empty;
            if (!Send(msg))
                return false;
            answer = Read();
            return true;
        }

        public event Action<string> LogUpdate;

        protected virtual void OnLogUpdate(string obj)
        {
            var handler = LogUpdate;
            if (handler != null) handler(obj);
            _logger.Trace(obj);
        }

        #region IObservable<CommandAction>

        public IDisposable Subscribe(IObserver<CommandAction> observer)
        {
            _observers.Add(observer);
            return new ObserverDisp(this, observer);
        }

        private void Unsubscribe(IObserver<CommandAction> observer)
        {
            _observers.Remove(observer);
        }

        private void OnNext(CommandAction cmd)
        {
            _observers.ForEach(el=>el.OnNext(cmd));
        }

        private class ObserverDisp:IDisposable
        {
            private readonly ADTSConsoleModel _observable;
            private readonly IObserver<CommandAction> _observer;
            
            public ObserverDisp(ADTSConsoleModel observable, IObserver<CommandAction> observer)
            {
                _observable = observable;
                _observer = observer;
            }

            public void Dispose()
            {
                _observable.Unsubscribe(_observer);
            }
        }

        #endregion
    }
}
