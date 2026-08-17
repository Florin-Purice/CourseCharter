using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace WoTMapWPF.CustomControls
{
    public class CustomTextBox : TextBox
    {
        public event EventHandler? TextApplied;

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    ApplyEdit();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    CancelEdit();
                    e.Handled = true;
                    break;
                default:
                    base.OnPreviewKeyDown(e);
                    break;
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            CancelEdit();
            base.OnLostFocus(e);
        }

        private void CancelEdit()
        {
            IsReadOnly = true;
            BindingExpression be = GetBindingExpression(TextBox.TextProperty);
            be?.UpdateTarget();
        }

        protected void ApplyEdit()
        {
            IsReadOnly = true;
            BindingExpression be = GetBindingExpression(TextBox.TextProperty);
            be?.UpdateSource();
            TextApplied?.Invoke(this, EventArgs.Empty);
        }
    }
}
