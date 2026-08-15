using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WoTMapWPF.CustomControls;
using WoTMapWPF.Services;

namespace WoTMapWPF
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly INavigationManager navigationManager;
        private readonly NotificationService notificationService;
        [ObservableProperty]
        private NotificationControlViewModel notificationControlViewModel;
        [ObservableProperty]
        private string windowTitleBase;

        public MainWindowViewModel(
            NavigationStore navigationStore, 
            INavigationManager navigationManager,
            NotificationService notificationService,
            MapManagerService mapManagerService,
            NotificationControlViewModel notificationControlViewModel)
        {
            NavigationStore = navigationStore;
            this.navigationManager = navigationManager;
            MapManagerService = mapManagerService;
            NotificationControlViewModel = notificationControlViewModel;
            this.notificationService = notificationService;
            WindowTitleBase = "CourseCharter";
        }

        public NavigationStore NavigationStore { get; }
        public MapManagerService MapManagerService { get; }

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
                notificationService.DoNotify(new NotificationMessage
                {
                    Message = "No saved maps found. Please create a map first.",
                    Type = NotificationType.ShowAndHide
                });
                navigationManager.Navigate(NavigationTarget.MapPanel);
            }
        }

        [RelayCommand]
        public void ShowSavePathPanel()
        {
            if (!navigationManager.Navigate(NavigationTarget.SavePathPanel))
            {
                notificationService.DoNotify(new NotificationMessage
                {
                    Message = "There is no path to save.",
                    Type = NotificationType.ShowAndHide
                });
                navigationManager.Navigate(NavigationTarget.MapPanel);
            }
        }

        [RelayCommand]
        public void ShowLoadPathPanel()
        {
            if (!navigationManager.Navigate(NavigationTarget.LoadPathPanel))
            {
                notificationService.DoNotify(new NotificationMessage
                {
                    Message = "No saved paths found for current map.",
                    Type = NotificationType.ShowAndHide
                });
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
