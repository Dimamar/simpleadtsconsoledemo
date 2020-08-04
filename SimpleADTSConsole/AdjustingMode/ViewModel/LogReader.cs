using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole.AdjustingMode
{
    public class LogReader : INotifyPropertyChanged
    {
        #region Fields

        private CancellationTokenSource _cancellationReadFromFile = new CancellationTokenSource();
        private TimeSpan _realPeriod;
        private readonly IADTSConsoleModel _model;

        /// <summary>
        /// ForDisigner
        /// </summary>
        public LogReader()
        {
            _model = null;
            _realPeriod = TimeSpan.Zero;
        }

        public LogReader(IADTSConsoleModel model, TimeSpan realPeriod)
        {
            _model = model;
            _realPeriod = realPeriod;
        }

        #endregion

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

        public string Path { get; set; }

        public bool IsStarted { get; set; }

        internal void UpdatePeriod(TimeSpan realPeriod)
        {
            _realPeriod = realPeriod;
        }

        #region Private

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
                var cmdStream = new CommandsFromFile(Properties.Settings.Default.Logversion);
                foreach (var cmd in cmdStream.Parce(Path))
                {
                    if (cancel.IsCancellationRequested)
                        break;
                    try
                    {
                        _model.Send(cmd.Command);
                    }
                    catch (Exception ex)
                    {
                        if (!Properties.Settings.Default.SuppressError)
                            throw;
                    }
                    if (cancel.WaitHandle.WaitOne(_realPeriod))
                        break;
                    try
                    {
                        _model.Read();
                    }
                    catch (Exception ex)
                    {
                        if (!Properties.Settings.Default.SuppressError)
                            throw;
                    }
                }
            }
            finally
            {
                IsStarted = false;
            }

        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
