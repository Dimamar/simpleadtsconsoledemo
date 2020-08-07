using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using SimpleADTSConsole.PeriodControls.Model;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole.PeriodControls.ViewModel
{
    public class UpDownVM : INotifyPropertyChanged
    {
        private readonly UpDownModel _model;
        private UpDownContext _context;
        private string _periodUpDown;

        public UpDownVM(UpDownModel model)
        {
            _model = model;
        }

        // Up Down Test
        public string UpPs { get; set; }

        public string PeriodUpDown
        {
            get { return _periodUpDown; }
            set
            {
                if (_periodUpDown == value)
                    return;
                double sec;
                if (double.TryParse(value, out sec))
                {
                    _context.Period = TimeSpan.FromSeconds(sec);
                }
                else
                {
                    return;
                }
                _periodUpDown = value;
                OnPropertyChanged("PeriodUpDown");
            }
        }

        public ICommand UpDownTest
        {
            get
            {
                return new CommandWrapper(() => _model.UpDownTest(_context));
            }
        }

        public string DownPs
        {
            get { return _context.Down; }
            set { _context.Down = value; }
        }

        public string RatePs
        {
            get { return _context.Rate; }
            set
            {
                if (value == _context.Rate)
                    return;
                _context.Rate = value;
                OnPropertyChanged("RatePs");
                _model.UpdateUpDownParameters(_context);
            }
        }

        public ICommand GoToGround { get { return new CommandWrapper(() => _model.GoToGroundAsync()); } }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
