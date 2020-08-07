using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace SimpleADTSConsole.Tools
{
    public static class DispatcherEx
    {
        /// <summary>
        /// Invoke только если это необходимо
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="act"></param>
        /// <param name="priority"></param>
        public static void InvokeIfNeed(this Dispatcher dispatcher, Action act, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                act();
            }
            else
            {
                dispatcher.Invoke(priority, act);
            }
        }
    }
}
