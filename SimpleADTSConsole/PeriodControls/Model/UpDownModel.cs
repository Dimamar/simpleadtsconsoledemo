using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole.PeriodControls.Model
{
    public class UpDownModel
    {

        private readonly PeriodicCommands _periodicCommand;
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private UpDownContext _context;
        private int _isCh = 1;
        private readonly object _contextLock = new object();

        public UpDownModel(PeriodicCommands periodicCommand)
        {
            _periodicCommand = periodicCommand;
        }

        public void UpdateUpDownParameters(UpDownContext context)
        {
            lock (_contextLock)
                _context = context;
            Interlocked.Exchange(ref _isCh, 1);
        }

        public Task UpDownTest(UpDownContext context)
        {
            var token = _cancellation.Token;
            UpdateUpDownParameters(context);
            return TaskExec.ExecuteAsync(DoUpDown, token, token);
        }

        public Task GoToGroundAsync()
        {
            var token = _cancellation.Token;
            return TaskExec.ExecuteAsync(DoGoToGround, token);
        }

        void DoUpDown(CancellationToken cancel)
        {
            bool up = true;
            UpDownContext context;
            lock (_contextLock)
                context = _context;
            var cmd = "SOUR:STAT ON";
            _periodicCommand.DoSend(cmd, false);
            if (cancel.WaitHandle.WaitOne(context.Period))
                return;
            bool firstCircle = true;
            while (!cancel.IsCancellationRequested)
            {
                if (Interlocked.CompareExchange(ref _isCh, 0, 1) == 1)
                {
                    UpDownContext newContext;
                    lock (_contextLock)
                        newContext = _context;
                    if (newContext.Rate != context.Rate || firstCircle)
                    {
                        firstCircle = false;
                        cmd = string.Format(CultureInfo.GetCultureInfo("en-US"), "SOUR:RATE PS,{0}", newContext.Rate);
                        _periodicCommand.DoSend(cmd, false);
                        if (cancel.WaitHandle.WaitOne(context.Period))
                            break;
                    }
                    context = newContext;
                }

                cmd = string.Format(CultureInfo.GetCultureInfo("en-US"), "SOUR:PRES PS,{0}", up ? context.Up : context.Down);
                _periodicCommand.DoSend(cmd, false);

                if(cancel.WaitHandle.WaitOne(context.Period))
                    break;
                up = !up;
            }
        }

        void DoGoToGround()
        {
            var cmd = "SOUR:GTGR";
            _periodicCommand.DoSend(cmd, false);
        }

    }
}