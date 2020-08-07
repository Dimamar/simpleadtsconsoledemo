using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using SimpleADTSConsole.AdjustingMode;

namespace SimpleADTSConsole.Tools
{
    public class BusyWrapper : IDisposable
    {
        private readonly IBusy _model;
        private readonly Dispatcher _dispatcher;

        public BusyWrapper(IBusy model, Dispatcher dispatcher = null)
        {
            _model = model;
            _dispatcher = dispatcher;
            _dispatcher.InvokeIfNeed(() => _model.IsBusy = true);
        }

        public void PostSync(Action postSyncAct)
        {
            _dispatcher.InvokeIfNeed(postSyncAct);
        }

        public void Dispose()
        {
            _dispatcher.InvokeIfNeed(() => _model.IsBusy = false);
        }
    }


    public static class BusyExt
    {
        public static void WithBusy(this IBusy model, Action act)
        {
            using (new BusyWrapper(model))
                act();
        }

        public static void WithBusy<T>(this IBusy model, Func<T> act, Action<T> postSyncAct )
        {
            using (var busy = new BusyWrapper(model))
            {
                var res = act();
                busy.PostSync(()=>postSyncAct(res));
            }
        }

        public static void WithBusy<T>(this IBusy model, Action<T> act, T arg)
        {
            using (new BusyWrapper(model))
                act(arg);
        }

        public static Task WithBusyAsync(this IBusy model, Action act)
        {
            return model.WithBusyAsync(act, CancellationToken.None);
        }

        public static Task WithBusyAsync(this IBusy model, Action act, CancellationToken cancel)
        {
            return TaskExec.ExecuteAsync(() => model.WithBusy(act), cancel);
        }

        public static Task WithBusyAsync(this IBusy model, Action<CancellationToken> act, CancellationToken cancel)
        {
            return TaskExec.ExecuteAsync((token) => model.WithBusy(act, token),
                cancel, cancel);
        }

        public static ICommand GetCmdBusyAsync(this IStatus model, Action act)
        {
            return new CommandWrapper(()=>model.WithBusyAsync(act));
        }

        public static ICommand GetCmdTryBusyAsync(this IStatus model, Action act)
        {
            return new CommandWrapper(() => model.WithBusyAsync(() => model.TryOrDisconnect(act)));
        }

        public static void TryOrDisconnect(this IStatus model, Action act)
        {
            try
            {
                act();
            }
            catch
            {
                model.IsOpened = false;
                throw;
            }
        }
    }
}
