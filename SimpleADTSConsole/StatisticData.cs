using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SimpleADTSConsole
{
    public class StatisticData : IObserver<CommandAction>
    {
        private IEnumerable<Tuple<string, string>> _parameters = new List<Tuple<string, string>>()
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

        public StatisticData()
        {
            States = new ObservableCollection<CurrentParameterState>();
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
                state = new CurrentParameterState()
                {
                    Name = name.Item1,
                    LastTime = DateTime.Now,
                    Value = value.Answer,
                    CurrentRepeats = 0,
                    MaxCountRepeat = 0,
                    CountAllRepeats = 0,
                };
                LastParametr = state;
                States.Add(state);
                return;
            }

            if (state.Value == value.Answer)
                state.CurrentRepeats = state.CurrentRepeats + 1;
            else
                state.CurrentRepeats = 0;
            state.Value = value.Answer;
            state.LastTime = DateTime.Now;
            state.CountAllRepeats = state.CountAllRepeats + 1;
            if (state.CurrentRepeats > state.MaxCountRepeat)
                state.MaxCountRepeat = state.CurrentRepeats;
            LastParametr = state;
        }

        private string ParceCmd(CommandAction value)
        {
            var prefix = "MEAS:PRES? ";
            if (!value.Command.StartsWith(prefix))
                return null;
            return value.Command.Remove(0, prefix.Length);
        }
    }
}