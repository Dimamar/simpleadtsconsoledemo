using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleADTSConsole
{
    /// <summary>
    /// Interaction logic for ToogleNameButton.xaml
    /// </summary>
    public partial class ToogleNameButton : ToggleButton
    {
        public static readonly DependencyProperty OnLabelProperty = DependencyProperty.Register(
            "OnLabel", typeof(string), typeof(ToogleNameButton), new PropertyMetadata(default(string)));

        public string OnLabel
        {
            get { return (string) GetValue(OnLabelProperty); }
            set { SetValue(OnLabelProperty, value); }
        }

        public static readonly DependencyProperty OffLabelProperty = DependencyProperty.Register(
            "OffLabel", typeof(string), typeof(ToogleNameButton), new PropertyMetadata(default(string)));

        public string OffLabel
        {
            get { return (string) GetValue(OffLabelProperty); }
            set { SetValue(OffLabelProperty, value); }
        }

        public ToogleNameButton()
        {
            InitializeComponent();
        }
    }
}
