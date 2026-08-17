using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace WoTMapWPF.CustomControls
{
    /// <summary>
    /// Interaction logic for NewMapControl.xaml
    /// </summary>
    public partial class NewMapControl : UserControl
    {
        [GeneratedRegex("[^0-9a-zA-Z _.-]")]
        private static partial Regex AlphaNumRegex();
        [GeneratedRegex("[^0-9]")]
        private static partial Regex NumRegex();

        public NewMapControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Filter that allows only alphanum characters.
        /// Handler for TextChanged event of a TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlphaNumFilter_TextChanged(object sender, EventArgs e)
        {
            TextBox textboxSender = (TextBox)sender;
            int cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = AlphaNumRegex().Replace(textboxSender.Text, "");
            textboxSender.SelectionStart = cursorPosition;
        }

        /// <summary>
        /// Filter that allows only numerical characters.
        /// Handler for TextChanged event of a TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumFilter_TextChanged(object sender, EventArgs e)
        {
            TextBox textboxSender = (TextBox)sender;
            int cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = NumRegex().Replace(textboxSender.Text, "");
            textboxSender.SelectionStart = cursorPosition;
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
            char[] invalidChars = [.. invalidFileNameChars, .. invalidPathChars];
            StringBuilder stringBuilder = new();
            foreach (char c in textboxSender.Text)
                if (!invalidChars.Contains(c))
                    stringBuilder.Append(c);
            textboxSender.Text = stringBuilder.ToString();
            textboxSender.SelectionStart = cursorPosition;
        }
    }
}
