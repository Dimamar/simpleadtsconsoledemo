using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole.PeriodControls.Model
{
    public class CommandCompilerModel
    {
        private readonly IADTSTransportModel _model;
        private readonly PeriodicCommands _periodicContext;
        private IDictionary<string, string> _parameters;

        public CommandCompilerModel(IADTSTransportModel model, PeriodicCommands periodicContext)
        {
            _model = model;
            _periodicContext = periodicContext;
            _parameters = new Dictionary<string, string>()
            {
                {"Высота","ALT"},
                {"Калибровочная скорость", "CAS"},
                {"Истинная воздушная скорость", "TAS"},
                {"Махи", "MACH"},
                {"Отношение давления в двигателе", "EPR"},
                {"Статическое давление", "PS"},
                {"Полное (динамическое) давление", "PT"},
                { "Дифференциальное давление", "QC"}
            };
        }

        public IEnumerable<string> Parameters => _parameters.Keys;

        public string CompileGetCommand(string key)
        {
            return string.Format(CultureInfo.GetCultureInfo("en-US"), "MEAS:PRES? {0}", _parameters[key]);
        }

        public string CompileSetCommand(string key, string val)
        {
            return string.Format(CultureInfo.GetCultureInfo("en-US"), "SOUR:PRES {0},{1}", _parameters[key], val);
        }

        public string ToControl()
        {
            return "SOUR:STAT ON";
        }

        public Task SendAsync(string command)
        {
            return TaskExec.ExecuteAsync(() => _periodicContext.DoSend(command, false), CancellationToken.None);
        }

        public Task SendReceiveAsync(string command)
        {
            return TaskExec.ExecuteAsync(() => _periodicContext.DoSend(command, true), CancellationToken.None);
        }
    }
}
