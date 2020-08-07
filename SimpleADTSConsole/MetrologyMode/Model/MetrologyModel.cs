using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SimpleADTSConsole.Scripts.Steps;

namespace SimpleADTSConsole.MetrologyMode.Model
{
    public class MetrologyModel
    {
        private readonly IADTSTransportModel _model;
        private readonly PeriodicCommands _periodicContext;

        public MetrologyModel(IADTSTransportModel model, PeriodicCommands periodicContext)
        {
            _model = model;
            _periodicContext = periodicContext;
        }


        public void DoToControl(CancellationToken token)
        {
            StepToControl.Run(_model, token);
        }

        public void DoToMeasuring(CancellationToken token)
        {
            StepToMeasuring.Run(_model, token);
        }

        public void DoSwitchAutoZero(bool state)
        {
            var cmd = state ? "CALC:AZER ON" : "CALC:AZER OFF";
            _periodicContext.DoSend(cmd, false);
        }

        public void DoSwitchAutoLeak(bool state)
        {
            var cmd = state ? "SOUR:MODE:ALE ON" : "SOUR:MODE:ALE OFF";
            _periodicContext.DoSend(cmd, false);
        }

    }
}
