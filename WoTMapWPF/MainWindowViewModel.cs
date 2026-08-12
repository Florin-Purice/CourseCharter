using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WoTMapWPF.CustomControls;
using WoTMapWPF.Services;
using WoTMapWPF.Stores;

namespace WoTMapWPF
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly NavigationStore navigationStore;
        private readonly NotificationService notificationService;
        private readonly INavigationService mapNavigationService;
        private readonly INavigationService newMapNavigationService;
        private readonly INavigationService loadMapNavigationService;
        private readonly INavigationService savePathNavigationService;
        private readonly INavigationService loadPathNavigationService;
        private readonly INavigationService guideNavigationService;
        private readonly INavigationService settingsNavigationService;
        [ObservableProperty]
        private NotificationControlViewModel notificationControlViewModel;
        [ObservableProperty]
        private string windowTitleBase;

        public MainWindowViewModel(
            NavigationStore navigationStore, 
            MapManagerService mapManagerService,
            NotificationControlViewModel notificationControlViewModel,
            NotificationService notificationService,
            INavigationService mapNavigationService,
            INavigationService newMapNavigationService, 
            INavigationService loadMapNavigationService, 
            INavigationService savePathNavigationService,
            INavigationService loadPathNavigationService,
            INavigationService guideNavigationService,
            INavigationService settingsNavigationService)
        {
            this.navigationStore = navigationStore;
            MapManagerService = mapManagerService;
            NotificationControlViewModel = notificationControlViewModel;
            this.notificationService = notificationService;
            this.mapNavigationService = mapNavigationService;
            this.newMapNavigationService = newMapNavigationService;
            this.loadMapNavigationService = loadMapNavigationService;
            this.savePathNavigationService = savePathNavigationService;
            this.loadPathNavigationService = loadPathNavigationService;
            this.guideNavigationService = guideNavigationService;
            this.settingsNavigationService = settingsNavigationService;
            navigationStore.ViewModelChanged += NavigationStore_ViewModelChanged;
            WindowTitleBase = "CourseCharter";
        }

        public ViewModelBase? CurrentViewModel => navigationStore.CurrentViewModel;

        public MapManagerService MapManagerService { get; }

        [RelayCommand]
        public void ShowMapPanel()
        {
            mapNavigationService.Navigate();
        }

        [RelayCommand]
        public void ShowNewMapPanel()
        {
            newMapNavigationService.Navigate();
        }

        [RelayCommand]
        public void ShowLoadMapPanel()
        {
            if (!loadMapNavigationService.Navigate())
                mapNavigationService.Navigate();
        }

        [RelayCommand]
        public void ShowSavePathPanel()
        {
            if (!savePathNavigationService.Navigate())
            {
                notificationService.DoNotify(new NotificationMessage
                {
                    Message = "There is no path to save.",
                    Type = NotificationType.ShowAndHide
                });
                mapNavigationService.Navigate();
            }
        }

        [RelayCommand]
        public void ShowLoadPathPanel()
        {
            if (!loadPathNavigationService.Navigate())
            {
                notificationService.DoNotify(new NotificationMessage
                {
                    Message = "No saved paths found for current map.",
                    Type = NotificationType.ShowAndHide
                });
                mapNavigationService.Navigate();
            }
        }

        [RelayCommand]
        public void ShowGuidePanel()
        {
            guideNavigationService?.Navigate();
        }

        [RelayCommand]
        public void ShowSettingsPanel()
        {
            settingsNavigationService?.Navigate();
        }

        private void NavigationStore_ViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}
