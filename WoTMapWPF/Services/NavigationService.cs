using System;

namespace WoTMapWPF.Services
{
    public class NavigationService<T>(
        NavigationStore navigationStore, 
        Func<T> createViewModel, 
        string panelTag, 
        string windowTitle, 
        Func<T, bool>? checkValidity = null
        ) : INavigationService where T : ViewModelBase
    {
        public bool Navigate()
        {
            T viewModel = createViewModel();
            if (checkValidity == null || checkValidity(viewModel))
            {
                navigationStore.CurrentViewModel = viewModel;
                navigationStore.WindowTitle = windowTitle;
                navigationStore.PanelTag = panelTag;
                return true;
            }
            return false;
        }
    }
}
