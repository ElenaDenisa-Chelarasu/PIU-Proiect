using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SFAudioView.GUI
{
    /// <summary>
    /// Interaction logic for AmplifyEffectDialog.xaml
    /// </summary>
    public partial class AmplifyEffectDialog : Window
    {
        public static readonly DependencyProperty ResultProperty 
            = DependencyProperty.Register(nameof(Result), typeof(double), typeof(AmplifyEffectDialog), new UIPropertyMetadata(1.0));

        public double Result
        {
            get => (double)GetValue(ResultProperty);
            set => SetValue(ResultProperty, value);
        }

        public AmplifyEffectDialog()
        {
            InitializeComponent();
        }

        private void OkClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
