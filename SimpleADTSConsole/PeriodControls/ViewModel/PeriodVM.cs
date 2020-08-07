using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using SimpleADTSConsole.PeriodControls.Model;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole.PeriodControls
{
    public class PeriodVm : INotifyPropertyChanged, IBusy
    {
        private ObservableCollection<CommandParametr> _loopCommandCollection;
        private ObservableCollection<CommandParametr> _prepareCommandCollection;
        private readonly Dispatcher _dispatcher;
        private bool _commandStart;
        private bool _isBusy = false;
        private readonly PeriodicCommands _periodicContext;

        public PeriodVm(PeriodicCommands periodicContext, Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _periodicContext = periodicContext;
            _loopCommandCollection = new ObservableCollection<CommandParametr>();
            _prepareCommandCollection = new ObservableCollection<CommandParametr>();
            _loopCommandCollection.Add(new CommandParametr(RemoveLoopCommand, false));
        }

        public ICommand StartPeriodic => new CommandWrapper(StartLoopCommands);

        private void StartLoopCommands()
        {
            if (CommandStart == false)
                return;

            var commands = GetCommands();
            _periodicContext.DoStartPeriodic(commands, this);
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

        public ObservableCollection<CommandParametr> LoopCommandCollection
        {
            get { return _loopCommandCollection; }
            set
            {
                _loopCommandCollection = value;
                OnPropertyChanged("LoopCommandCollection");
            }
        }

        public ObservableCollection<CommandParametr> PrepareCommandCollection
        {
            get { return _prepareCommandCollection; }
            set
            {
                _prepareCommandCollection = value;
                OnPropertyChanged("PrepareCommandCollection");
            }
        }

        public ICommand AddLoopCommand => new CommandWrapper(AddNewLoopCommand);
        public ICommand AddPrepareCommand => new CommandWrapper(AddNewPrepareCommand);

        public bool IsBusy
        {
            get { return !_isBusy; }
            set
            {
                _dispatcher.InvokeIfNeed(()=> {
                    _isBusy = value;
                    OnPropertyChanged("IsBusy");
                });
            }
        }

        public bool CommandStart
        {
            get { return !_commandStart; }
            set
            {
                _commandStart = value;
                OnPropertyChanged("CommandStart");
            }
        }


        private IEnumerable<Command> GetPrepareCommands()
        {
            return PrepareCommandCollection.Select(o=> o.GetCommand(false));
        }

        private IEnumerable<Command> GetLoopCommands()
        {
            return from p in LoopCommandCollection
                   select p.GetCommand(true);
        }

        public Queue<Command> GetCommands()
        {
            Queue<Command> commands = new Queue<Command>();
            foreach (var prepareCommand in GetPrepareCommands())
            {
                commands.Enqueue(prepareCommand);
            }

            foreach (var loopCommand in GetLoopCommands())
            {
                commands.Enqueue(loopCommand);
            }

            return commands;
        }

        private void AddNewLoopCommand()
        {
            bool isFirstElement = _loopCommandCollection.Count != 0;
            LoopCommandCollection.Add(new CommandParametr(RemoveLoopCommand, isFirstElement));
        }

        private void AddNewPrepareCommand()
        {
            PrepareCommandCollection.Add(new CommandParametr(RemovePrepareCommand, true));
        }

        private void RemoveLoopCommand(CommandParametr cmd)
        {
            if (cmd != null)
                LoopCommandCollection.Remove(cmd);
        }

        private void RemovePrepareCommand(CommandParametr cmd)
        {
            if (cmd != null)
                PrepareCommandCollection.Remove(cmd);
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
