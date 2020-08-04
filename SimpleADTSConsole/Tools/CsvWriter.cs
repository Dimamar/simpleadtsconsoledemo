using System;
using System.IO;

namespace SimpleADTSConsole.Tools
{
    public class CsvWriter : IDisposable, ILogWriter
    {
        private StreamWriter _writer;
        private const string Extension = "csv";
        private const string DatePattern = "HH:mm:ss:ffff";
        private readonly string _folderPath;
        private int _countCommand;

        public CsvWriter(string folderPath)
        {
            _folderPath = folderPath;
            _countCommand = 0;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public void Start()
        {
            bool createFile = false;
            Stream stream = null;

            if (!Directory.Exists(_folderPath))
                Directory.CreateDirectory(_folderPath);

            string fileName = Path.Combine(_folderPath, GetFileName());
            if (File.Exists(fileName))
            {
                stream = File.Open(fileName, FileMode.Append);
            }
            else
            {
                stream = File.Create(fileName);
                createFile = true;
            }
            _writer = new StreamWriter(stream);
            if (createFile)
                CreateHeader();
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public void PostCommand(CurrentParameterState state)
        {
            Write(state, false);
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public void PostAnswer(CurrentParameterState state)
        {
            Write(state, true);
        }

        private string GetFileName()
        {
            return $"{DateTime.Now:dd.MM.yyyy}.log.{Extension}";
        }

        private void CreateHeader()
        {
            string header =
                "Command;" +
                "Answer;" +
                "first time command;" +
                "last time command;" +
                "first time answer;" +
                "last time answer;" +
                "number repeats;" +
                "max number repeat";

            Write(header);
        }

        private void Write(CurrentParameterState command, bool withAnswer)
        {
            string firsAnswer = "";
            string lastAnswer = "";
            string answer = "";

            var firstCom = command.FirstCommand.ToString(DatePattern);
            var lastCom = command.LastCommand.ToString(DatePattern);

            if (withAnswer)
            {
                answer = command.Value;
                firsAnswer = command.FirstAnswer.ToString(DatePattern);
                lastAnswer = command.LastAnswer.ToString(DatePattern);
            }
            answer = answer.Replace('.', ',');
            var toWrite = $"{command.CommandText};{answer};{firstCom};{lastCom};{firsAnswer};{lastAnswer};{command.CurrentRepeats};{command.MaxCountRepeat}";
            _writer.WriteLine(toWrite);
            _countCommand++;
            if (_countCommand > 100)
            {
                _writer.Flush();
                _countCommand = 0;
            }
        }

        private void Write(string write)
        {
            _writer.WriteLine(write);
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }
    }
}
