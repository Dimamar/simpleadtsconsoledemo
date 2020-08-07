using System.ComponentModel;
using System.Windows;
using MahApps.Metro.Controls;
using SimpleADTSConsole.Tools;

namespace SimpleADTSConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private double _oldHeigth = 700;
        private double _oldWidth = 1000;


        private readonly ConsoleVM _vm;

        public MainWindow()
        {
            _vm = Composition.Config(Dispatcher);
            DataContext = _vm;
            _vm.Subscribe(o => o.IsMetrogyMode, SwitchMode);
            InitializeComponent();
        }

        private void SwitchMode(bool isMetrogyMode)
        {
            if (isMetrogyMode)
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
            _vm.Dispose();
        }
    }
}
