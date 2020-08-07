using System.Windows.Threading;
using IGPIBTransfer;
using SimpleADTSConsole.AdjustingMode;
using SimpleADTSConsole.AdjustingMode.Model;
using SimpleADTSConsole.ForTest;
using SimpleADTSConsole.Main.Model;
using SimpleADTSConsole.MetrologyMode;
using SimpleADTSConsole.MetrologyMode.Model;
using SimpleADTSConsole.PeriodControls;
using SimpleADTSConsole.PeriodControls.Model;
using SimpleADTSConsole.PeriodControls.ViewModel;
using SimpleADTSConsole.Properties;
using SimpleADTSConsole.Scripts.ViewModel;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole
{
    class Composition
    {
        public static ConsoleVM Config(Dispatcher dispatcher)
        {
            ConnectionType type = Settings.Default.ConnectionType;
            IzGPIBDataTransfer transfer = new zGPIBDataTransfer(new MoqIee488(100));
            
            var cmdObservable = new BaseObservable<CommandAction>();
            var transport = new AdtsTransportModel(transfer, cmdObservable);
            var periodicCommand = new PeriodicCommands(transport);

            var scripts = new ScriptsVM(new ScriptsModel(transport));
            var upDown = new UpDownVM(new UpDownModel(periodicCommand));
            var periodLoopContext = new PeriodVm(periodicCommand, dispatcher);
            var commandCompiler = new CommandCompilerVM(new CommandCompilerModel(transport, periodicCommand));

            var adjusting = new AdjustingModelVM(scripts, upDown, periodLoopContext, commandCompiler);

            BusySynchronizer synchronizer = new BusySynchronizer();
            MetrologyVM metrology = new MetrologyVM(new MetrologyModel(transport, periodicCommand), synchronizer);
            LogReaderVM logReder = new LogReaderVM(new LogReader(new ReadLogConfig() { Period = periodicCommand.CurrentPeriod }, transport));
            ConsoleModel console = new ConsoleModel(transport, periodicCommand);
            return new ConsoleVM(console, adjusting, metrology, logReder, synchronizer);
        }
    }
}
