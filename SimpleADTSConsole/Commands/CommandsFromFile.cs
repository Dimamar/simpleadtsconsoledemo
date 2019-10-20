using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace SimpleADTSConsole
{
    public class CommandsFromFile
    {
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
                        cmd.Command = line.Remove(0, 16);
                    else if (IsAnswer(line))
                    {
                        cmd.Answer = line.Remove(0, 16);
                        cmd.Timestamp = DateTime.Parse(line.Substring(0, 13));
                        yield return cmd;
                        cmd = new CommandAction();
                    }
                    line = stream.ReadLine();
                }
            }
        }

        private bool IsAnswer(string line)
        {
            return line.Contains(">>");
        }

        private bool IsCommand(string line)
        {
            return line.Contains("<<");
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
