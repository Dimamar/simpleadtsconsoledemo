using System;
using IGPIBTransfer;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole
{
    public class AdtsTransportModel : IADTSTransportModel
    {
        private int id;
        private string _lastCmd = string.Empty;
        private readonly IzGPIBDataTransfer _transfer;
        private readonly NLog.Logger _logger;
        private readonly NLog.Logger _loggerError;

        private readonly IObservableUpdater<CommandAction> _observers;

        public event Action<string> LogUpdate;
        public event Action<string> LogErrorUpdate;

        public AdtsTransportModel(IzGPIBDataTransfer transfer, IObservableUpdater<CommandAction> observableUpdater)
        {
            _observers = observableUpdater;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _loggerError = NLog.LogManager.GetLogger("Error");
            _transfer = transfer;

            _transfer.ToLogAction = OnLogUpdate;
            _transfer.ToLogErrorAction = OnLogErrorUpdate;
        }

        #region IADTSTransportModel

        public bool Open()
        {
            id = _transfer.Open(0, 1);
            return id == 0;
        }

        public void Close()
        {
            _transfer.Clear(id);
        }

        public bool Send(string msg, bool answerIsNeed = false)
        {
            _lastCmd = msg;
            _observers.OnNext(new CommandAction
            {
                Command = msg,
                Answer = null,
                Timestamp = DateTime.Now,
                IsAnswer = false,
                AnswerIsNeed = answerIsNeed
            });
            return _transfer.Send(id, msg);
        }

        public string Read()
        {
            var answer = _transfer.Receive(id) as string;
            _observers.OnNext(new CommandAction
            {
                Command = _lastCmd,
                Answer = answer,
                Timestamp = DateTime.Now,
                IsAnswer = true
            });
            return answer;
        }

        public bool SendReceive(string msg)
        {
            if (!Send(msg))
                return false;
            Read();
            return true;
        }

        public bool SendReceive(string msg, out string answer)
        {
            answer = string.Empty;
            if (!Send(msg))
                return false;
            answer = Read();
            return true;
        }

        #endregion

        protected virtual void OnLogUpdate(string obj)
        {
            var handler = LogUpdate;
            if (handler != null)
                handler(obj);
            _logger.Trace(obj);
        }

        protected virtual void OnLogErrorUpdate(string obj)
        {
            var handler = LogErrorUpdate;
            if (handler != null)
                handler(obj);
            _loggerError.Trace(obj);
        }
    }
}
