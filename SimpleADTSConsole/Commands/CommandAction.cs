using System;

namespace SimpleADTSConsole
{
    public struct CommandAction
    {
        public DateTime Timestamp { get; set; }

        public string Command { get; set; }

        public string Answer { get; set; }
    }
}