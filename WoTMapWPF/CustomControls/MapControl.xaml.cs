using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WoTMapWPF.Graphics;

namespace WoTMapWPF.CustomControls
{
    public partial class MapControl : UserControl
    {
        public MapControl()
        {
            ViewModel = (MapControlViewModel)DataContext;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            InitializeComponent();
        }

        public MapControlViewModel ViewModel { get; private set; }

        private void PathNodesInfoListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = (ListView)sender;
            if (listView.SelectedItem != null)
            {
                PathNode selectedNode = (PathNode)listView.SelectedItem;
                ViewModel.Path?.SelectedIndex = selectedNode.Index;
            }
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PathNodesInfoListView.Tag = "ShowNamedOnly";
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PathNodesInfoListView.Tag = "ShowAll";
        }

        private void NameBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Focus();
            tb.IsReadOnly = false;
            tb.SelectAll();
        }
        private void ListViewItem_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ((ListViewItem)sender).IsSelected = true;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Path":
                    if(ViewModel.Path != null)
                    {
                        PathNodesInfoListView.ItemsSource = ViewModel.Path.Nodes;
                        ChangeListViewSelectedNode(ViewModel.Path.SelectedIndex);
                        //ViewModel.Scene?.SetPath(ViewModel.Path);
                        //ResetPathSubscriptions(ViewModel.OldPath, ViewModel.Path);
                    }
                    break;
            }
        }

        private void ChangeListViewSelectedNode(int nodeIndex)
        {
            if (nodeIndex >= 0 && nodeIndex < PathNodesInfoListView.Items.Count)
            {
                PathNode selectedNode = (PathNode)PathNodesInfoListView.Items[nodeIndex];
                if (selectedNode != PathNodesInfoListView.SelectedItem)
                    PathNodesInfoListView.SelectedItem = selectedNode;
            }
            else
                PathNodesInfoListView.UnselectAll();
        }
    }
}
