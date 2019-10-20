using System;

namespace SimpleADTSConsole
{
    public class zGPIBDataTransfer
    {
        private int tick = 0;
        private int tickLastCommand = 0;
        private const string DTymeStringFormat = "hh:mm:ss.fff";

        private Action<string> _toLogAction = null;
        /// <summary>
        /// ����� �����������
        /// </summary>
        public Action<string> ToLogAction
        {
            get { return _toLogAction; }
            set
            {
                _toLogAction = value;
                _isTrustLogAction = true;
            }
        }

        /// <summary>
        /// �������, ��� ToLogAction ��������� � �� ���� �� ������� ����������
        /// ������ ��� �� ��������� ����������� ����������� �������� ���������� 
        /// </summary>
        private bool _isTrustLogAction = true;

        #region Open
        public int Open(int board, int address)
        {
            return 0;
        }
        #endregion

        #region Send
        public bool Send(int id, object command)
        {
            return true;
        }
        #endregion

        #region Receive
        public object Receive(int id)
        {
            return null;
        }
        #endregion

        public void Clear(int id)
        {
        }
    }
}
