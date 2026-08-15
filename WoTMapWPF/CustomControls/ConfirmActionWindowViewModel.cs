using CommunityToolkit.Mvvm.ComponentModel;

namespace WoTMapWPF.CustomControls
{
    public partial class ConfirmActionWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string Message { get; set; } = string.Empty;
    }
}
