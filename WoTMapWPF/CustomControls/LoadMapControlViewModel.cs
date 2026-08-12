using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using WoTMapWPF.Graphics;
using WoTMapWPF.Services;

namespace WoTMapWPF.CustomControls
{
    public partial class LoadMapControlViewModel : ViewModelBase
    {
        private readonly NotificationService notificationService;
        private readonly MapManagerService mapManagerService;
        private readonly INavigationService mapNavigationService;

        [ObservableProperty]
        private MapFileDefinition? selectedMap;
        [ObservableProperty]
        private WriteableBitmap previewOnBitmap;
        [ObservableProperty]
        private WriteableBitmap previewOffBitmap;

        public LoadMapControlViewModel(NotificationService notificationService, MapManagerService mapManagerService, INavigationService mapNavigationService)
        {
            this.notificationService = notificationService;
            this.mapManagerService = mapManagerService;
            this.mapNavigationService = mapNavigationService;

            int imageHeight = 64;
            BitmapImage bitmapImage = new BitmapImage();
            Uri uri = new Uri("../Res/preview_off.png", UriKind.Relative);
            StreamResourceInfo sri = App.GetResourceStream(uri);
            using (Stream stream = sri.Stream)
            {
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.DecodePixelHeight = imageHeight;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                previewOffBitmap = new WriteableBitmap(bitmapImage);
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
                previewOnBitmap = new WriteableBitmap(bitmapImage);
            }

            string? saveLocation = Settings.GetOrDefault<string>("SaveLocation");
            List<MapFileDefinition> maps = new List<MapFileDefinition>();
            if (Directory.Exists($"{saveLocation}\\maps"))
                foreach (string subdir in Directory.GetDirectories($"{saveLocation}\\maps"))
                    foreach (string mapInfoFile in Directory.GetFiles(subdir, "*.info"))
                        try
                        {
                            string jsonString = File.ReadAllText(mapInfoFile);
                            MapFileDefinition? map = JsonSerializer.Deserialize<MapFileDefinition>(jsonString, new JsonSerializerOptions()
                            {
                                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                                WriteIndented = true
                            });
                            if (map != null)
                                maps.Add(map);
                        }
                        catch { }
            maps.Sort((a, b) => a == null ? 1 : a.Name.CompareTo(b.Name));
            foreach (MapFileDefinition map in maps)
                Maps.Add(map);
            ////also recolor preview button images in case the theme was changed
            //SolidColorBrush brush = (SolidColorBrush)App.Current.Resources["ThemeColorText"];
            //if (PreviewOffBitmap != null)
            //    BitmapColorChanger.ChangeColorKeepAlpha(PreviewOffBitmap, brush.Color);
            //if (PreviewOnBitmap != null)
            //    BitmapColorChanger.ChangeColorKeepAlpha(PreviewOnBitmap, brush.Color);
        }

        public ObservableCollection<MapFileDefinition> Maps { get; set; } = new ObservableCollection<MapFileDefinition>();

        [RelayCommand]
        public void LoadMap()
        {
            if (SelectedMap != null)
            {
                if (mapManagerService.LoadMap(SelectedMap))
                    notificationService.DoNotify(new NotificationMessage
                    {
                        Message = $"Loaded map \"{SelectedMap.Name}\".",
                        Type = NotificationType.ShowAndHide
                    });
                else
                    notificationService.DoNotify(new NotificationMessage
                    {
                        Message = $"Could not load map \"{SelectedMap.Name}\".",
                        Type = NotificationType.ShowError
                    });
                mapNavigationService.Navigate();
            }
        }

        [RelayCommand]
        public void DeleteMap()
        {
            if (SelectedMap != null)
            {
                string message = $"Are you sure you want to delete the map \"{SelectedMap.Name}\"?\nThis can also result in the removal of associated paths.";
                ConfirmActionWindow caw = new ConfirmActionWindow(message);
                if (caw.ShowDialog().GetValueOrDefault())
                {
                    string? saveLocation = Settings.GetOrDefault<string>("SaveLocation");
                    List<MapFileDefinition> maps = new List<MapFileDefinition>();
                    if (Directory.Exists($"{saveLocation}\\maps"))
                        foreach (string subdir in Directory.GetDirectories($"{saveLocation}\\maps"))
                            foreach (string mapInfoFile in Directory.GetFiles(subdir, "*.info"))
                                try
                                {
                                    string jsonString = File.ReadAllText(mapInfoFile);
                                    MapFileDefinition? map = JsonSerializer.Deserialize<MapFileDefinition>(jsonString, new JsonSerializerOptions()
                                    {
                                        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                                        WriteIndented = true
                                    });
                                    if (map != null)
                                    {
                                        if (SelectedMap.Equals(map))
                                        {
                                            File.Delete(mapInfoFile);
                                            if (Directory.GetFiles(subdir, "*.info").Length < 1)
                                            {
                                                //also delete associated files (paths, map image) when there are no other map definitions using the same map image base
                                                Directory.Delete(subdir, true);
                                                notificationService.DoNotify(new NotificationMessage
                                                {
                                                    Message = $"Deleted map \"{SelectedMap.Name}\" and all associated files.",
                                                    Type = NotificationType.ShowAndHide
                                                });
                                            }
                                            else
                                                notificationService.DoNotify(new NotificationMessage
                                                {
                                                    Message = $"Deleted map \"{SelectedMap.Name}\".\nAssociated files that are used by other map definitions have not been deleted.",
                                                    Type = NotificationType.ShowAndHide,
                                                    HideDelay = 6000
                                                });
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
                        mapNavigationService.Navigate();
                }
            }
        }
    }
}
