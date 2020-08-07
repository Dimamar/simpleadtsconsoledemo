using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleADTSConsole.Tools
{
    /// <summary>
    /// Элемент, освобождающий ресурс
    /// </summary>
    public class DisposeItem : IDisposable
    {
        /// <summary>
        /// Делегат, освобождающий ресурс
        /// </summary>
        private readonly Action _disposeAction;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="disposeAction">Делегат, освобождающий ресурс</param>
        public DisposeItem(Action disposeAction)
        {
            if (disposeAction == null)
                throw new ArgumentNullException("disposeAction");

            _disposeAction = disposeAction;
        }


        /// <summary>
        /// Освободить ресурс
        /// </summary>
        public void Dispose()
        {
            _disposeAction();
        }
    }

}
