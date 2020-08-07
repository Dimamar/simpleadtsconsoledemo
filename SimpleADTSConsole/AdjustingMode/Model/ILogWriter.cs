namespace SimpleADTSConsole.Tools
{
    public interface ILogWriter
    {
        /// <summary>
        /// Начало работы с логером
        /// </summary>
        void Start();

        /// <summary>
        /// Сделать запись в лог команды не требующую овтета
        /// </summary>
        /// <param name="state"></param>
        void PostCommand(CurrentParameterState state);

        /// <summary>
        /// Сделать запись в лог команды с ответом
        /// </summary>
        /// <param name="state"></param>
        void PostAnswer(CurrentParameterState state);
    }
}