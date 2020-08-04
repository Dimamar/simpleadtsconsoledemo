using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using SimpleADTSConsole.AdjustingMode;
using SimpleADTSConsole.MetrologyMode;
using SimpleADTSConsole.Properties;
using SimpleADTSConsole.Scripts.Steps;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole
{
    public class ConsoleViewModel : INotifyPropertyChanged, IDisposable, IStatus
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

        private readonly IADTSConsoleModel _model;
        private readonly PeriodicCommands _periodicSync;
        private readonly AdjustingModel _adjusting;
        private readonly AdtsModeModel _AdtsMode;
        private readonly Dispatcher _dispatcher;

        private CsvWriter _csvWriter;
        private CancellationTokenSource _cancellation = new CancellationTokenSource();
        private readonly ObservableCollection<string> _log = new ObservableCollection<string>();
        private int maxLog = 1000;
        private IDisposable _unsubscriber;
        private bool _isControlMode;
        private Action<string, string> _showMsg;
        private readonly PeriodicCommands _periodicCommands;

        #endregion

        #region Constructor

        public ConsoleViewModel(IADTSConsoleModel model, Dispatcher dispatcher, Action<string, string> showMsg)
        {
            _dispatcher = dispatcher;
            _model = model;
            _periodicSync = new PeriodicCommands(_model);
            _adjusting = new AdjustingModel(model, _periodicSync, this.GetCmdTryBusyAsync, dispatcher);

            _periodicCommands = new PeriodicCommands(_model);
            _AdtsMode = new AdtsModeModel(_model, _periodicSync, this.GetCmdTryBusyAsync, _cancellation.Token);
            _showMsg = showMsg ?? ((title, msg) => { });
            Init();
        }

        private void Init()
        {
            _model.LogUpdate += _model_LogUpdate;
            _model.LogErrorUpdate += ModelOnLogErrorUpdate;
            IsBusy = false;
            IsOpened = false;
            _csvWriter = new CsvWriter(Settings.Default.BaseStatisticPath);
            Statistic = new CommandSniffer(_dispatcher, _csvWriter);
            _unsubscriber = _model.Subscribe(Statistic);
            IsMetrogyMode = false;
            Logs = new LogReader(_model, _periodicCommands.CurrentPeriod);
            _periodicCommands.RealPeriodUpdated += (newPeriod) => Logs.UpdatePeriod(newPeriod);
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

        public AdjustingModel Adjusting { get { return _adjusting; } }

        public AdtsModeModel AdtsMode { get { return _AdtsMode; } }

        public LogReader Logs { get; private set; }

        public bool IsBusy { get; set; }

        public bool IsOpened { get; set; }

        public ObservableCollection<string> Log
        {
            get { return _log; }
        }

        public CommandSniffer Statistic { get; set; }

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
                if (cancel.WaitHandle.WaitOne(_periodicCommands.CurrentPeriod))
                    return;

                var cmd = "*IDN?";
                _model.Send(cmd);
                var answer = _model.Read();
                if (string.IsNullOrEmpty(answer) || answer == "?")
                {
                    IsOpened = false;
                    return;
                }
                if (cancel.WaitHandle.WaitOne(_periodicCommands.CurrentPeriod))
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
            var cmd = "CALC:AZER?";
            _model.Send(cmd);
            var answer = _model.Read();
            IsAutoZeroChecked = answer == "ON";
            if (cancel.WaitHandle.WaitOne(_periodicCommands.CurrentPeriod))
                return;

            cmd = "SOUR:MODE:ALE?";
            _model.Send(cmd);
            answer = _model.Read();
            IsAutoLeakChecked = answer == "ON";
            if (cancel.WaitHandle.WaitOne(_periodicCommands.CurrentPeriod))
                return;

            cmd = "SOUR:STAT?";
            _model.Send(cmd);
            answer = _model.Read();
            IsControlMode = answer == "ON";
        }

        private void DoSwitchMetrologyMode()
        {
            IsMetrogyMode = !IsMetrogyMode;
        }

        ConcurrentQueue<string> _buufer = new ConcurrentQueue<string>();
        Stopwatch _watch = new Stopwatch();
        private const int WaitingTime = 150;

        void _model_LogUpdate(string obj)
        {
            if (!_watch.IsRunning)
                _watch.Start();

            _buufer.Enqueue(obj);

            if (_watch.ElapsedMilliseconds > WaitingTime)
            {
                _watch.Stop();
                _dispatcher.Invoke((Action)delegate
                {
                    while (_log.Count > maxLog)
                        _log.RemoveAt(0);

                    string s;
                    while (_buufer.TryDequeue(out s))
                    {
                        _log.Add(s);
                    }
                    _watch.Start();
                });
            }
        }

        private void ModelOnLogErrorUpdate(string s)
        {
            
        }

        private void DoToControl()
        {
            StepToControl.Run(_model, _cancellation.Token);
        }

        private void DoToMeasuring()
        {
            StepToMeasuring.Run(_model, _cancellation.Token);
        }

        private void DoSwitchAutoZero(bool state)
        {
            var cmd = state ? "CALC:AZER ON" : "CALC:AZER OFF";
            _periodicCommands.DoSend(cmd, false);
        }

        private void DoSwitchAutoLeak(bool state)
        {
            var cmd = state ? "SOUR:MODE:ALE ON" : "SOUR:MODE:ALE OFF";
            _periodicCommands.DoSend(cmd, false);
        }

        void _goToGround()
        {
            var cmd = "SOUR:GTGR";
            _periodicCommands.DoSend(cmd, false);
        }

        #endregion

        #region Implementation of INotifiPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            _adjusting.Dispose();
            _csvWriter.Dispose();
            _periodicCommands.Dispose();
            if (IsOpened)
                _goToGround();
        }

        #endregion
    }
}
