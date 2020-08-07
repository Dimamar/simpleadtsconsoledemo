using System.Collections.Generic;
using System.Threading;

namespace SimpleADTSConsole.Scripts
{
    internal class Scheduler
    {
        private readonly IEnumerable<IStep> _stepsList;
        private IEnumerator<IStep> _steps;
        private bool _isStarted = false;

        public Scheduler(IEnumerable<IStep> steps)
        {
            _stepsList = steps;
            _steps = _stepsList.GetEnumerator();
        }

        public bool IsEnd(CancellationToken cancel)
        {
            if (!_isStarted)
                return true;
            if (_steps.Current == null)
                return true;

            return _steps.Current.IsEnd(cancel);
        }

        public IStep Next(CancellationToken cancel)
        {
            _isStarted = true;
            if (!_steps.MoveNext())
                return null;
            _steps.Current.Run(cancel);
            return _steps.Current;
        }

        public void Reset()
        {
            _isStarted = false;
            _steps = _stepsList.GetEnumerator();
        }

        public interface IStep
        {
            /// <summary>
            /// Название шага
            /// </summary>
            string Name { get; }

            /// <summary>
            /// Запустить шаг
            /// </summary>
            /// <param name="cancel"></param>
            /// <returns></returns>
            bool Run(CancellationToken cancel);

            /// <summary>
            /// Проверить что шаг окончен
            /// </summary>
            /// <param name="cancel"></param>
            /// <returns></returns>
            bool IsEnd(CancellationToken cancel);
        }
    }
}