using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SimpleADTSConsole.Tools
{
    public static class PropertyName
    {
        /// <summary>
        /// Получить имя поля 
        /// </summary>
        /// <typeparam name="T">Тип поля</typeparam>
        /// <param name="propertyLambda">Лямбда выражение</param>
        /// <returns>Имя поля</returns>
        public static string GetPropertyName<T>(this Expression<Action<T>> propertyLambda)
        {
            var memberExpression = propertyLambda.Body as MemberExpression;

            if (memberExpression == null)
            {
                throw new InvalidOperationException();
            }

            return memberExpression.Member.Name;
        }

        /// <summary>
        /// Получить имя поля 
        /// </summary>
        /// <typeparam name="T">Тип поля</typeparam>
        /// <param name="propertyLambda">Лямбда выражение</param>
        /// <returns>Имя поля</returns>
        public static string GetPropertyName<T>(this Expression<Func<T>> propertyLambda)
        {
            var memberExpression = propertyLambda.Body as MemberExpression;

            if (memberExpression == null)
            {
                throw new InvalidOperationException();
            }

            return memberExpression.Member.Name;
        }

        /// <summary>
        /// Получить имя поля 
        /// </summary>
        /// <typeparam name="T">Тип поля</typeparam>
        /// <typeparam name="Tobj">Тип объекта</typeparam>
        /// <param name="propertyLambda">Лямбда выражение</param>
        /// <returns>Имя поля</returns>
        public static string GetPropertyName<Tobj, T>(this Expression<Func<Tobj, T>> propertyLambda)
        {
            var memberExpression = propertyLambda.Body as MemberExpression;

            if (memberExpression == null)
            {
                throw new InvalidOperationException();
            }

            return memberExpression.Member.Name;
        }
    }

}
