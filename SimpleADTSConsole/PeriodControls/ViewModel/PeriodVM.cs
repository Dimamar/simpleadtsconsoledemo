using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole.PeriodControls
{
    public class PeriodVm : INotifyPropertyChanged, IBusy
    {
        private ObservableCollection<CommandParametr> _loopCommandCollection;
        private ObservableCollection<CommandParametr> _prepeareCommandCollection;
        private readonly Dispatcher _dispatcher;

        public PeriodVm(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _loopCommandCollection = new ObservableCollection<CommandParametr>();
            _prepeareCommandCollection = new ObservableCollection<CommandParametr>();
            _loopCommandCollection.Add(new CommandParametr(RemoveLoopCommand, false));
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
        public ObservableCollection<CommandParametr> PrepeareCommandCollection
        {
            get { return _prepeareCommandCollection; }
            set
            {
                _prepeareCommandCollection = value;
                OnPropertyChanged("PrepeareCommandCollection");
            }
        }

        public ICommand AddLoopCommand => new CommandWrapper(AddNewLoopCommand);
        public ICommand AddPrepeareCommand => new CommandWrapper(AddNewPrepeareCommand);

        private bool _isBusy = false;
        public bool IsBusy
        {
            get { return !_isBusy; }
            set
            {
                if (_dispatcher.CheckAccess())
                {
                    _isBusy = value;
                    OnPropertyChanged("IsBusy");
                }
                else
                {
                    _dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => IsBusy = value));
                }
            }
        }

        private IEnumerable<Command> GetPrepeareCommands()
        {
            return from p in PrepeareCommandCollection
                   select p.GetCommand(false);
        }

        private IEnumerable<Command> GetLoopCommands()
        {
            return from p in LoopCommandCollection
                   select p.GetCommand(true);
        }

        public Queue<Command> GetCommands()
        {
            Queue<Command> commands = new Queue<Command>();
            foreach (var prepeareCommand in GetPrepeareCommands())
            {
                commands.Enqueue(prepeareCommand);
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

        private void AddNewPrepeareCommand()
        {
            PrepeareCommandCollection.Add(new CommandParametr(RemovePrepeareCommand, true));
        }

        private void RemoveLoopCommand(CommandParametr cmd)
        {
            if (cmd != null)
                LoopCommandCollection.Remove(cmd);
        }

        private void RemovePrepeareCommand(CommandParametr cmd)
        {
            if (cmd != null)
                PrepeareCommandCollection.Remove(cmd);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
