using System;

namespace SimpleADTSConsole.PeriodControls
{
    public struct Command
    {
        /// <summary>
        /// ����� �������
        /// </summary>
        public string TextCommand { get; }

        /// <summary>
        /// ��������� �� �������� ����� ����� �������� �������
        /// </summary>
        public bool AnswerIsNeed { get; }

        /// <summary>
        /// ������� ���������� ������� ����� �������� �������
        /// </summary>
        public TimeSpan Period { get; }

        /// <summary>
        /// ���������� �� ��������� ������ ������� � �����
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