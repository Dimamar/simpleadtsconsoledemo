using System;
using System.ComponentModel;

namespace SimpleADTSConsole.Tools
{
    /// <summary>
    ///  Описатель по одному параметру
    /// </summary>
    public class CurrentParameterState
    {
        /// <summary>
        /// Название параметра
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Текст команды
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        /// Ответ
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Дата и время последней дублирующей команды
        /// </summary>vs
        public DateTime LastCommand { get; set; }

        /// <summary>
        /// Дата и время ответа на последнюю дублирующую команду
        /// </summary>
        public DateTime LastAnswer { get; set; }

        /// <summary>
        /// Дата и время первой команды
        /// </summary>
        public DateTime FirstCommand { get; set; }

        /// <summary>
        /// Дата и время ответа на первую команду
        /// </summary>
        public DateTime FirstAnswer { get; set; }

        /// <summary>
        /// Текущее количество повторов
        /// </summary>
        public int CurrentRepeats { get; set; }

        /// <summary>
        /// Общее количество повторов с момента накопления статистики
        /// </summary>
        public int CountAllRepeats { get; set; }

        /// <summary>
        /// Максимальное количество неприрывных повторов
        /// </summary>
        public int MaxCountRepeat { get; set; }
    }
}