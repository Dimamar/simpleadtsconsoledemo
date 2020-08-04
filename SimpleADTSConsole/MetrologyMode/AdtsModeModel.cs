using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;
using SimpleADTSConsole.Scripts.Steps;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole.MetrologyMode
{
    public class AdtsModeModel:INotifyPropertyChanged
    {
        private readonly IADTSConsoleModel _model;
        private readonly Func<Action, ICommand> _wrapBusyAsync;
        private readonly PeriodicCommands _periodicContext;
        private bool _isControlMode;
        private readonly CancellationToken _token;

        public AdtsModeModel(IADTSConsoleModel model, PeriodicCommands periodicContext, Func<Action, ICommand> wrapBusyAsync, CancellationToken token)
        {
            _model = model;
            _periodicContext = periodicContext;
            _wrapBusyAsync = wrapBusyAsync;
            _token = token;
        }

        public bool IsAutoZeroChecked { get; set; }

        public ICommand SwitchAutoZero
        {
            get
            {
                return new CommandWrapper(() =>
                {
                    _wrapBusyAsync(() => DoSwitchAutoZero(IsAutoZeroChecked));
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
                    _wrapBusyAsync(() => DoSwitchAutoLeak(IsAutoLeakChecked));
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
                return _wrapBusyAsync(() =>
                {
                    if (!IsControlMode)
                        DoToMeasuring();
                    else
                        DoToControl();
                });
            }
        }

        private void DoToControl()
        {
            StepToControl.Run(_model, _token);
        }

        private void DoToMeasuring()
        {
            StepToMeasuring.Run(_model, _token);
        }

        private void DoSwitchAutoZero(bool state)
        {
            var cmd = state ? "CALC:AZER ON" : "CALC:AZER OFF";
            _periodicContext.DoSend(cmd, false);
        }

        private void DoSwitchAutoLeak(bool state)
        {
            var cmd = state ? "SOUR:MODE:ALE ON" : "SOUR:MODE:ALE OFF";
            _periodicContext.DoSend(cmd, false);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
