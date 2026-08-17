using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Media.Imaging;
using WoTMapWPF.Services;

namespace WoTMapWPF.CustomControls
{
    public partial class NewMapControlViewModel : ViewModelBase
    {
        private readonly INavigationManager navigationManager;
        private readonly IMessenger messenger;
        private readonly MapManagerService mapManagerService;

        public NewMapControlViewModel(
            INavigationManager navigationManager,
            IMessenger messenger,
            MapManagerService mapManagerService)
        {
            this.navigationManager = navigationManager;
            this.messenger = messenger;
            this.mapManagerService = mapManagerService;

            string? saveLocation = Settings.GetOrDefault<string>("SaveLocation");
            List<string> existingMaps = [];
            if (Directory.Exists($"{saveLocation}\\maps"))
                foreach (string subdir in Directory.GetDirectories($"{saveLocation}\\maps"))
                    existingMaps.AddRange(Directory.GetFiles(subdir, "*.info"));
            ImageFileName = "-no image selected-";
            Name = "map_name";
            SamplePixels = 1;
            SampleUnits = 1;
            UnitLabel = "km";
            for (int i = 0; i < existingMaps.Count; i++)
                existingMaps[i] = System.IO.Path.GetFileNameWithoutExtension(existingMaps[i]);
            NameSuggestionValues = existingMaps;
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UnitsPerPixel))]
        [Range(1, int.MaxValue)]
        public partial int SampleUnits { get; set; }
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UnitsPerPixel))]
        [Range(1, int.MaxValue)]
        public partial int SamplePixels { get; set; }
        [ObservableProperty]
        public partial string Name { get; set; }
        [ObservableProperty]
        public partial string UnitLabel { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string ImageFileName { get; set; }
        [ObservableProperty]
        public partial string ImageFilePath { get; set; } = string.Empty;
        [ObservableProperty]
        public partial string ImageMD5 { get; set; } = string.Empty;
        [ObservableProperty]
        public partial List<string> NameSuggestionValues { get; set; } = [];
        [ObservableProperty]
        public partial double ImageHeight { get; set; } = 200;
        [ObservableProperty]
        public partial BitmapImage? MapImage { get; set; }

        public double UnitsPerPixel => (double)SampleUnits / SamplePixels;

        [RelayCommand]
        public void Save()
        {
            if (!string.IsNullOrWhiteSpace(Name) &&
                !string.IsNullOrWhiteSpace(ImageFileName) &&
                !string.IsNullOrWhiteSpace(ImageFilePath) &&
                !string.IsNullOrWhiteSpace(UnitLabel) &&
                !string.IsNullOrWhiteSpace(ImageMD5))
            {
                string? saveLocation = Settings.GetOrDefault<string>("SaveLocation");
                if (File.Exists($"{saveLocation}\\maps\\{ImageMD5}\\{Name}.info"))
                {
                    ConfirmActionWindow caw = new($"A map with the name \"{Name}\" already exists for the selected image base.\nDo you wish to replace it?");
                    if (!caw.ShowDialog().GetValueOrDefault())
                        return;
                }
                string imageFileExtension = System.IO.Path.GetExtension(ImageFilePath);
                MapFileDefinition map = new()
                {
                    Name = Name,
                    UnitLabel = UnitLabel,
                    ImageMD5 = ImageMD5,
                    SampleUnits = SampleUnits,
                    SamplePixels = SamplePixels,
                    ImageExt = imageFileExtension
                };
                string jsonString = JsonSerializer.Serialize(map, Settings.Get<JsonSerializerOptions>("JsonSerializerOptions"));
                Directory.CreateDirectory($"{saveLocation}\\maps\\{ImageMD5}");
                if (!File.Exists($"{saveLocation}\\maps\\{ImageMD5}\\map_image{imageFileExtension}"))
                    File.Copy(ImageFilePath, $"{saveLocation}\\maps\\{ImageMD5}\\map_image{imageFileExtension}", true);
                File.WriteAllText($"{saveLocation}\\maps\\{ImageMD5}\\{Name}.info", jsonString);
                messenger.Send(new NotificationMessage(
                    Message: $"Saved map \"{map.Name}\".",
                    Type: NotificationType.ShowAndHide));
                if (!mapManagerService.LoadMap(map))
                    messenger.Send(new NotificationMessage(
                        Message: $"Could not load map \"{map.Name}\".",
                        Type: NotificationType.ShowError));
                navigationManager.Navigate(NavigationTarget.MapPanel);
            }
        }

        [RelayCommand]
        public void SelectImage()
        {
            OpenFileDialog ofd = new();
            if (ofd.ShowDialog().GetValueOrDefault())
            {
                if (ofd.CheckFileExists)
                {
                    try
                    {
                        BitmapImage bitmapImage = new();
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = new Uri(ofd.FileName);
                        bitmapImage.DecodePixelHeight = (int)ImageHeight;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                        MapImage = bitmapImage;
                        using (MD5 md5 = MD5.Create())
                        {
                            using FileStream stream = File.OpenRead(ofd.FileName);
                            byte[] hashBytes = md5.ComputeHash(stream);
                            string hashString = Convert.ToHexStringLower(hashBytes);
                            ImageMD5 = hashString;
                        }
                        ImageFilePath = ofd.FileName;
                        ImageFileName = ofd.SafeFileName;
                    }
                    catch { }
                }
            }
        }
    }
}
