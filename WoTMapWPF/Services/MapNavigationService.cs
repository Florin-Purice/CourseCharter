using System;
using System.Collections.Generic;
using System.Text;
using WoTMapWPF.CustomControls;

namespace WoTMapWPF.Services
{
    public class MapNavigationService : INavigationService
    {
        private readonly NavigationStore navigationStore;
        private readonly Func<MapControlViewModel> createViewModel;
        private readonly MapManagerService mapManagerService;

        public MapNavigationService(NavigationStore navigationStore, Func<MapControlViewModel> createViewModel, MapManagerService mapManagerService)
        {
            this.navigationStore = navigationStore;
            this.createViewModel = createViewModel;
            this.mapManagerService = mapManagerService;
        }

        public bool Navigate()
        {
            string mapName = mapManagerService.MapInfo != null ? $" - {mapManagerService.MapInfo.Name}" : string.Empty;
            string windowTitle = $"{Settings.Get<string>("AppTitle")}{mapName}";
            MapControlViewModel viewModel = createViewModel();
            navigationStore.CurrentViewModel = viewModel;
            navigationStore.WindowTitle = windowTitle;
            navigationStore.PanelTag = "Map";
            return true;
        }
    }
}
