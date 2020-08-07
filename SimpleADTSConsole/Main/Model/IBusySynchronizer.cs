using System;
using System.Windows.Input;

namespace SimpleADTSConsole.Main.Model
{
    public interface IBusySynchronizer
    {
        void WrapBusy(Action action);
        void WrapBusy<T>(Action<T> action, T arg);
        ICommand GetCmdWrapBusyAsync(Action action);
    }
}