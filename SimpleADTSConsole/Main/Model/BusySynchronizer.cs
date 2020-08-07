using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole.Main.Model
{
    public class BusySynchronizer : IBusySynchronizer
    {
        private IStatus _statusHolder;

        public void UpdateStatusHolder(IStatus statusHolder)
        {
            _statusHolder = statusHolder;
        }

        public void WrapBusy(Action action)
        {
            _statusHolder.WithBusy(action);
        }

        public void WrapBusy<T>(Func<T> act, Action<T> postSyncAct)
        {
            _statusHolder.WithBusy(act, postSyncAct);
        }

        public void WrapBusyAsync<T>(Func<T> act, Action<T> postSyncAct, CancellationToken token)
        {
            TaskExec.ExecuteAsync(() => _statusHolder.WithBusy(act, postSyncAct), token);
        }

        public void WrapBusy<T>(Action<T> action, T arg)
        {
            _statusHolder.WithBusy<T>(action, arg);
        }

        public ICommand GetCmdWrapBusyAsync(Action action)
        {
            return _statusHolder.GetCmdBusyAsync(action);
        }
    }
}
