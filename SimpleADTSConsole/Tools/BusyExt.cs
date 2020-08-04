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
            if (_dispatcher == null)
                _model.IsBusy = true;
            else
                _dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => _model.IsBusy = true));
        }


        public void Dispose()
        {
            if (_dispatcher == null)
                _model.IsBusy = false;
            else
                _dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => _model.IsBusy = false));
        }
    }


    public static class BusyExt
    {
        public static Task WithBusyAsync(this IBusy model, Action act)
        {
            var tsk = new Task(() =>
            {
                using (new BusyWrapper(model))
                    act();
            });
            tsk.Start(TaskScheduler.Default);
            return tsk;
        }

        public static Task WithBusyAsync(this IBusy model, Action act, Dispatcher dispatcher)
        {
            var tsk = new Task(() =>
            {
                using (new BusyWrapper(model, dispatcher))
                    act();
            });
            tsk.Start(TaskScheduler.Default);
            return tsk;
        }

        public static Task WithBusyAsync(this IBusy model, Action act, CancellationToken cancel)
        {
            var tsk = new Task(() =>
            {
                using (new BusyWrapper(model))
                    act();
            }, cancel);
            tsk.Start(TaskScheduler.Default);
            return tsk;
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
