using System;

namespace SimpleADTSConsole.AdjustingMode.Model
{
    public interface ILogReader: IDisposable
    {
        void UpdatePeriod(TimeSpan realPeriod);
        void UpdatePath(string path);
        void Start();
        void Stop();
    }
}