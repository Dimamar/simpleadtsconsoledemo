using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace SimpleADTSConsole
{
    public static class BusyExt
    {
        //public static void WithBusy(this ConsoleViewModel model, Action act)
        //{
        //    using (new BusyWrapper(model))
        //        act();
        //}

        //public static void WithBusy(this ConsoleViewModel model, Action act, Dispatcher dispatcher)
        //{
        //    using (new BusyWrapper(model, dispatcher))
        //        //if (dispatcher == null)
        //        act();
        //    //else
        //    //    dispatcher.Invoke(DispatcherPriority.Normal, act);
        //}

        public static Task WithBusyAsync(this ConsoleViewModel model, Action act)
        {
            var tsk = new Task(() =>
            {
                using (new BusyWrapper(model))
                    act();
            });
            tsk.Start(TaskScheduler.Default);
            return tsk;
        }

        public static Task WithBusyAsync(this ConsoleViewModel model, Action act, CancellationToken cancel)
        {
            var tsk = new Task(() =>
            {
                using (new BusyWrapper(model))
                    act();
            }, cancel);
            tsk.Start(TaskScheduler.Default);
            return tsk;
        }

        //public static ICommand GetCmdBusyAsync(this ConsoleViewModel model, Action act, CancellationToken cancel)
        //{
        //    return new CommandWrapper(() => model.WithBusyAsync(act, cancel));
        //}

        public static ICommand GetCmdBusyAsync(this ConsoleViewModel model, Action act)
        {
            return new CommandWrapper(()=>model.WithBusyAsync(act));
        }

        public static ICommand GetCmdTryBusyAsync(this ConsoleViewModel model, Action act)
        {
            return new CommandWrapper(() => model.WithBusyAsync(() => model.TryOrDisconnect(act)));
        }

        //public static ICommand GetCmdTryBusy(this ConsoleViewModel model, Action act)
        //{
        //    return new CommandWrapper(() => model.WithBusy(()=>model.TryOrDisconnect(act)));
        //}


        //public static IDisposable WrapBusy(this ConsoleViewModel model, Dispatcher dispatcher)
        //{
        //    return new BusyWrapper(model, dispatcher);
        //}


        public static void TryOrDisconnect(this ConsoleViewModel model, Action act)
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

        private class BusyWrapper:IDisposable
        {
            private readonly ConsoleViewModel _model;
            private readonly Dispatcher _dispatcher;

            public BusyWrapper(ConsoleViewModel model, Dispatcher dispatcher = null)
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
    }
}
