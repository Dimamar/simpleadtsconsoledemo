using System;

namespace SimpleADTSConsole.Tools
{
    public struct CommandAction
    {
        public DateTime Timestamp { get; set; }

        public string Command { get; set; }

        public string Answer { get; set; }

        public bool IsAnswer { get; set; }

        public bool AnswerIsNeed { get; set; }
    }
}