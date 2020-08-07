using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleADTSConsole.Scripts;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole.AdjustingMode.Model
{
    public class ScriptsModel
    {
        private IDictionary<string, IAdtsScript> _scripts;
        private string _statPath;
        private CancellationTokenSource _cancellation;
        private IADTSTransportModel _model;

        public ScriptsModel(IADTSTransportModel model)
        {
            _cancellation = new CancellationTokenSource();
            _model = model;
            _statPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                Properties.Settings.Default.BaseStatisticPath);
            _scripts = new IAdtsScript[]
            {
                new AdtsMeasuringRepeatsScript(),
            }.ToDictionary(k => k.Name, v => v);
        }

        public IEnumerable<string> Scripts => _scripts.Keys;

        public Task StartScriptAsync(string script)
        {
            var token = _cancellation.Token;
            return TaskExec.ExecuteAsync((cncl) => DoStartScript(_scripts[script], cncl), token, token);
        }

        private void DoStartScript(IAdtsScript script, CancellationToken token)
        {
            script.Start(_model, token);
        }

    }
}