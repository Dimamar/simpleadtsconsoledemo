using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using SimpleADTSConsole.Scripts.Steps;

namespace SimpleADTSConsole.Scripts
{
    internal class AdtsMeasuringRepeatsScript : INotifyPropertyChanged, IAdtsScript
    {
        private TimeSpan _period = TimeSpan.FromMilliseconds(100);

        private string parameter = "PS";
        private string unit = "MMHG";
        private string unitCommandFormat = "UNIT:PRES {0}";
        private string aimCommandFormat = "SOUR:PRES {0},{1}";
        private double aimFrom = 760;
        private double aimTo = 1060;
        private double rateFast = 300;
        private double rate = 20;
        private int callAfter = 10;
        private TimeSpan waitTimeout = TimeSpan.FromMinutes(15);

        private readonly IEnumerable<string> _commandsPs = new[]
        {
            "MEAS:PRES? PS",
            "MEAS:PRES? PT",
            "MEAS:PRES? ALT",
        };
        private readonly IEnumerable<string> _commandsPsPtAlt = new []
        {
            "MEAS:PRES? PS",
            "MEAS:PRES? PT",
            "MEAS:PRES? ALT",
        };
        private readonly string _basePath;

        public AdtsMeasuringRepeatsScript(string basePath)
        {
            _basePath = basePath;
        }

        public void Start(IADTSConsoleModel adts, CancellationToken cancel)
        {
            var startTime = DateTime.Now;
            var currentRunDir = string.Format("{0:yy.MM.dd.hh.mm.ss}\\", startTime);
            var observer = new StatisticObserver(Path.Combine(_basePath, currentRunDir));
            using (adts.Subscribe(observer))
            {
                observer.SetDir("S1");
                var period = new StepToAim.PeriodDescriptor(TimeSpan.FromMilliseconds(50));

                var sheduller = new Sheduller(new Sheduller.IStep[]
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
                    RunScript(sheduller, adts, state.commands, period.Period, observer, cancel);
                    sheduller.Reset();
                    if (cancel.IsCancellationRequested)
                        break;
                }
            }
        }

        public string Name { get { return "Анализ повторов в обмене ADTS"; } }

        public double Progress { get; set; }

        public string Step { get; set; }

        public AdtsMeasuringRepeatsScript SetPeriod(TimeSpan period)
        {
            _period = period;
            return this;
        }

        private void RunScript(Sheduller sheduller, IADTSConsoleModel adts, IEnumerable<string> commands, TimeSpan period, StatisticObserver observer, CancellationToken cancel)
        {
            var dirFormat = "{0}" + string.Format("_{0}m_{1}cmd", (int)period.TotalMilliseconds, commands.Count());
            var whEnd = new ManualResetEvent(false);
            var whGroup = new[] { whEnd, cancel.WaitHandle};
            while (WaitHandle.WaitAny(whGroup, period) == WaitHandle.WaitTimeout)
            {
                bool isFirst = true;
                foreach (var command in commands)
                {
                    if (!isFirst && cancel.WaitHandle.WaitOne(period))
                        break;
                    if (sheduller.IsEnd(cancel))
                    {
                        if (cancel.WaitHandle.WaitOne(period))
                            break;
                        var step = sheduller.Next(cancel);
                        if (step == null)
                        {
                            whEnd.Set();
                            break;
                        }
                        observer.SetDir(string.Format(dirFormat, step.Name));
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}