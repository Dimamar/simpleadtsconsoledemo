using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using SimpleADTSConsole.PeriodControls;
using SimpleADTSConsole.Scripts;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole.AdjustingMode
{
    public class AdjustingModel : INotifyPropertyChanged, IDisposable
    {
        #region Fields

        private readonly IADTSConsoleModel _model;
        private readonly Func<Action, ICommand> _wrapBusyAsync;
        private readonly PeriodicCommands _periodicContext;
        private readonly Dispatcher _dispatcher;
        private readonly CancellationTokenSource _cancellationUpDown;

        private Task _treadUpDown = null;
        private IEnumerable<Tuple<string, string>> _parameters;
        private string _periodUpDown = "120";
        private string _ratePs = "100";
        private TimeSpan _periodRealUpDown = TimeSpan.FromSeconds(120);
        private ObservableCollection<IAdtsScript> _scripts;

        #endregion

        public AdjustingModel(IADTSConsoleModel model, PeriodicCommands periodicContext, Func<Action, ICommand> wrapAsync, Dispatcher dispatcher)
        {
            _cancellationUpDown = new CancellationTokenSource();
            _model = model;
            _dispatcher = dispatcher;
            _periodicContext = periodicContext;
            _wrapBusyAsync = wrapAsync;
            Init();
        }

        private void Init()
        {
            _loopContext = new PeriodVm(_dispatcher);
            _parameters = new List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Высота","ALT"),
                new Tuple<string, string>("Калибровочная скорость", "CAS"),
                new Tuple<string, string>("Истинная воздушная скорость", "TAS"),
                new Tuple<string, string>("Махи", "MACH"),
                new Tuple<string, string>("Отношение давления в двигателе", "EPR"),
                new Tuple<string, string>("Статическое давление", "PS"),
                new Tuple<string, string>("Полное (динамическое) давление", "PT"),
                new Tuple<string, string>("Дифференциальное давление", "QC"),
            };
            UpPs = "1060";
            DownPs = "760";
            SelectedParameter = _parameters.FirstOrDefault();
            var statPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                Properties.Settings.Default.BaseStatisticPath);

            _scripts = new ObservableCollection<IAdtsScript>(new IAdtsScript[]
            {
                new AdtsMeasuringRepeatsScript(statPath),
            });
            SelectedSctipt = _scripts.First();
        }

        #region Periodic

        private bool _commandStart;
        public bool CommandStart
        {
            get { return !_commandStart; }
            set
            {
                _commandStart = value;
                OnPropertyChanged("CommandStart");
            }
        }

        private PeriodVm _loopContext;
        public PeriodVm PeriodLoopContext => _loopContext;

        public ICommand StartPeriodic => new CommandWrapper(StartLoopCommands);

        private void StartLoopCommands()
        {
            if (CommandStart == false)
                return;

            var commands = PeriodLoopContext.GetCommands();
            _periodicContext.DoStartPeropdic(commands, PeriodLoopContext);
            CommandStart = true;
        }

        public ICommand StopPeriodic
        {
            get
            {
                return new CommandWrapper(() =>
                {
                    _periodicContext.DoStopPeriodic();
                    CommandStart = false;
                });
            }
        }

        #endregion

        #region Scripts

        public ObservableCollection<IAdtsScript> Scripts => _scripts;

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

        #endregion

        #region Simple /* Maybe sAmple? */ 

        public string Command { get; set; }

        public ICommand Send { get { return WrapBusyAsync(() => _periodicContext.DoSend(Command, false)); } }

        public ICommand SendReceive { get { return WrapBusyAsync(() => _periodicContext.DoSend(Command, true)); } }

        #endregion

        #region Command compill

        public Tuple<string, string> SelectedParameter { get; set; }

        public IEnumerable<Tuple<string, string>> Parameters => _parameters;

        public string Value { get; set; }

        public ICommand CompilSetValue
        {
            get
            {
                return new CommandWrapper(() =>
                {
                    Command = string.Format(CultureInfo.GetCultureInfo("en-US"), "SOUR:PRES {0},{1}",
                        SelectedParameter.Item2, Value);
                });
            }
        }

        public ICommand CompilGetValue
        {
            get
            {
                return new CommandWrapper(() =>
                {
                    Command = string.Format(CultureInfo.GetCultureInfo("en-US"), "MEAS:PRES? {0}",
                        SelectedParameter.Item2);
                });
            }
        }

        public ICommand CompilToControl
        {
            get
            {
                return new CommandWrapper(() =>
                {
                    Command = "SOUR:STAT ON";
                });
            }
        }

        #endregion

        #region Up down Test

        // Up Down Test
        public string UpPs { get; set; }

        public string PeriodUpDown
        {
            get { return _periodUpDown; }
            set
            {
                if (_periodUpDown == value)
                    return;
                double sec;
                if (double.TryParse(value, out sec))
                {
                    _periodRealUpDown = TimeSpan.FromSeconds(sec);
                }
                else
                {
                    return;
                }
                _periodUpDown = value;
                OnPropertyChanged("PeriodUpDown");
            }
        }

        public ICommand UpDownTest
        {
            get
            {
                return new CommandWrapper(_upDownTest);
            }
        }

        // Go To Ground Test

        public string DownPs { get; set; }

        public string RatePs
        {
            get { return _ratePs; }
            set
            {
                if (_ratePs == value)
                    return;
                _ratePs = value;
                if (_treadUpDown != null)
                {
                    var cmd = string.Format(CultureInfo.GetCultureInfo("en-US"), "SOUR:RATE PS,{0}", _ratePs);
                    _periodicContext.DoSend(cmd, false);
                }
                OnPropertyChanged("RatePs");
            }
        }

        public ICommand GoToGround { get { return WrapBusyAsync(_goToGround); } }

        #endregion

        #region Privates

        private ICommand WrapBusyAsync(Action action)
        {
            return _wrapBusyAsync(action);
        }

        private void DoStartScript(IAdtsScript sctipt)
        {
            sctipt.Start(_model, _cancellationUpDown.Token);
        }

        void _upDownTest()
        {
            var cancel = _cancellationUpDown.Token;
            _treadUpDown = new Task(() => _upDown(cancel));
            _treadUpDown.Start();
        }

        void _upDown(CancellationToken cancel)
        {
            bool up = true;
            var cmd = "SOUR:STAT ON";
            _periodicContext.DoSend(cmd, false);

            cmd = string.Format(CultureInfo.GetCultureInfo("en-US"), "SOUR:RATE PS,{0}", _ratePs);
            _periodicContext.DoSend(cmd, false);
            while (!cancel.IsCancellationRequested)
            {
                cmd = string.Format(CultureInfo.GetCultureInfo("en-US"), "SOUR:PRES PS,{0}", up ? UpPs : DownPs);
                _periodicContext.DoSend(cmd, false);

                var startTime = DateTime.Now;
                while (!cancel.IsCancellationRequested)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(10));
                    if ((DateTime.Now - startTime) > _periodRealUpDown)
                        break;
                }
                up = !up;
            }
            _treadUpDown = null;
        }

        void _goToGround()
        {
            var cmd = "SOUR:GTGR";
            _periodicContext.DoSend(cmd, false);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
        /// </summary>
        public void Dispose()
        {
            _cancellationUpDown.Cancel();
        }
        #endregion
    }
}
