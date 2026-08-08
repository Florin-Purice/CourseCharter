using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace WoTMapWPF.CustomControls
{
    /// <summary>
    /// Interaction logic for SavePathControl.xaml
    /// </summary>
    public partial class SavePathControl : UserControl
    {
        public SavePathControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Filter out characters that are not valid for a file's name.
        /// Handler for TextChanged event of a TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileNameFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textboxSender = (TextBox)sender;
            int cursorPosition = textboxSender.SelectionStart;
            char[] invalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();
            char[] invalidPathChars = System.IO.Path.GetInvalidPathChars();
            char[] invalidChars = invalidFileNameChars.Concat(invalidPathChars).ToArray();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (char c in textboxSender.Text)
                if (!invalidChars.Contains(c))
                    stringBuilder.Append(c);
            textboxSender.Text = stringBuilder.ToString();
            textboxSender.SelectionStart = cursorPosition;
        }
    }
}
