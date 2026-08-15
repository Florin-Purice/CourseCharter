using System;
using System.Collections.Generic;
using System.Text;

namespace WoTMapWPF.Services
{
    public class NavigationService<T> : INavigationService where T : ViewModelBase
    {
        private readonly NavigationStore navigationStore;
        private readonly Func<T> createViewModel;
        private readonly string panelTag;
        private readonly string windowTitle;
        private readonly Func<T, bool>? checkValidity;

        public NavigationService(NavigationStore navigationStore, Func<T> createViewModel, string panelTag, string windowTitle, Func<T, bool>? checkValidity = null)
        {
            this.navigationStore = navigationStore;
            this.createViewModel = createViewModel;
            this.panelTag = panelTag;
            this.windowTitle = windowTitle;
            this.checkValidity = checkValidity;
        }

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
