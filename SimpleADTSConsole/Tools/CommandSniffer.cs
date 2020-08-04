using System;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SimpleADTSConsole.Tools
{
    public class CommandSniffer : IObserver<CommandAction>, INotifyPropertyChanged
    {
        private readonly Dispatcher _dispatcher;
        private readonly StatisticData _statistic;
        private int _depthLog = 100;
        private Stopwatch _watch;

        public ObservableCollection<CommandAction> Log { get; private set; }
        public ConcurrentQueue<CommandAction> _bufferLog;
        public ObservableCollection<CurrentParameterState> States { get { return _statistic.States; } }
        private const int WaitingTime = 150;

        public CommandSniffer(Dispatcher dispatcher, ILogWriter writer)
        {
            _watch = new Stopwatch();
            _watch.Start();
            _bufferLog = new ConcurrentQueue<CommandAction>();
            _dispatcher = dispatcher;
            _statistic = new StatisticData(writer);
            Log = new ObservableCollection<CommandAction>();
        }

        public void OnNext(CommandAction value)
        {
            var task = new Task((arg) =>
            {
                _bufferLog.Enqueue(value);
                while (Log.Count >= _depthLog && _depthLog > 0)
                {
                    _dispatcher.Invoke(new Action(() => Log.RemoveAt(0)));
                }

                if (_watch.ElapsedMilliseconds > WaitingTime)
                {
                    _watch.Stop();
                    // Задержка перед выводом на экран если опрос слишком частый то без задержки UI виснет
                    _dispatcher.Invoke(new Action(() =>
                    {
                        CommandAction action;
                        while (_bufferLog.TryDequeue(out action))
                        {
                            Log.Add(action);
                        }
                        _watch.Start();
                    }));
                }
                _dispatcher.Invoke(new Action(() => _statistic.OnNext(value)));

            }, value);
            task.Start(TaskScheduler.Default);
        }

        public void OnError(Exception error)
        {
            _statistic.OnError(error);
        }

        public void OnCompleted()
        {
            _statistic.OnCompleted();
        }



        public int LogDepth
        {
            get
            {
                return _depthLog;
            }
            set
            {
                if (_depthLog == value)
                    return;
                _depthLog = value;
                OnPropertyChanged("LogDepth");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
