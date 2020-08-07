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
using SimpleADTSConsole.AdjustingMode.Model;
using SimpleADTSConsole.Main.Model;
using SimpleADTSConsole.MetrologyMode;
using SimpleADTSConsole.Properties;
using SimpleADTSConsole.Scripts.Steps;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole
{
    public class ConsoleVM : INotifyPropertyChanged, IDisposable, IStatus
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

        private readonly BusySynchronizer _synchronizer;

        private readonly IConsoleModel _model;

        private CancellationTokenSource _cancellation = new CancellationTokenSource();
        private int maxLog = 1000;
        #endregion

        #region Constructor

        public ConsoleVM(IConsoleModel model, AdjustingModelVM adjusting, MetrologyVM metrology, LogReaderVM logReaderReder, BusySynchronizer synchronizer)
        {
            Adjusting = adjusting;
            Metrology = metrology;
            LogReader = logReaderReder;
            Log = new ObservableCollection<string>();
            _synchronizer = synchronizer;
            _synchronizer.UpdateStatusHolder(this);
            _model = model;
            _model.LogUpdated += LogUpdate;

            IsBusy = false;
            IsOpened = false;
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

        public ICommand SwitchConnect
        {
            get
            {
                return new CommandWrapper(() =>
                {
                    var isOpened = IsOpened;
                    var token = _cancellation.Token;
                    _synchronizer.WrapBusyAsync(() =>
                    {
                        if (!isOpened)
                        {
                            return _model.DoConnect(token);
                        }
                        else
                        {
                            _model.DoDisconnect();
                            return false;
                        }
                    }, res => IsOpened = res, token);
                });
            }
        }

        public AdjustingModelVM Adjusting { get; }

        public MetrologyVM Metrology { get; }

        public LogReaderVM LogReader { get; }

        public bool IsBusy { get; set; }

        public bool IsOpened { get; set; }


        public ObservableCollection<string> Log { get; private set; }

        public bool IsMetrogyMode { get; set; }


        public ICommand SwitchMetrologyMode
        {
            get { return this.GetCmdBusyAsync(DoSwitchMetrologyMode); }
        }

        #region Privates

        private void LogUpdate(IEnumerable<string> obj)
        {
            foreach (var line in obj)
            {
                if(Log.Count >= maxLog)
                    Log.RemoveAt(0);
                Log.Add(line);
            }
        }

        private void DoSwitchMetrologyMode()
        {
            IsMetrogyMode = !IsMetrogyMode;
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
            _cancellation.Cancel();
            _cancellation = new CancellationTokenSource();
            _model.Dispose();
        }

        #endregion
    }
}
