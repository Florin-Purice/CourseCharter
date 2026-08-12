using System;
using System.Collections.Generic;
using System.Text;

namespace WoTMapWPF.Services
{
    public interface INavigationManager
    {
        bool Navigate(NavigationTarget navigateTo);
        void Register(NavigationTarget navigationTarget, INavigationService navigationService);
    }

    public class NavigationManager : INavigationManager
    {
        private readonly Dictionary<NavigationTarget, INavigationService> registeredNavs = [];

        public bool Navigate(NavigationTarget navigateTo)
        {
            if (registeredNavs.ContainsKey(navigateTo))
            {
                return registeredNavs[navigateTo].Navigate();
            }
            else
                return false;
        }

        public void Register(NavigationTarget navigationTarget, INavigationService navigationService) => registeredNavs[navigationTarget] = navigationService;
    }

    public enum NavigationTarget
    {
        MapPanel,
        NewMapPanel,
        LoadMapPanel,
        SavePathPanel,
        LoadPathPanel,
        GuidePanel,
        SettingsPanel
    }
}
