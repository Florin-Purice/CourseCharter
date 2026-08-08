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
        private readonly INavigationService guideNavigationService;
        [ObservableProperty]
        private NotificationControlViewModel notificationControlViewModel;
        [ObservableProperty]
        private string windowTitleBase;

        public MainWindowViewModel(
            NavigationStore navigationStore, 
            NotificationControlViewModel notificationControlViewModel,
            NotificationService notificationService,
            INavigationService mapNavigationService,
            INavigationService newMapNavigationService, 
            INavigationService loadMapNavigationService, 
            INavigationService savePathNavigationService,
            INavigationService guideNavigationService)
        {
            this.navigationStore = navigationStore;
            NotificationControlViewModel = notificationControlViewModel;
            this.notificationService = notificationService;
            this.mapNavigationService = mapNavigationService;
            this.newMapNavigationService = newMapNavigationService;
            this.loadMapNavigationService = loadMapNavigationService;
            this.savePathNavigationService = savePathNavigationService;
            this.guideNavigationService = guideNavigationService;
            navigationStore.ViewModelChanged += NavigationStore_ViewModelChanged;
            WindowTitleBase = "CourseCharter";
        }

        public ViewModelBase? CurrentViewModel => navigationStore.CurrentViewModel;

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
        public void ShowGuidePanel()
        {
            guideNavigationService?.Navigate();
        }

        private void NavigationStore_ViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}
