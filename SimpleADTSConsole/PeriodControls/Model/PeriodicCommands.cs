using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SimpleADTSConsole.PeriodControls;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole
{
    public class PeriodicCommands : IDisposable
    {
        private readonly IADTSTransportModel _model;
        private TimeSpan _realPeriod = TimeSpan.FromMilliseconds(100);
        private Thread _threadPeriodic = null;

        private ConcurrentQueue<Command> _queueLoop = new ConcurrentQueue<Command>();
        private CancellationTokenSource _cancellation = new CancellationTokenSource();

        public PeriodicCommands(IADTSTransportModel model)
        {
            _model = model;
        }

        public TimeSpan CurrentPeriod { get { return _realPeriod; } }

        /// <summary>
        /// Запуск команды
        /// Если процесс выполнения команд уже запущен, то добовляем в очередь текущую комманду на выполнение
        /// Команды выполниться ровно один раз
        /// Если процесс выполнения команд не запущен, то запуск производится в текущем потоке
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="receive"></param>
        public void DoSend(string msg, bool receive)
        {
            if (_threadPeriodic == null)
            {
                if (!receive)
                {
                    _model.Send(msg);
                }
                else
                {
                    _model.Send(msg);
                    Thread.Sleep(_realPeriod);
                    _model.Read();
                }
            }
            else
            {
                _queueLoop.Enqueue(new Command(msg, receive));
            }
        }

        public void DoStartPeriodic(Queue<Command> commands, IBusy busy)
        {
            if (_threadPeriodic != null)
                return;
            // Очищаем текущую очередь
            _queueLoop = new ConcurrentQueue<Command>(commands);
            _threadPeriodic = new Thread(() =>
            {
                busy.WithBusyAsync(PeriodicQuery, _cancellation.Token);
            });
            _threadPeriodic.Start();
        }

        private WaitHandle wh;
        private void PeriodicQuery(CancellationToken token)
        {
            wh = token.WaitHandle;
            // Основной цикл
            while (!token.IsCancellationRequested)
            {
                Command cmd;
                if (_queueLoop.TryDequeue(out cmd))
                {
                    _model.Send(cmd.TextCommand);
                    token.WaitHandle.WaitOne(cmd.Period);

                    if (cmd.AnswerIsNeed)
                    {
                        _model.Read();
                    }
                    if (cmd.Repeat)
                        _queueLoop.Enqueue(new Command(cmd));
                }
                else
                {
                    wh.WaitOne(TimeSpan.FromMilliseconds(10));
                }
            }
        }

        /// <summary>
        /// Останавливает процес выполнения команд, прерывая поток
        /// </summary>
        public void DoStopPeriodic()
        {
            if (_threadPeriodic == null)
                return;

            Dispose();
            wh.WaitOne();
            wh = null;
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
        /// </summary>
        public void Dispose()
        {
            _cancellation.Cancel();
            _cancellation = new CancellationTokenSource();
            _threadPeriodic = null;
        }

        #endregion
    }
}