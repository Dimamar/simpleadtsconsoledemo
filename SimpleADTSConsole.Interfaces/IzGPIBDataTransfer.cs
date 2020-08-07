using System;

namespace IGPIBTransfer
{
    public interface IzGPIBDataTransfer
    {
        /// <summary>
        /// Метод логирования
        /// </summary>
        Action<string> ToLogAction { get; set; }

        /// <summary>
        /// Метод логирования ошибок
        /// </summary>
        Action<string> ToLogErrorAction { get; set; }

        int Open(int board, int address);
        bool Send(int id, object command);
        object Receive(int id);
        void Clear(int id);
    }
}
