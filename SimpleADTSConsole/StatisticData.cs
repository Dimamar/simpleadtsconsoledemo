using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole
{
    public class StatisticData : IObserver<CommandAction>
    {
        private readonly IEnumerable<Tuple<string, string>> _parameters = new List<Tuple<string, string>>()
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

        private readonly ILogWriter _logWriter;

        public StatisticData(ILogWriter statisticWriter)
        {
            States = new ObservableCollection<CurrentParameterState>();
            OtherCommands = new List<CurrentParameterState>();
            _logWriter = statisticWriter;
            _logWriter.Start();
        }

        public void OnNext(CommandAction commandAction)
        {
            UpdateStatistic(commandAction);
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            //States.Clear();
            //LastParametr = null;
        }

        public ObservableCollection<CurrentParameterState> States { get; }

        private List<CurrentParameterState> OtherCommands { get; }

        /// <summary>
        /// Текущее состояние
        /// </summary>
        public CurrentParameterState LastParametr { get; set; }

        private void UpdateStatistic(CommandAction value)
        {
            var cmd = ParceCmd(value);

            if (string.IsNullOrEmpty(cmd))
            {
                LastParametr = null;
                OtherParametrLog(value);
                return;
            }

            var name = _parameters.FirstOrDefault(el => el.Item2 == cmd);
            if (name == null)
            {
                LastParametr = null;
                return;
            }

            var state = States.FirstOrDefault(el => el.Name == name.Item1);
            if (state == null)
            {
                state = CreateNewState(name.Item1, value.Command, value.Answer);
                LastParametr = state;
                States.Add(state);
                state = SetSendData(state);
                return;
            }

            if (!value.IsAnswer)
            {
                state = SetSendData(state);
                return;
            }

            state = SetRecieveDataToParam(state, value);
            _logWriter?.PostAnswer(state);
            LastParametr = state;
        }

        private void OtherParametrLog(CommandAction value)
        {
            // Если это не измерительные параметры
            // Необходимо добавить посмотреть какая это команда, которая требует ответа или нет
            // Если нет, то просто кидаем в лог
            if (value.IsAnswer)
            {
                // Если нам пришёл ответ
                // смотрим есть ли что-то такое в нашёй коллекции
                var com = OtherCommands.FirstOrDefault(el => el.CommandText == value.Command);
                if (com == null)
                {
                    // нам пришёл ответ, но запрос мы не видели, напишем в лог об этом маленьком недоразумении
                    com = CreateNewState(string.Empty, value.Command, value.Answer);
                    com.FirstAnswer = value.Timestamp;
                    com.LastCommand = value.Timestamp;
                    _logWriter.PostAnswer(com);
                    return;
                }

                com = SetRecieveDataToParam(com, value);
                _logWriter.PostAnswer(com);
                OtherCommands.Remove(com);
            }
            else
            {
                // Если нам пришёл запрос
                if (value.AnswerIsNeed)
                {
                    // Если нужен ответ, то добовляем в коллекцию необходимый элемент
                    OtherCommands.Add(CreateNewState(string.Empty, value.Command, value.Answer));
                }
                else
                {
                    _logWriter.PostCommand(new CurrentParameterState
                    {
                        CommandText = value.Command,
                        FirstCommand = value.Timestamp,
                        LastCommand = value.Timestamp,
                        Value = ""
                    });
                }
            }
        }

        private CurrentParameterState CreateNewState(string name, string commandText, string value)
        {
            return new CurrentParameterState()
            {
                Name = name,
                CommandText = commandText,
                FirstCommand = DateTime.Now,
                LastCommand = DateTime.Now,
                Value = value,
                CurrentRepeats = 0,
                MaxCountRepeat = 0,
                CountAllRepeats = 0,
            };
        }

        private CurrentParameterState SetRecieveDataToParam(CurrentParameterState state, CommandAction value)
        {
            if (state.Value == value.Answer)
            {
                state.CurrentRepeats = state.CurrentRepeats + 1;
                state.LastAnswer = DateTime.Now;
            }
            else
            {
                state.FirstCommand = state.LastCommand;
                state.FirstAnswer = DateTime.Now;
                state.LastAnswer = DateTime.Now;
                state.CurrentRepeats = 0;
            }

            state.Value = value.Answer;

            state.CountAllRepeats = state.CountAllRepeats + 1;
            if (state.CurrentRepeats > state.MaxCountRepeat)
                state.MaxCountRepeat = state.CurrentRepeats;

            return state;
        }

        private CurrentParameterState SetSendData(CurrentParameterState state)
        {
            state.LastCommand = DateTime.Now;
            return state;
        }

        private string ParceCmd(CommandAction value)
        {
            var prefix = "MEAS:PRES? ";
            if (value.Command.StartsWith(prefix))
                return value.Command.Remove(0, prefix.Length);

            return null;
        }
    }
}