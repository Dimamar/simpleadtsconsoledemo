using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace SimpleADTSConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private double _oldHeigth = 700;
        private double _oldWidth = 1000;

        private ADTSConsoleModel _model = new ADTSConsoleModel();
        private ConsoleViewModel _viewModel;
        private double[] line;
        public MainWindow()
        {
            line = new double[100];
            line.Count();
            _viewModel = new ConsoleViewModel(_model, this.Dispatcher, ShowMsg);
            DataContext = _viewModel;
            _viewModel.PropertyChanged += _viewModel_PropertyChanged;
            InitializeComponent();
            SwitchMode();
        }

        private void ShowMsg(string title, string msg)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(()=>this.ShowMessageAsync("", msg)));
        }

        private void _viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsMetrogyMode")
                return;
            this.Dispatcher.Invoke(new Action(SwitchMode));
        }

        private void SwitchMode()
        {
            if (_viewModel.IsMetrogyMode)
            {
                _oldHeigth = Height;
                _oldWidth = Width;
                this.MaxHeight = 120;
                this.MaxWidth = 360;
                this.MinHeight = 120;
                this.MinWidth = 360;
                this.ResizeMode = ResizeMode.NoResize;
            }
            else
            {
                this.MaxHeight = double.PositiveInfinity;
                this.MaxWidth = double.PositiveInfinity;
                this.MinHeight = 0;
                this.MinWidth = 0;
                Height = _oldHeigth;
                Width = _oldWidth;
                this.ResizeMode = ResizeMode.CanResize;
            }
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _viewModel.Dispose();
        }
    }
}
