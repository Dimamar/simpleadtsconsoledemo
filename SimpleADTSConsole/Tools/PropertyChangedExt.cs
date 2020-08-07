using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SimpleADTSConsole.Tools
{
    public static class PropertyChangedExt
    {

        /// <summary>
        /// Подписаться типизировано на событие
        /// </summary>
        /// <typeparam name="TObj"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="propertyLambda"></param>
        /// <param name="callback"></param>
        public static IDisposable Subscribe<TObj, T>(this TObj target, Expression<Func<TObj, T>> propertyLambda, Action<T> callback)
            where TObj : class, INotifyPropertyChanged
        {
            if (target == null)
                throw new NullReferenceException();
            if (propertyLambda == null)
                throw new NullReferenceException();
            if (callback == null)
                throw new NullReferenceException();

            var propName = propertyLambda.GetPropertyName();
            var getVal = propertyLambda.Compile();
            var func = new PropertyChangedEventHandler(new Action<object, PropertyChangedEventArgs>((sender, args) =>
            {
                if (args.PropertyName != propName)
                    return;
                var typedTarget = sender as TObj;
                if (typedTarget == null)
                    return;
                var val = getVal(target);
                callback(val);
            }));
            target.PropertyChanged += func;
            return new DisposeItem(() =>
            {
                target.PropertyChanged -= func;
            });
        }
    }

}
