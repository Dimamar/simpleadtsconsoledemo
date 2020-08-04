using System.Windows;
using System.Windows.Controls.Primitives;

namespace SimpleADTSConsole.Tools
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
