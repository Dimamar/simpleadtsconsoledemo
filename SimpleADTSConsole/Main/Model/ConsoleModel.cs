using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace SimpleADTSConsole.Main.Model
{
    class ConsoleModel : IConsoleModel
    {
        private const int WaitingTime = 150;

        private readonly IADTSTransportModel _transportModel;
        private CancellationTokenSource _cancellation = new CancellationTokenSource();
        private readonly PeriodicCommands _periodicCommands;

        ConcurrentQueue<string> _buufer = new ConcurrentQueue<string>();
        Stopwatch _watch = new Stopwatch();


        public ConsoleModel(IADTSTransportModel transportModel, PeriodicCommands periodicCommands)
        {
            _transportModel = transportModel;
            _periodicCommands = periodicCommands;
            _transportModel.LogUpdate += TransportModelLogUpdate;
        }

        #region Privates

        public void DoDisconnect()
        {
            try
            {
                _transportModel.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public bool DoConnect(CancellationToken cancel)
        {
            try
            {
                _transportModel.Open();
                if (cancel.WaitHandle.WaitOne(_periodicCommands.CurrentPeriod))
                    return false;

                var cmd = "*IDN?";
                _transportModel.Send(cmd);
                var answer = _transportModel.Read();
                if (string.IsNullOrEmpty(answer) || answer == "?")
                {
                    return false;
                }
                if (cancel.WaitHandle.WaitOne(_periodicCommands.CurrentPeriod))
                    return false;

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //_showMsg("Подключение", "Ошибка подключения");
                return false;
            }
        }

        public event Action<IEnumerable<string>> LogUpdated;

        protected virtual void OnLogUpdated(IEnumerable<string> obj)
        {
            LogUpdated?.Invoke(obj);
        }

        void TransportModelLogUpdate(string obj)
        {
            if (!_watch.IsRunning)
                _watch.Start();

            _buufer.Enqueue(obj);

            if (_watch.ElapsedMilliseconds > WaitingTime)
            {
                _watch.Reset();
                var log = new List<string>();
                string s;
                while (_buufer.TryDequeue(out s))
                {
                    log.Add(s);
                }

                OnLogUpdated(log);
                _watch.Start();
            }
        }

        void _goToGround()
        {
            var cmd = "SOUR:GTGR";
            _periodicCommands.DoSend(cmd, false);
        }

        #endregion

        public void Dispose()
        {
            _goToGround();
        }
    }
}
