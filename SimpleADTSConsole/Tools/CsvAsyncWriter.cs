using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace SimpleADTSConsole.Tools
{
    /// <summary>
    /// Асинхронная запист в файл
    /// При очень частом опросе ест память
    /// </summary>
    public class CsvAsyncWriter : IDisposable, ILogWriter
    {
        Thread _threadWriter;
        private StreamWriter _writer;
        private const string Extension = "csv";
        private const string DatePattern = "HH:mm:ss:ffff";
        private readonly string _folderPath;
        private int _countCommand;

        private ConcurrentQueue<Tuple<CurrentParameterState, bool>> _states;
        private CancellationTokenSource _source;
        private CancellationToken _token;

        public CsvAsyncWriter(string folderPath)
        {
            _folderPath = folderPath;
            _countCommand = 0;
        }

        public void Start()
        {
            _states = new ConcurrentQueue<Tuple<CurrentParameterState, bool>>();
            _source = new CancellationTokenSource();
            _token = _source.Token;

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

            _threadWriter = new Thread(MainCicle);
            _threadWriter.Start();
        }

        private void MainCicle()
        {
            while (!_token.IsCancellationRequested)
            {
                Tuple<CurrentParameterState, bool> cmd;
                if (!_states.TryDequeue(out cmd))
                {
                    _token.WaitHandle.WaitOne(10);
                }
                else
                {
                    Write(cmd.Item1, cmd.Item2);
                }
            }
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

        private string GetFileName()
        {
            return $"{DateTime.Now:dd.MM.yyyy}.log.{Extension}";
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

        public void PostCommand(CurrentParameterState state)
        {
            _states.Enqueue(new Tuple<CurrentParameterState, bool>(state, false));
        }

        public void PostAnswer(CurrentParameterState state)
        {
            _states.Enqueue(new Tuple<CurrentParameterState, bool>(state, true));
        }

        public void Dispose()
        {
            _source.Cancel();
            _token.WaitHandle.WaitOne();
            _source = new CancellationTokenSource();
            _writer?.Dispose();
            _threadWriter = null;
        }
    }
}