using System;
using System.Collections.Generic;
using System.Text;
using WoTMapWPF.Stores;

namespace WoTMapWPF.Services
{
    public class NavigationService<T> : INavigationService where T : ViewModelBase
    {
        private readonly NavigationStore navigationStore;
        private readonly Func<T> createViewModel;
        private readonly Func<T, bool>? checkValidity;

        public NavigationService(NavigationStore navigationStore, Func<T> createViewModel, Func<T, bool>? checkValidity = null)
        {
            this.navigationStore = navigationStore;
            this.createViewModel = createViewModel;
            this.checkValidity = checkValidity;
        }

        public bool Navigate()
        {
            T viewModel = createViewModel();
            if (checkValidity == null || checkValidity(viewModel))
            {
                navigationStore.ChangeViewModel(viewModel);
                return true;
            }
            return false;
        }
    }
}
