using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace SimpleADTSConsole
{
    internal class StatisticObserver : IObserver<CommandAction>
    {
        private readonly string _basePath;
        private string _dir;
        private StatisticData _statistic = new StatisticData();

        private List<string> _eventBuffer = new List<string>();
        private int _maxLengthBuffer = 15;

        public StatisticObserver(string basePath)
        {
            _basePath = basePath;
            _statistic = new StatisticData();
        }

        public void OnNext(CommandAction value)
        {
            _statistic.OnNext(value);
            var line = string.Format("<time=\"{0:yy.MM.dd_hh:mm:ss.fff}\"/><cmd=\"{1}\"/><cmd=\"{2}\"/>",
                value.Timestamp, value.Command, value.Answer);
            if (_statistic.LastParametr != null && _statistic.LastParametr.CurrentRepeats >0)
                line = string.Format("{0}<rep=\"{1}\"/>", line, _statistic.LastParametr.CurrentRepeats);
            _eventBuffer.Add(line);
            if(_eventBuffer.Count< _maxLengthBuffer)
                return;

            var path = Path.Combine(_basePath, _dir, "log.xml");
            AppendFile(path, _eventBuffer.ToArray());
            _eventBuffer.Clear();
        }

        public void OnError(Exception error)
        {
            _statistic.OnError(error);
        }

        public void OnCompleted()
        {
            AppendStatistic(Path.Combine(_basePath, _dir, "statistic.xml"), _dir, _statistic);
            _statistic.OnCompleted();
            _statistic = new StatisticData();
        }

        public void SetDir(string dir)
        {
            if(_dir == dir)
                return;
            _dir = dir;
            OnCompleted();
        }

        private void AppendFile(string path, IEnumerable<string> lines)
        {
            CreateDirIfNotExist(path);
            using (var file = new StreamWriter(path, true))
            {
                foreach (var line in lines)
                {
                    file.WriteLine(line);
                }
            }
        }

        private void AppendStatistic(string path, string note, StatisticData data)
        {
            CreateDirIfNotExist(path);
            using (var file = new StreamWriter(path, true))
            {
                file.WriteLine(note);
                var xml = new XmlSerializer(data.GetType());
                xml.Serialize(file, data);
            }
        }

        private void CreateDirIfNotExist(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if(string.IsNullOrEmpty(dir))
                return;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }
}