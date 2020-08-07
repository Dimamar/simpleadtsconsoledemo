using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleADTSConsole.Tools
{
    /// <summary>
    /// базовая реализация IObservable<T>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseObservable<T> : IObservable<T>, IObservableUpdater<T>
    {
        private readonly List<IObserver<T>> _observers = new List<IObserver<T>>();

        #region Implementation of IObservable<out T>

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<T> observer)
        {
            _observers.Add(observer);
            return new Unsubscriber<T>(this, observer);
        }

        #endregion

        #region Implementation of IObservableUpdater<T>

        public void OnNext(T data)
        {
            _observers.ForEach(el => el.OnNext(data));
        }

        #endregion

        #region Unsubscribe

        private class Unsubscriber<Targ> : IDisposable
        {
            private readonly BaseObservable<Targ> _observable;
            private readonly IObserver<Targ> _observer;

            public Unsubscriber(BaseObservable<Targ> observable, IObserver<Targ> observer)
            {
                _observable = observable;
                _observer = observer;
            }

            public void Dispose()
            {
                _observable.Unsubscribe(_observer);
            }
        }

        private void Unsubscribe(IObserver<T> observer)
        {
            _observers.Remove(observer);
        }

        #endregion

    }
}
