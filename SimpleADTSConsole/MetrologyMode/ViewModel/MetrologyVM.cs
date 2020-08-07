using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;
using SimpleADTSConsole.Main.Model;
using SimpleADTSConsole.MetrologyMode.Model;
using SimpleADTSConsole.Scripts.Steps;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole.MetrologyMode
{
    public class MetrologyVM:INotifyPropertyChanged
    {
        private readonly MetrologyModel _model;
        private readonly IBusySynchronizer _synchronizer;
        private bool _isControlMode;
        private readonly CancellationTokenSource _cancellation;

        public MetrologyVM(MetrologyModel model, IBusySynchronizer synchronizer)
        {
            _model = model;
            _synchronizer = synchronizer;
            _cancellation = new CancellationTokenSource();
        }

        public bool IsAutoZeroChecked { get; set; }

        public ICommand SwitchAutoZero
        {
            get
            {
                return _synchronizer.GetCmdWrapBusyAsync(() =>
                    _model.DoSwitchAutoZero(IsAutoZeroChecked));
            }
        }

        public bool IsAutoLeakChecked { get; set; }

        public ICommand SwitchAutoLeak
        {
            get
            {
                return new CommandWrapper(() =>
                {
                    _synchronizer.GetCmdWrapBusyAsync(() =>
                        _model.DoSwitchAutoLeak(IsAutoZeroChecked));
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
                return _synchronizer.GetCmdWrapBusyAsync(() =>
                {
                    if (!IsControlMode)
                        _model.DoToMeasuring(_cancellation.Token);
                    else
                        _model.DoToControl(_cancellation.Token);
                });
            }
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
