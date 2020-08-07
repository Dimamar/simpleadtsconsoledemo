using System;
using System.Diagnostics;
using System.Threading;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole.AdjustingMode.Model
{
    internal class LogReader : ILogReader, IDisposable
    {
        private readonly IADTSTransportModel _model;
        private ReadLogConfig _config;
        private string _path = null;
        private readonly ManualResetEvent _wh = new ManualResetEvent(true);
        CancellationTokenSource _cancellation = new CancellationTokenSource();
        
        public LogReader(ReadLogConfig config, IADTSTransportModel model)
        {
            _config = config;
            _model = model;
        }

        #region ILogReader

        public void Start()
        {
            var token = _cancellation.Token;
            TaskExec.ExecuteAsync(DoStartFromFile, token, token);
        }

        public void Stop()
        {
            _cancellation.Cancel();
            _cancellation = new CancellationTokenSource();
            _wh.WaitOne();
        }

        public void UpdatePeriod(TimeSpan realPeriod)
        {
            _config.Period = realPeriod;
        }

        public void UpdatePath(string path)
        {
            _path = path;
        }

        #endregion

        private void DoStartFromFile(CancellationToken cancel)
        {
            _wh.Reset();
            try
            {
                var cmdStream = new CommandsFromFile(Properties.Settings.Default.Logversion);
                foreach (var cmd in cmdStream.Parce(_path))
                {
                    if (cancel.IsCancellationRequested)
                        break;
                    try
                    {
                        _model.Send(cmd.Command);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                        if (!Properties.Settings.Default.SuppressError)
                            throw;
                    }
                    if (cancel.WaitHandle.WaitOne(_config.Period))
                        break;
                    try
                    {
                        _model.Read();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                        if (!Properties.Settings.Default.SuppressError)
                            throw;
                    }
                }
            }
            finally
            {
                _wh.Reset();
            }
        }

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _cancellation.Cancel();
            _wh.Set();
        }

        #endregion
    }
}
