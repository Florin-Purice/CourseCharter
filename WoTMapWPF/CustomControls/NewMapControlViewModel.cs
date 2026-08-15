using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;
using WoTMapWPF.Services;

namespace WoTMapWPF.CustomControls
{
    public partial class NewMapControlViewModel : ViewModelBase
    {
        private readonly INavigationManager navigationManager;
        private readonly NotificationService notificationService;
        private readonly MapManagerService mapManagerService;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UnitsPerPixel))]
        [Range(1, int.MaxValue)]
        private int sampleUnits;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UnitsPerPixel))]
        [Range(1, int.MaxValue)]
        private int samplePixels;

        [ObservableProperty]
        private string name;
        [ObservableProperty]
        private string unitLabel = string.Empty;
        [ObservableProperty]
        private string imageFileName;
        [ObservableProperty]
        private string imageFilePath = string.Empty;
        [ObservableProperty]
        private string imageMD5 = string.Empty;
        [ObservableProperty]
        private List<string> nameSuggestionValues = new List<string>();
        [ObservableProperty]
        private double imageHeight = 200;
        [ObservableProperty]
        private BitmapImage? mapImage;

        public NewMapControlViewModel(
            INavigationManager navigationManager,
            NotificationService notificationService, 
            MapManagerService mapManagerService)
        {
            this.navigationManager = navigationManager;
            this.notificationService = notificationService;
            this.mapManagerService = mapManagerService;

            string? saveLocation = Settings.GetOrDefault<string>("SaveLocation");
            List<string> existingMaps = new List<string>();
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
                    ConfirmActionWindow caw = new ConfirmActionWindow($"A map with the name \"{Name}\" already exists for the selected image base.\nDo you wish to replace it?");
                    if (!caw.ShowDialog().GetValueOrDefault())
                        return;
                }
                string imageFileExtension = System.IO.Path.GetExtension(ImageFilePath);
                MapFileDefinition map = new MapFileDefinition();
                map.Name = Name;
                map.UnitLabel = UnitLabel;
                map.ImageMD5 = ImageMD5;
                map.SampleUnits = SampleUnits;
                map.SamplePixels = SamplePixels;
                map.ImageExt = imageFileExtension;

                string jsonString = JsonSerializer.Serialize(map, Settings.Get<JsonSerializerOptions>("JsonSerializerOptions"));
                Directory.CreateDirectory($"{saveLocation}\\maps\\{ImageMD5}");
                if (!File.Exists($"{saveLocation}\\maps\\{ImageMD5}\\map_image{imageFileExtension}"))
                    File.Copy(ImageFilePath, $"{saveLocation}\\maps\\{ImageMD5}\\map_image{imageFileExtension}", true);
                File.WriteAllText($"{saveLocation}\\maps\\{ImageMD5}\\{Name}.info", jsonString);
                notificationService.DoNotify(new NotificationMessage
                {
                    Message = $"Saved map \"{map.Name}\".",
                    Type = NotificationType.ShowAndHide
                });
                if (!mapManagerService.LoadMap(map))
                    notificationService.DoNotify(new NotificationMessage
                    {
                        Message = $"Could not load map \"{map.Name}\".",
                        Type = NotificationType.ShowError
                    });
                navigationManager.Navigate(NavigationTarget.MapPanel);
            }
        }

        [RelayCommand]
        public void SelectImage()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog().GetValueOrDefault())
            {
                if (ofd.CheckFileExists)
                {
                    try
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = new Uri(ofd.FileName);
                        bitmapImage.DecodePixelHeight = (int)ImageHeight;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                        MapImage = bitmapImage;
                        using (MD5 md5 = MD5.Create())
                        {
                            using (FileStream stream = File.OpenRead(ofd.FileName))
                            {
                                byte[] hashBytes = md5.ComputeHash(stream);
                                string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                                ImageMD5 = hashString;
                            }
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
