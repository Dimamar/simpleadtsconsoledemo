using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SimpleADTSConsole
{
    public class CommandSniffer:IObserver<CommandAction>, INotifyPropertyChanged
    {
        private readonly Dispatcher _dispatcher;

        private readonly StatisticData _statistic = new StatisticData();

        private int _depthLog = 100;

        public CommandSniffer(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            Log = new ObservableCollection<CommandAction>();
        }

        public void OnNext(CommandAction value)
        {
            var task = new Task((arg) =>
            {
                while (Log.Count >= _depthLog && _depthLog > 0)
                    _dispatcher.Invoke(new Action(() => Log.RemoveAt(0)));
                _dispatcher.Invoke(new Action(() => Log.Add(value)));

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

        public ObservableCollection<CommandAction> Log { get; private set; }

        public ObservableCollection<CurrentParameterState> States { get { return _statistic.States; } }

        public int LogDepth
        {
            get
            {
                return _depthLog;
            }
            set
            {
                if(_depthLog == value)
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

    /// <summary>
    ///  Описатель по одному параметру
    /// </summary>
    public class CurrentParameterState:INotifyPropertyChanged
    {
        /// <summary>
        /// Название параметра
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Ответ
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Последнее обновление состояния
        /// </summary>
        public DateTime LastTime { get; set; }
        /// <summary>
        /// Текущее количество повторов
        /// </summary>
        public int CurrentRepeats { get; set; }
        /// <summary>
        /// Общее количество повторов с момента накопления статистики
        /// </summary>
        public int CountAllRepeats { get; set; }
        /// <summary>
        /// Максимальное количество неприрывных повторов
        /// </summary>
        public int MaxCountRepeat { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
