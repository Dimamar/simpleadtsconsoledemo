using System;

namespace SimpleADTSConsole.PeriodControls
{
    public struct Command
    {
        /// <summary>
        /// Текст команды
        /// </summary>
        public string TextCommand { get; }

        /// <summary>
        /// Требуется ли получить ответ после отправки команды
        /// </summary>
        public bool AnswerIsNeed { get; }

        /// <summary>
        /// Сколько необходимо ожидать после отправки команды
        /// </summary>
        public TimeSpan Period { get; }

        /// <summary>
        /// Необходимо ли повторять данную команду в цикле
        /// </summary>
        public bool Repeat { get; }

        public Command(string textCommand, bool answerIsNeed, int period, bool repeat)
        {
            Repeat = repeat;
            Period = TimeSpan.FromMilliseconds(period);
            TextCommand = textCommand;
            AnswerIsNeed = answerIsNeed;
        }

        public Command(string textCommand, bool answerIsNeed, TimeSpan period, bool repeat)
        {
            Repeat = repeat;
            Period = period;
            TextCommand = textCommand;
            AnswerIsNeed = answerIsNeed;
        }

        public Command(string textCommand, bool answerIsNeed) : this(textCommand, answerIsNeed, 100, false)
        { }

        public Command(Command cmd)
        {
            this = cmd;
        }
    }
}