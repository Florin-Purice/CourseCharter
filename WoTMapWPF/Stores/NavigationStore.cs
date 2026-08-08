using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace WoTMapWPF.Stores
{
    public partial class NavigationStore
    {
        private ViewModelBase? currentViewModel;

        public event Action? ViewModelChanged;

        public ViewModelBase? CurrentViewModel 
        {
            get => currentViewModel;
            set
            {
                currentViewModel = value;
                OnCurrentViewModelChanged();
            }
        }

        public void ChangeViewModel(ViewModelBase viewModel)
        {
            CurrentViewModel = viewModel;
        }

        private void OnCurrentViewModelChanged()
        {
            ViewModelChanged?.Invoke();
        }
    }
}
