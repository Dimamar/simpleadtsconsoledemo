using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Input;
using SimpleADTSConsole.PeriodControls.Model;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole.PeriodControls.ViewModel
{
    public class CommandCompilerVM : INotifyPropertyChanged
    {
        private readonly CommandCompilerModel _model;
        private string _command;

        public CommandCompilerVM(CommandCompilerModel model)
        {
            _model = model;
            Parameters = _model.Parameters;
        }

        #region Simple /* Maybe sAmple? */ 

        public string Command
        {
            get { return _command; }
            set
            {
                _command = value;
                OnPropertyChanged("Command");
            }
        }

        public ICommand Send { get { return new CommandWrapper(()=>_model.SendAsync(Command)); } }

        public ICommand SendReceive { get { return new CommandWrapper(() => _model.SendReceiveAsync(Command)); } }

        #endregion

        #region Command compill

        public string SelectedParameter { get; set; }

        public IEnumerable<string> Parameters { get; private set; }

        public string Value { get; set; }

        public ICommand CompileSetValue
        {
            get { return new CommandWrapper(() => { Command = _model.CompileSetCommand(SelectedParameter, Value); }); }
        }

        public ICommand CompileGetValue
        {
            get { return new CommandWrapper(() => { Command = _model.CompileGetCommand(SelectedParameter); });}
        }

        public ICommand CompileToControl
        {
            get { return new CommandWrapper(() => { Command = _model.ToControl(); }); }
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
