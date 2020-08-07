using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using SimpleADTSConsole.AdjustingMode.Model;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole.Scripts.ViewModel
{
    public class ScriptsVM : INotifyPropertyChanged
    {
        private readonly ScriptsModel _scripts;

        public ScriptsVM(ScriptsModel scripts)
        {
            _scripts = scripts;

            Scripts = new ObservableCollection<string>(_scripts.Scripts);
            SelectedScript = Scripts.First();

        }


        public ObservableCollection<string> Scripts { get; private set; }

        public string SelectedScript { get; set; }

        public ICommand StartSelectScript
        {
            get { return new CommandWrapper(() => _scripts.StartScriptAsync(SelectedScript)); }
        }


        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
