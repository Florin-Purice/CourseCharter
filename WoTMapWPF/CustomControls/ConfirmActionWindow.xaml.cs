using System.Windows;

namespace WoTMapWPF.CustomControls
{
    /// <summary>
    /// Interaction logic for ConfirmActionWindow.xaml
    /// </summary>
    public partial class ConfirmActionWindow : Window
    {
        public ConfirmActionWindow(string message)
        {
            InitializeComponent();
            ((ConfirmActionWindowViewModel)DataContext).Message = message;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
