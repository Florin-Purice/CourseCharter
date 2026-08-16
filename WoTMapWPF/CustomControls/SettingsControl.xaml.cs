using System.Linq;
using System.Windows.Controls;

namespace WoTMapWPF.CustomControls
{
    public partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            ViewModel = DataContext as SettingsControlViewModel;
            ViewModel?.ResetToDefault += OnResetToDefault;
            InitializeComponent();
            //preselect the correct theme
            SelectCorrectListViewThemeItem();
        }

        public SettingsControlViewModel? ViewModel { get; private set; }

        private void OnResetToDefault()
        {
            SelectCorrectListViewThemeItem();
        }

        private void SelectCorrectListViewThemeItem()
        {
            string? themeName = Settings.GetOrDefault<string>("ThemeName");
            ListViewItem? item = ThemeListView.Items.Cast<ListViewItem>().Where(e => (string)e.Tag == themeName).FirstOrDefault();
            if (item != null)
                ThemeListView.SelectedItem = item;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = (ListView)sender;
            ListViewItem selectedItem = (ListViewItem)listView.SelectedItem;
            if (selectedItem != null)
            {
                string themeName = (string)selectedItem.Tag;
                ((App)App.Current).ChangeTheme(themeName);
                Settings.Set("ThemeName", themeName);
            }
        }
    }
}
