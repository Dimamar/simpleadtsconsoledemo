using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using SimpleADTSConsole.Scripts.Steps;

namespace SimpleADTSConsole.Scripts
{
    internal class AdtsMeasuringRepeatsScript : IAdtsScript
    {
        private readonly double aimFrom = 760;
        private readonly double aimTo = 1060;
        private readonly double rateFast = 300;
        private readonly double rate = 20;
        private readonly int callAfter = 10;
        private TimeSpan waitTimeout = TimeSpan.FromMinutes(15);

        private readonly IEnumerable<string> _commandsPs = new[]
        {
            "MEAS:PRES? PS",
            "MEAS:PRES? PT",
            "MEAS:PRES? ALT",
        };
        private readonly IEnumerable<string> _commandsPsPtAlt = new[]
        {
            "MEAS:PRES? PS",
            "MEAS:PRES? PT",
            "MEAS:PRES? ALT",
        };

        public void Start(IADTSTransportModel adts, CancellationToken cancel)
        {

            var period = new StepToAim.PeriodDescriptor(TimeSpan.FromMilliseconds(50));

            var scheduler = new Scheduler(new Scheduler.IStep[]
            {
                    new StepToControl("S1", adts),
                    new StepToAim("S1", adts, period, aimFrom, rateFast, callAfter),
                    new StepToAim("S1", adts, period, aimTo, rate, callAfter),
                    new StepWait("S2", waitTimeout),
                    new StepToMeasuring("S3", adts),
                    new StepWait("S3", waitTimeout),
                    new StepToAim("S4", adts, period, aimFrom, rate, callAfter),
                    new StepToGround("S4", adts),
                    new StepToControl("S5", adts),
                    new StepWait("S5", waitTimeout),
                    new StepToMeasuring("S6", adts),
                    new StepWait("S6", waitTimeout),
            });

            var states = new[]
            {
                    new {commands = _commandsPsPtAlt.ToArray(), p = TimeSpan.FromMilliseconds(50)},
                    new {commands = _commandsPsPtAlt.ToArray(), p = TimeSpan.FromMilliseconds(100)},
                    new {commands = _commandsPsPtAlt.ToArray(), p = TimeSpan.FromMilliseconds(200)},
                    new {commands = _commandsPs.ToArray(), p = TimeSpan.FromMilliseconds(50)},
                    new {commands = _commandsPs.ToArray(), p = TimeSpan.FromMilliseconds(100)},
                    new {commands = _commandsPs.ToArray(), p = TimeSpan.FromMilliseconds(200)},
                };

            foreach (var state in states)
            {
                period.Period = state.p;
                RunScript(scheduler, adts, state.commands, period.Period, cancel);
                scheduler.Reset();
                if (cancel.IsCancellationRequested)
                    break;
            }
        }

        public string Name { get { return "Анализ повторов в обмене ADTS"; } }

        public double Progress { get; set; }

        public string Step { get; set; }

        private void RunScript(Scheduler scheduler, IADTSTransportModel adts, IEnumerable<string> commands, TimeSpan period, CancellationToken cancel)
        {
            var whEnd = new ManualResetEvent(false);
            var whGroup = new[] { whEnd, cancel.WaitHandle };
            while (WaitHandle.WaitAny(whGroup, period) == WaitHandle.WaitTimeout)
            {
                bool isFirst = true;
                foreach (var command in commands)
                {
                    if (!isFirst && cancel.WaitHandle.WaitOne(period))
                        break;
                    if (scheduler.IsEnd(cancel))
                    {
                        if (cancel.WaitHandle.WaitOne(period))
                            break;
                        var step = scheduler.Next(cancel);
                        if (step == null)
                        {
                            whEnd.Set();
                            break;
                        }
                    }
                    if (cancel.IsCancellationRequested)
                        break;
                    isFirst = false;
                    adts.Send(command);
                    adts.Read();
                    if (cancel.IsCancellationRequested)
                        break;
                }
            }
        }

    }
}