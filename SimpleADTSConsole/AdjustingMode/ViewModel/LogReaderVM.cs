using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using SimpleADTSConsole.AdjustingMode.Model;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole.AdjustingMode
{
    public class LogReaderVM : INotifyPropertyChanged
    {
        #region Fields

        private readonly ILogReader _reader;

        /// <summary>
        /// ForDisigner
        /// </summary>
        public LogReaderVM():this(null){}

        public LogReaderVM(ILogReader reader)
        {
            _reader = reader;
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
                        IsStarted = true;
                        _reader.Start();
                    }
                    else
                    {
                        _reader.Stop();
                        IsStarted = false;
                    }
                });
            }
        }

        public string Path { get; set; }

        public bool IsStarted { get; set; }

        #region Private

        private void DoOpenFile()
        {
            var ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            var res = ofd.ShowDialog();
            if (res == null || !res.Value)
                return;
            _reader.UpdatePath(ofd.FileName);
            Path = ofd.FileName;
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
