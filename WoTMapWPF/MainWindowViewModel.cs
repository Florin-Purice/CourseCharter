using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WoTMapWPF.Services;
using WoTMapWPF.Stores;

namespace WoTMapWPF
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly NavigationStore navigationStore;
        private readonly INavigationService guideNavigationService;
        private readonly INavigationService loadMapNavigationService;
        private readonly INavigationService newMapNavigationService;
        [ObservableProperty]
        private string distanceUnit;
        [ObservableProperty]
        private double distanceUnitsPerPixel;
        [ObservableProperty]
        private string mapImageMD5;
        [ObservableProperty]
        private string mapName;
        [ObservableProperty]
        private string windowTitleBase;
        [ObservableProperty]
        private Path path;
        [ObservableProperty]
        private Path? oldPath;

        public MainWindowViewModel(NavigationStore navigationStore, INavigationService newMapNavigationService, INavigationService loadMapNavigationService, INavigationService guideNavigationService)
        {
            this.navigationStore = navigationStore;
            this.loadMapNavigationService = loadMapNavigationService;
            this.newMapNavigationService = newMapNavigationService;
            this.guideNavigationService = guideNavigationService;
            navigationStore.ViewModelChanged += NavigationStore_ViewModelChanged;
            DistanceUnit = "km";
            DistanceUnitsPerPixel = 1;
            MapImageMD5 = string.Empty;
            MapName = string.Empty;
            WindowTitleBase = "CourseCharter";
            Path = new Path();
            OldPath = null;
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
            {
                //to-do handle case
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
