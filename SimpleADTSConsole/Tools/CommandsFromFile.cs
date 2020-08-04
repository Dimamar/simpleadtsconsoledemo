using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Path = System.IO.Path;

namespace SimpleADTSConsole.Tools
{
    public class CommandsFromFile
    {

        private readonly LogVersion _version;

        private readonly string keyCommand;
        private readonly string keyAnswer;

        private readonly int _commandPrefix;
        private readonly int _answerPrefix;

        public CommandsFromFile(LogVersion version = LogVersion.v1)
        {
            _version = version;
            switch (version)
            {
                case LogVersion.v1:
                    keyCommand = "<<";
                    keyAnswer = ">>";
                    _commandPrefix = 16;
                    _answerPrefix = 16;
                    break;
                case LogVersion.v2:
                    keyCommand = "Command:";
                    keyAnswer = "Answer:";
                    _commandPrefix = 28 + keyCommand.Length;
                    _answerPrefix = 28 + keyAnswer.Length;
                    break;
            }
        }

        public IEnumerable<CommandAction> Parce(string file)
        {
            List<string> files = new List<string>();
            if (file.Contains("*"))
            {
                var dirPath = Path.GetDirectoryName(file);
                var fileNameMask = file.Remove(0, dirPath.Length);
                Regex searchPattern = new Regex(fileNameMask);
                files.AddRange(Directory.GetFiles(dirPath).Where(f => searchPattern.IsMatch(f))
                    .Select(f => Path.Combine(dirPath, f)).ToArray());
            }
            else
            {
                files.Add(file);
            }
            using (var stream = new MultyFilesStream(files))
            {
                var line = stream.ReadLine();
                var cmd = new CommandAction();
                while (line != null)
                {
                    line = line.Replace("\0", "");
                    if (IsCommand(line))
                        cmd.Command = GetCommand(line, _version);
                    else if (IsAnswer(line))
                    {
                        cmd.Answer = GetAnswer(line, _version);
                        cmd.Timestamp = GetDateTime(line, _version);
                        yield return cmd;
                        cmd = new CommandAction();
                    }
                    line = stream.ReadLine();
                }
            }
        }

        private static DateTime GetDateTime(string line, LogVersion version)
        {
            return DateTime.Parse(line.Substring(0, 13));
        }

        private string GetAnswer(string line, LogVersion version)
        {
            return line.Remove(0, _answerPrefix);
        }

        private string GetCommand(string line, LogVersion version)
        {
            return line.Remove(0, _commandPrefix);
        }

        private bool IsAnswer(string line)
        {
            return line.Contains(keyAnswer);
        }

        private bool IsCommand(string line)
        {
            return line.Contains(keyCommand);
        }


        public class MultyFilesStream:IDisposable
        {
            private IEnumerator<string> _files = null;
            private StreamReader _curFile = null;
            private bool _isEndFile = false;
            private bool _isEnd = false;

            public MultyFilesStream(IEnumerable<string> files)
            {
                _files = files.GetEnumerator();
            }

            public string ReadLine()
            {
                if (_curFile == null || _curFile.EndOfStream)
                {
                    if(_curFile != null)
                        _curFile.Dispose();

                    if (!_files.MoveNext())
                    {
                        _isEndFile = true;
                        return null;
                    }
                    _curFile = new StreamReader(_files.Current);
                }
                return _curFile.ReadLine();
            }


            public void Dispose()
            {
                if (_curFile != null)
                    _curFile.Dispose();
            }
        }
    }
}
