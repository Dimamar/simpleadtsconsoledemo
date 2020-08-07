using System;
using System.ComponentModel;
using System.Windows.Input;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole.PeriodControls
{
    public class CommandParametr : INotifyPropertyChanged
    {
        private string _command;
        public string Command
        {
            get { return _command; }
            set
            {
                _command = value;
                OnPropertyChanged("Command");
            }
        }

        private bool _needAnswer;
        public bool NeedAnswer
        {
            get { return _needAnswer; }
            set
            {
                _needAnswer = value;
                OnPropertyChanged("NeedAnswer");
            }
        }

        private int _timeToWait;
        public int TimeToWait
        {
            get { return _timeToWait; }
            set
            {
                _timeToWait = value;
                OnPropertyChanged("TimeToWait");
            }
        }

        public ICommand Remove
        {
            get
            {
                return new CommandWrapper(() =>
                {
                    _removeACtion?.Invoke(this);
                });
            }
        }

        private bool _isButtonVisible;
        public bool IsButtonvisible
        {
            get { return _isButtonVisible; }
            set
            {
                _isButtonVisible = value;
                OnPropertyChanged("IsButtonvisible");
            }
        }

        public Command GetCommand(bool needRepeat)
        {
            return new Command(Command, NeedAnswer, TimeToWait, needRepeat);
        }

        private readonly Action<CommandParametr> _removeACtion;

        /// <summary>
        /// Создаёт экземпляр команды
        /// </summary>
        /// <param name="removeCommand">метод удаления команды из коллекции</param>
        /// <param name="isButtonvisible">отображать ли кнопку удаления</param>
        public CommandParametr(Action<CommandParametr> removeCommand, bool isButtonvisible)
        {
            if (removeCommand == null)
                throw new ArgumentException("Remove Command");

            IsButtonvisible = isButtonvisible;
            _removeACtion = removeCommand;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}