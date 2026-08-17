using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WoTMapWPF.CustomControls;
using WoTMapWPF.Services;

namespace WoTMapWPF
{
    public partial class MainWindowViewModel(
        NavigationStore navigationStore,
        INavigationManager navigationManager,
        IMessenger messenger,
        MapManagerService mapManagerService,
        NotificationControlViewModel notificationControlViewModel
        ) : ViewModelBase
    {
        [ObservableProperty]
        public partial string WindowTitleBase { get; set; } = "CourseCharter";
        [ObservableProperty]
        public partial NotificationControlViewModel NotificationControlViewModel { get; set; } = notificationControlViewModel;

        public NavigationStore NavigationStore { get; } = navigationStore;
        public MapManagerService MapManagerService { get; } = mapManagerService;

        [RelayCommand]
        public void ShowMapPanel()
        {
            navigationManager.Navigate(NavigationTarget.MapPanel);
        }

        [RelayCommand]
        public void ShowNewMapPanel()
        {
            navigationManager.Navigate(NavigationTarget.NewMapPanel);
        }

        [RelayCommand]
        public void ShowLoadMapPanel()
        {
            if (!navigationManager.Navigate(NavigationTarget.LoadMapPanel))
            {
                messenger.Send(new NotificationMessage(
                    Message: "No saved maps found. Please create a map first.",
                    Type: NotificationType.ShowAndHide));
                navigationManager.Navigate(NavigationTarget.MapPanel);
            }
        }

        [RelayCommand]
        public void ShowSavePathPanel()
        {
            if (!navigationManager.Navigate(NavigationTarget.SavePathPanel))
            {
                messenger.Send(new NotificationMessage(
                    Message: "There is no path to save.",
                    Type: NotificationType.ShowAndHide));
                navigationManager.Navigate(NavigationTarget.MapPanel);
            }
        }

        [RelayCommand]
        public void ShowLoadPathPanel()
        {
            if (!navigationManager.Navigate(NavigationTarget.LoadPathPanel))
            {
                messenger.Send(new NotificationMessage(
                    Message: "No saved paths found for current map.",
                    Type: NotificationType.ShowAndHide));
                navigationManager.Navigate(NavigationTarget.MapPanel);
            }
        }

        [RelayCommand]
        public void ShowGuidePanel()
        {
            navigationManager.Navigate(NavigationTarget.GuidePanel);
        }

        [RelayCommand]
        public void ShowSettingsPanel()
        {
            navigationManager.Navigate(NavigationTarget.SettingsPanel);
        }
    }
}
