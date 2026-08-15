using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WoTMapWPF.CustomControls
{
    public class AutoCompleteTextBox : TextBox
    {
        private string currentInput = string.Empty;
        private string currentSuggestion = string.Empty;
        public static readonly DependencyProperty SuggestionValuesProperty = DependencyProperty.Register(
            "SuggestionValues", typeof(ICollection<string>), typeof(AutoCompleteTextBox),
            new FrameworkPropertyMetadata(default(ICollection)));

        public ICollection<string> SuggestionValues
        {
            get => (ICollection<string>)GetValue(SuggestionValuesProperty);
            set => SetValue(SuggestionValuesProperty, value);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            string input = Text;
            if (SuggestionValues != null && input.Length > currentInput.Length && input != currentSuggestion)
            {
                currentSuggestion = SuggestionValues.FirstOrDefault(s => s.StartsWith(input)) ?? string.Empty;
                if (!string.IsNullOrEmpty(currentSuggestion))
                {
                    int selectionStart = input.Length;
                    int selectionLength = currentSuggestion.Length - input.Length;
                    Text = currentSuggestion;
                    Select(selectionStart, selectionLength);
                }
            }
            currentInput = input;
        }
    }
}
