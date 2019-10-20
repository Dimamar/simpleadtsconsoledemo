using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using SimpleADTSConsole.Scripts.Steps;

namespace SimpleADTSConsole
{
    public class ConsoleViewModel : INotifyPropertyChanged, IDisposable
    {
        #region Fields

        private Dictionary<string, string> _themes = new Dictionary<string, string>()
        {
            {"Blue"     , "pack://application:,,,/MahApps.Metro;component/Styles/Accents/Blue.xaml"},
            {"Brown"    , "pack://application:,,,/MahApps.Metro;component/Styles/Accents/brown.xaml"},
            {"Amber"    , "pack://application:,,,/MahApps.Metro;component/Styles/Accents/amber.xaml"},
            {"Basedark" , "pack://application:,,,/MahApps.Metro;component/Styles/Accents/basedark.xaml"},
            {"Baselight", "pack://application:,,,/MahApps.Metro;component/Styles/Accents/baselight.xaml"},
            {"Cobalt"   , "pack://application:,,,/MahApps.Metro;component/Styles/Accents/cobalt.xaml"},
            {"Crimson"  , "pack://application:,,,/MahApps.Metro;component/Styles/Accents/crimson.xaml"},
            {"Cyan"     , "pack://application:,,,/MahApps.Metro;component/Styles/Accents/cyan.xaml"},
            {"Emerald"  , "pack://application:,,,/MahApps.Metro;component/Styles/Accents/emerald.xaml"},
            {"Green"    , "pack://application:,,,/MahApps.Metro;component/Styles/Accents/green.xaml"},
            {"Indigo"   , "pack://application:,,,/MahApps.Metro;component/Styles/Accents/indigo.xaml"},
        };

        private readonly ADTSConsoleModel _model;
        private TimeSpan _realPeriod = TimeSpan.FromMilliseconds(100);
        private Thread _treadPeriodic = null;
        private Thread _treadUpDown = null;
        private Dispatcher _dispatcher;

        private ConcurrentQueue<Tuple<string, bool>> _queue = new ConcurrentQueue<Tuple<string, bool>>();

        private CancellationTokenSource _cancellation = new CancellationTokenSource();
        private ObservableCollection<string> _log = new ObservableCollection<string>();
        private int maxLog = 1000;
        private IEnumerable<Tuple<string, string>> _parameters;
        private string _periodUpDown = "120";
        private TimeSpan _periodRealUpDown = TimeSpan.FromSeconds(120);
        private string _ratePs = "100";
        private CancellationTokenSource _cancellationReadFromFile = new CancellationTokenSource();
        private IDisposable _unsubscriber;
        private ObservableCollection<IAdtsScript> _scripts;
        private bool _isControlMode;
        private Action<string, string> _showMsg;

        #endregion

        #region Constructor

        public ConsoleViewModel(ADTSConsoleModel model, Dispatcher dispatcher, Action<string, string> showMsg)
        {
            _dispatcher = dispatcher;
            _model = model;
            _showMsg = showMsg ?? ((title, msg) => { });
            Init();
        }

