using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using WoTMapWPF.Services;

namespace WoTMapWPF.CustomControls
{
    public partial class LoadMapControlViewModel : ViewModelBase
    {
        private readonly INavigationManager navigationManager;
        private readonly IMessenger messenger;
        private readonly MapManagerService mapManagerService;

        public LoadMapControlViewModel(
            INavigationManager navigationManager,
            IMessenger messenger,
            MapManagerService mapManagerService)
        {
            this.navigationManager = navigationManager;
            this.messenger = messenger;
            this.mapManagerService = mapManagerService;

            int imageHeight = 64;
            BitmapImage bitmapImage = new();
            Uri uri = new("../Res/preview_off.png", UriKind.Relative);
            StreamResourceInfo sri = App.GetResourceStream(uri);
            using (Stream stream = sri.Stream)
            {
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.DecodePixelHeight = imageHeight;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                PreviewOffBitmap = new WriteableBitmap(bitmapImage);
            }
            uri = new Uri("../Res/preview_on.png", UriKind.Relative);
            sri = App.GetResourceStream(uri);
            using (Stream stream = sri.Stream)
            {
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.DecodePixelHeight = imageHeight;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                PreviewOnBitmap = new WriteableBitmap(bitmapImage);
            }

            string? saveLocation = Settings.GetOrDefault<string>("SaveLocation");
            List<MapFileDefinition> maps = [];
            if (Directory.Exists($"{saveLocation}\\maps"))
                foreach (string subdir in Directory.GetDirectories($"{saveLocation}\\maps"))
                    foreach (string mapInfoFile in Directory.GetFiles(subdir, "*.info"))
                        try
                        {
                            string jsonString = File.ReadAllText(mapInfoFile);
                            MapFileDefinition? map = JsonSerializer.Deserialize<MapFileDefinition>(jsonString, Settings.Get<JsonSerializerOptions>("JsonSerializerOptions"));
                            if (map != null)
                                maps.Add(map);
                        }
                        catch { }
            maps.Sort((a, b) => a == null ? 1 : a.Name.CompareTo(b.Name));
            foreach (MapFileDefinition map in maps)
                Maps.Add(map);
        }

        [ObservableProperty]
        public partial MapFileDefinition? SelectedMap { get; set; }
        [ObservableProperty]
        public partial WriteableBitmap PreviewOnBitmap { get; set; }
        [ObservableProperty]
        public partial WriteableBitmap PreviewOffBitmap { get; set; }
        public ObservableCollection<MapFileDefinition> Maps { get; set; } = [];

        [RelayCommand]
        public void LoadMap()
        {
            if (SelectedMap != null)
            {
                if (mapManagerService.LoadMap(SelectedMap))
                    messenger.Send(new NotificationMessage(
                        Message: $"Loaded map \"{SelectedMap.Name}\".",
                        Type: NotificationType.ShowAndHide));
                else
                    messenger.Send(new NotificationMessage(
                        Message: $"Could not load map \"{SelectedMap.Name}\".",
                        Type: NotificationType.ShowError));
                navigationManager.Navigate(NavigationTarget.MapPanel);
            }
        }

        [RelayCommand]
        public void DeleteMap()
        {
            if (SelectedMap != null)
            {
                string message = $"Are you sure you want to delete the map \"{SelectedMap.Name}\"?\nThis can also result in the removal of associated paths.";
                ConfirmActionWindow caw = new(message);
                if (caw.ShowDialog().GetValueOrDefault())
                {
                    string? saveLocation = Settings.GetOrDefault<string>("SaveLocation");
                    List<MapFileDefinition> maps = [];
                    if (Directory.Exists($"{saveLocation}\\maps"))
                        foreach (string subdir in Directory.GetDirectories($"{saveLocation}\\maps"))
                            foreach (string mapInfoFile in Directory.GetFiles(subdir, "*.info"))
                                try
                                {
                                    string jsonString = File.ReadAllText(mapInfoFile);
                                    MapFileDefinition? map = JsonSerializer.Deserialize<MapFileDefinition>(jsonString, Settings.Get<JsonSerializerOptions>("JsonSerializerOptions"));
                                    if (map != null)
                                    {
                                        if (SelectedMap.Equals(map))
                                        {
                                            File.Delete(mapInfoFile);
                                            if (Directory.GetFiles(subdir, "*.info").Length < 1)
                                            {
                                                //also delete associated files (paths, map image) when there are no other map definitions using the same map image base
                                                Directory.Delete(subdir, true);
                                                messenger.Send(new NotificationMessage(
                                                    Message: $"Deleted map \"{SelectedMap.Name}\" and all associated files.",
                                                    Type: NotificationType.ShowAndHide));
                                            }
                                            else
                                                messenger.Send(new NotificationMessage(
                                                    Message: $"Deleted map \"{SelectedMap.Name}\".\nAssociated files that are used by other map definitions have not been deleted.",
                                                    Type: NotificationType.ShowAndHide));
                                        }
                                        else
                                            maps.Add(map);
                                    }
                                }
                                catch { }
                    if (maps.Count > 0)
                    {
                        SelectedMap = null;
                        Maps.Clear();
                        maps.Sort((a, b) => a == null ? 1 : a.Name.CompareTo(b.Name));
                        foreach (MapFileDefinition map in maps)
                            Maps.Add(map);
                    }
                    else
                        navigationManager.Navigate(NavigationTarget.MapPanel);
                }
            }
        }
    }
}
