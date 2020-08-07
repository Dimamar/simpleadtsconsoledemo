using System;
using System.ComponentModel;
using SimpleADTSConsole.PeriodControls;
using SimpleADTSConsole.PeriodControls.ViewModel;
using SimpleADTSConsole.Scripts.ViewModel;

namespace SimpleADTSConsole.AdjustingMode
{
    public class AdjustingModelVM : INotifyPropertyChanged
    {
        public AdjustingModelVM(
            ScriptsVM scripts,
            UpDownVM upDown,
            PeriodVm periodLoopContext,
            CommandCompilerVM commandCompiler)
        {
            Scripts = scripts;
            UpDown = upDown;
            PeriodLoopContext = periodLoopContext;
            CommandCompiler = commandCompiler;
        }

        #region Periodic

        public PeriodVm PeriodLoopContext { get; private set; }

        #endregion

        #region Scripts

        public ScriptsVM Scripts { get; private set; }

        #endregion

        #region Simple

        public CommandCompilerVM CommandCompiler { get; private set; }

        #endregion

        #region Up down Test

        public UpDownVM UpDown { get; private set; }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
