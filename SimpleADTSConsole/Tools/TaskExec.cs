using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleADTSConsole.Tools
{
    public static class TaskExec
    {
        /// <summary>
        /// Запустить задачу от диспетчера по умолчанию
        /// </summary>
        /// <param name="action"></param>
        /// <param name="token"></param>
        public static Task ExecuteAsync(Action action, CancellationToken token)
        {
            var task = new Task(action, token);
            task.Start(TaskScheduler.Default);
            return task;
        }

        /// <summary>
        /// Запустить задачу с типизированным аргументом от диспетчера по умолчанию
        /// </summary>
        /// <typeparam name="TParam"></typeparam>
        /// <param name="action"></param>
        /// <param name="argument"></param>
        /// <param name="token"></param>
        public static Task ExecuteAsync<TParam>(Action<TParam> action, TParam argument, CancellationToken token)
        {
            var task = new Task((arg) =>
            {
                var typedArg = (TParam)arg;
                action(typedArg);
            }, (object)argument, token);
            task.Start(TaskScheduler.Default);
            return task;
        }

        /// <summary>
        /// Запустить задачу с типизированным аргументом от диспетчера по умолчанию и опцией функционирования
        /// </summary>
        /// <typeparam name="TParam"></typeparam>
        /// <param name="action"></param>
        /// <param name="argument"></param>
        /// <param name="token"></param>
        /// <param name="createOptions"></param>
        public static Task ExecuteAsync<TParam>(Action<TParam> action, TParam argument, CancellationToken token, TaskCreationOptions createOptions)
        {
            var task = new Task((arg) =>
            {
                var typedArg = (TParam)arg;
                action(typedArg);
            }, (object)argument, token, createOptions);
            task.Start(TaskScheduler.Default);
            return task;
        }
    }
}