        private void Init()
        {
            _model.LogUpdate += _model_LogUpdate;
            _parameters = new List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Высота",""),
                new Tuple<string, string>("Калибровочная скорость", ""),
                new Tuple<string, string>("Истинная воздушная скорость", ""),
                new Tuple<string, string>("Махи", ""),
                new Tuple<string, string>("Отношение давления в двигателе", ""),
                new Tuple<string, string>("Статическое давление", ""),
                new Tuple<string, string>("Полное (динамическое) давление", ""),
                new Tuple<string, string>("Дифференциальное давление", ""),
            };
            IsBusy = false;
            IsOpened = false;
            Period = 100;
            UpPs = "1060";
            DownPs = "760";
            SelectedParameter = _parameters.FirstOrDefault();
            Statistic = new CommandSniffer(_dispatcher);
            _unsubscriber = _model.Subscribe(Statistic);
            var statPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                Properties.Settings.Default.BaseStatisticPath);
            _scripts = new ObservableCollection<IAdtsScript>(new IAdtsScript[]
            {
                new AdtsMeasuringRepeatsScript(statPath),
            });
            SelectedSctipt = _scripts.First();
            IsMetrogyMode = false;
        }

        #endregion

        public ICommand ChangeTheme
        {
            get
            {
                return new CommandWrapper((arg) =>
                {
                    var theme = _themes[arg as string];
                    var app = (App)Application.Current;
                    app.ChangeTheme(new Uri(theme));
                });
            }
        }

        public ICommand ChangeLng
        {
            get
            {
                return new CommandWrapper((arg) =>
                {
                    var lng = arg as string;
                    var app = (App)Application.Current;
                    app.ChangeLang(lng);
                });
            }
        }

        public ICommand SwitchConnect
        {
            get
            {
                return new CommandWrapper(() =>
                {
                    if (!IsOpened)
                    {
                        var cancel = _cancellation.Token;
                        this.WithBusyAsync(() => DoConnect(cancel), cancel);
                    }
                    else
                    {
                        var cancel = _cancellation.Token;
                        this.WithBusyAsync(DoDisconnect, cancel);
                    }
                });
            }
        }

        public ICommand Send { get { return this.GetCmdTryBusyAsync(() => DoSend(Command, false)); } }

        public ICommand SendReceive { get { return this.GetCmdTryBusyAsync(() => DoSend(Command, true)); } }

        public ICommand StartPeriodic
        {
            get
            {
                return new CommandWrapper(() =>
                {
                    if (_treadPeriodic == null)
                    {
                        var cancel = _cancellation.Token;
                        _realPeriod = TimeSpan.FromMilliseconds(Period);
                        _queue.Enqueue(new Tuple<string, bool>(PeriodicCommand, true));
                        _treadPeriodic = new Thread(() => PeriodicQuery(cancel));
                        _treadPeriodic.Start();
                    }
                    else
                    {
                        _realPeriod = TimeSpan.FromMilliseconds(Period);
                    }
                });
            }
        }

        public ICommand UpDownTest
        {
            get
            {
                return new CommandWrapper(_upDownTest);
            }
        }

        public ICommand GoToGround { get { return this.GetCmdTryBusyAsync(_goToGround); } }

        public ICommand CompilSetValue
        {
            get
            {
                return new CommandWrapper(() =>
                {
                });
            }
        }

        public ICommand CompilGetValue
        {
            get
            {
                return new CommandWrapper(() =>
                {
                });
            }
        }

        public ICommand CompilToControl
        {
            get
            {
                return new CommandWrapper(() =>
                {
                });
            }
        }

        public ICommand StopPeriodic
        {
            get
            {
                return new CommandWrapper(() =>
                    {
                        _cancellation.Cancel();
                        _cancellation = new CancellationTokenSource();
                        _treadPeriodic = null;
                    });
            }
        }

        public ICommand OpenFile { get { return new CommandWrapper(DoOpenFile); } }

        public ICommand StartFromFile
        {
            get
            {
                return new CommandWrapper(() =>
                {
                    if (!IsStarted)
                    {
                        var task = new Task(DoStartFromFile);
                        task.Start(TaskScheduler.Default);
                    }
                    else
                        DoStopFromFile();
                });
            }
        }

        public bool IsBusy { get; set; }

        public string Path { get; set; }

        public bool IsOpened { get; set; }

        public bool IsStarted { get; set; }

        public IEnumerable<Tuple<string, string>> Parameters
        {
            get { return _parameters; }
        }

        public Tuple<string, string> SelectedParameter { get; set; }

        public string Value { get; set; }

        public int Period { get; set; }

        public string PeriodicCommand { get; set; }

        public string Command { get; set; }

        public string PeriodUpDown
        {
            get { return _periodUpDown; }
            set
            {
                _periodUpDown = value;
                OnPropertyChanged("PeriodUpDown");
            }
        }

        public string UpPs { get; set; }

        public string DownPs { get; set; }

        public string RatePs
        {
            get { return _ratePs; }
            set { _ratePs = value; }
        }

        public ObservableCollection<string> Log
        {
            get { return _log; }
        }

        public CommandSniffer Statistic { get; set; }

        //public ICommand AutoZeroOn
        //{
        //    get { return new CommandWrapper(()=>DoSwitchAutoZero(true)); }
        //}

        //public ICommand AutoZeroOff
        //{
        //    get { return new CommandWrapper(() => DoSwitchAutoZero(false)); }
        //}

        public bool IsAutoZeroChecked { get; set; }

        public ICommand SwitchAutoZero
        {
            get
            {
                return new CommandWrapper(() =>
                {
                    this.TryOrDisconnect(()=>DoSwitchAutoZero(IsAutoZeroChecked));
                });
            }
        }

        //public ICommand AutoLeakOn
        //{
        //    get { return new CommandWrapper(() => DoSwitchAutoLeak(true)); }
        //}

        //public ICommand AutoLeakOff
        //{
        //    get { return new CommandWrapper(() => DoSwitchAutoLeak(false)); }
        //}

        public bool IsAutoLeakChecked { get; set; }

        public ICommand SwitchAutoLeak
        {
            get
            {
                return new CommandWrapper(() =>
                {
                    this.TryOrDisconnect(() => DoSwitchAutoLeak(IsAutoLeakChecked));
                });
            }
        }

        public ObservableCollection<IAdtsScript> Scripts
        {
            get { return _scripts; }
        }

        public IAdtsScript SelectedSctipt { get; set; }

        public ICommand StartSelectScript
        {
            get
            {
                return new CommandWrapper(() =>
          {
              var run = new Task(() => DoStartScript(SelectedSctipt));
              run.Start(TaskScheduler.Default);
          });
            }
        }

        //public ICommand ToControl
        //{
        //    get { return new CommandWrapper(DoToControl);}
        //}

        //public ICommand ToMeasuring
        //{
        //    get { return new CommandWrapper(DoToMeasuring); }
        //}

        public bool IsControlMode
        {
            get { return _isControlMode; }
            set
            {
                _isControlMode = value;
                OnPropertyChanged("IsControlMode");
            }
        }

        public ICommand SwitchMode
        {
            get
            {
                return this.GetCmdTryBusyAsync(() =>
                  {
                      if (!IsControlMode)
                          DoToMeasuring();
                      else
                          DoToControl();
                  });
            }
        }

        public bool IsMetrogyMode { get; set; }

        public ICommand SwitchMetrologyMode
        {
            get { return this.GetCmdBusyAsync(DoSwitchMetrologyMode); }
        }

        #region Privates

        private void DoDisconnect()
        {
            try
            {
                _model.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            IsOpened = false;
        }

        private void DoConnect(CancellationToken cancel)
        {
            try
            {
                _model.Open();
                if (cancel.WaitHandle.WaitOne(_realPeriod))
                    return;

                DoUpdate(cancel);
                IsOpened = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //_showMsg("Подключение", "Ошибка подключения");
                IsOpened = false;
            }
        }

        private void DoUpdate(CancellationToken cancel)
        {
            IsAutoZeroChecked = true;
            if (cancel.WaitHandle.WaitOne(_realPeriod))
                return;

            IsAutoLeakChecked = true;
            if (cancel.WaitHandle.WaitOne(_realPeriod))
                return;

            IsControlMode = true;
        }

        private void DoSwitchMetrologyMode()
        {
            IsMetrogyMode = !IsMetrogyMode;
        }

        private void DoToControl()
        {
            StepToControl.Run(_model, _cancellation.Token);
        }

        private void DoToMeasuring()
        {
            StepToMeasuring.Run(_model, _cancellation.Token);
        }

        void _model_LogUpdate(string obj)
        {
            _dispatcher.Invoke((Action)delegate
            {

                while (_log.Count > maxLog)
                    _log.RemoveAt(0);
                _log.Add(obj);
            });
        }

        private void DoStartScript(IAdtsScript sctipt)
        {
            sctipt.Start(_model, _cancellation.Token);
        }

        private void DoSwitchAutoZero(bool state)
        {
        }

        private void DoSwitchAutoLeak(bool state)
        {
        }

        private void DoOpenFile()
        {
            var ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            var res = ofd.ShowDialog();
            if (res == null || !res.Value)
                return;
            Path = ofd.FileName;
        }

        private void DoStopFromFile()
        {
            _cancellationReadFromFile.Cancel();
            _cancellationReadFromFile = new CancellationTokenSource();
        }

        private void DoStartFromFile()
        {
            IsStarted = true;
            try
            {
                var cancel = _cancellationReadFromFile.Token;
                var cmdStream = new CommandsFromFile();
                foreach (var cmd in cmdStream.Parce(Path))
                {
                    if (cancel.IsCancellationRequested)
                        break;
                    _model.Send(cmd.Command);
                    if (cancel.WaitHandle.WaitOne(_realPeriod))
                        break;
                    _model.Read();
                }
            }
            finally
            {
                IsStarted = false;
            }

        }

        void DoSend(string msg, bool receive)
        {
            if (_treadPeriodic == null)
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
                _queue.Enqueue(new Tuple<string, bool>(msg, receive));
            }
        }

        void PeriodicQuery(CancellationToken cancel)
        {
            while (!cancel.IsCancellationRequested)
            {
                Thread.Sleep(_realPeriod);
                Tuple<string, bool> cmd;
                if (_queue.TryDequeue(out cmd))
                {
                    _model.Send(cmd.Item1);
                    if (cmd.Item2)
                    {
                        Thread.Sleep(_realPeriod);
                        _model.Read();
                    }
                    _queue.Enqueue(new Tuple<string, bool>(PeriodicCommand, true));
                }
            }
            _treadPeriodic = null;
        }

        void _upDownTest()
        {
            var cancel = _cancellation.Token;
            _treadUpDown = new Thread(() => _upDown(cancel));
            _treadUpDown.Start();
        }

        void _upDown(CancellationToken cancel)
        {
            bool up = true;
            _treadUpDown = null;
        }

        void _goToGround()
        {
        }

        #endregion

        #region Implementation of INotifiPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
        /// </summary>
        public void Dispose()
        {
            if (_unsubscriber != null)
                _unsubscriber.Dispose();
            _cancellation.Cancel();
            _cancellation = new CancellationTokenSource();
            _treadPeriodic = null;
            if (IsOpened)
                _goToGround();
        }

        #endregion
    }
}
