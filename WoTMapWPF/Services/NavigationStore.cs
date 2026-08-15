using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace WoTMapWPF.Services
{
    public partial class NavigationStore : ObservableObject
    {
        [ObservableProperty]
        public partial ViewModelBase? CurrentViewModel { get; set; }

        [ObservableProperty]
        public partial string WindowTitle { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string PanelTag { get; set; } = string.Empty;
    }
}
