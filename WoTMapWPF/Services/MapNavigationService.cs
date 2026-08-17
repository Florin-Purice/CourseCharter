using System;
using WoTMapWPF.CustomControls;

namespace WoTMapWPF.Services
{
    public class MapNavigationService(
        NavigationStore navigationStore, 
        Func<MapControlViewModel> createViewModel, 
        MapManagerService mapManagerService
        ) : INavigationService
    {
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
