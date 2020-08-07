using System;
using IGPIBTransfer;
using SimpleADTSConsole.Interfaces.IEEE488;

namespace SimpleADTSConsole
{
    public class zGPIBDataTransfer : IzGPIBDataTransfer
    {
        private int _tick = 0;
        private int _tickLastCommand = 0;
        private const string DTymeStringFormat = "hh:mm:ss.fff";
        private readonly IIee488 _transport;
        
        private Action<string> _toLogAction = null;
        private Action<string> _toLogErrorAction = null;

        public zGPIBDataTransfer(IIee488 transport)
        {
            _transport = transport;
        }

        /// <summary>
        /// Говорит, что ToLogAction действует и ни разу не генерил исключение
        /// Введен что бы исключить возможность постоянного генережа исключений 
        /// </summary>
        private bool _isTrustLogAction = true;
        private bool _isTrustLogErrorAction = true;

        /// <summary>
        /// Метод логирования
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
        /// Метод логирования
        /// </summary>
        public Action<string> ToLogErrorAction
        {
            get { return _toLogErrorAction; }
            set
            {
                _toLogErrorAction = value;
                _isTrustLogAction = true;
            }
        }

        #region Open
        public int Open(int board, int address)
        {
            int id = _transport.ibdev(board, address, 0, Timeout.T3s, true, 0);
            _transport.ibclr(id);
            return id;
        }
        #endregion

        #region Send
        public bool Send(int id, object command)
        {
            string cmd = command as string;
            if (cmd == null) throw new Exception("Incorrect Command");

            _tick = Environment.TickCount;
            string fromLastCmd = "";
            if (_tickLastCommand != 0)
                fromLastCmd = string.Format("[FromLast:{0}]", _tick - _tickLastCommand);
            _tickLastCommand = _tick;
            try
            {
                _transport.ibwrt(id, cmd);
                string note = string.Format("[{1}]Command:{0}{2}", cmd, DateTime.Now.ToString(DTymeStringFormat), fromLastCmd);
                ToLog(note);
                return true;
            }
            catch (IEEE488Exception ex)
            {
                ToErrorLog(string.Format("Send Exception: {0}\nError by code: {1}", ex.ToString(), TranslateExc(ex)));
                string note = string.Format("[{0}]Command:ERROR", DateTime.Now.ToString(DTymeStringFormat));
                ToLog(note);
                return false;
            }
        }
        #endregion

        #region Receive
        public object Receive(int id)
        {
            string answer = null;
            string note = null;
            try
            {
                answer = _transport.ibrd(id).Trim();
                if (answer != null)
                {
                    note = string.Format("[{1}]Answer:{0}", answer, DateTime.Now.ToString(DTymeStringFormat));
                }
            }
            catch (IEEE488Exception ex)
            {
                ToErrorLog(string.Format("Receive Exception: {0}\nError by code: {1}", ex.ToString(), TranslateExc(ex)));
                note = string.Format("[{0}]Answer:ERROR", DateTime.Now.ToString(DTymeStringFormat));
                ToLog(note);
            }

            int time = Environment.TickCount - _tick;
            note = note + string.Format("\t(send receive interval:{0})", time);
            ToLog(note);

            return answer;
        }
        #endregion

        public void Clear(int id)
        {
            _transport.ibclr(id);
        }

        private void ToLog(string note)
        {
            if (ToLogAction == null)
                return;
            if (!_isTrustLogAction)
                return;
            try
            {
                ToLogAction(note);
            }
            catch
            {
                _isTrustLogAction = false;
            }
        }

        private void ToErrorLog(string note)
        {
            if (ToLogErrorAction == null)
                return;
            if (!_isTrustLogErrorAction)
                return;
            try
            {
                ToLogErrorAction(note);
            }
            catch
            {
                _isTrustLogErrorAction = false;
            }
        }

        private string TranslateExc(IEEE488Exception ex)
        {
            var startmsg = "WrapLib.IEEE488.IEEE488Exception:";
            if (!ex.Message.StartsWith(startmsg))
                return "Undefined";
            var numStr = ex.Message.Substring(startmsg.Length, ex.Message.Length - startmsg.Length).Trim();
            int index;
            if (!int.TryParse(numStr, out index))
                return "Undefined";
            return GetErrByNum(index);
        }

        private string GetErrByNum(int er)
        {
            switch (er)
            {
                case 0: return "No error";
                case 1: return "Syntax error";
                case 3: return "Device not accessible";
                case 4: return "Invalid link identifier";
                case 5: return "Parameter error";
                case 6: return "Channel not established";
                case 8: return "Operation not supported";
                case 9: return "Out of resources";
                case 11: return "Device locked by another link";
                case 12: return "No lock held by this link";
                case 15: return "I/O timeout";
                case 17: return "I/O error";
                case 21: return "Invalid address";
                case 23: return "Abort";
                case 29: return "Channel already established";
                default: return "Device error code " + er;
            }
        }
    }
}
