using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using WoTMapWPF.Services;

namespace WoTMapWPF.CustomControls
{
    public partial class SavePathControlViewModel : ViewModelBase
    {
        private readonly NotificationService notificationService;
        private readonly MapManagerService mapManagerService;
        private readonly INavigationService mapNavigationService;

        [ObservableProperty]
        private string name = string.Empty;
        [ObservableProperty]
        private List<string> nameSuggestionValues = new List<string>();

        public SavePathControlViewModel(
            NotificationService notificationService,
            MapManagerService mapManagerService,
            INavigationService mapNavigationService)
        {
            this.notificationService = notificationService;
            this.mapManagerService = mapManagerService;
            this.mapNavigationService = mapNavigationService;

            if (Path?.Nodes.Count < 1)
            {
                IsValid = false;
            }
            else
            {
                string? saveLocation = Settings.GetOrDefault<string>("SaveLocation");
                string? mapImageHash = mapManagerService.MapInfo?.ImageMD5;
                List<string> existingPaths = new List<string>();
                string dir = $"{saveLocation}\\maps\\{mapImageHash}\\paths";
                if (Directory.Exists(dir))
                    existingPaths.AddRange(Directory.GetFiles(dir, "*.info"));
                Name = "path_name";
                for (int i = 0; i < existingPaths.Count; i++)
                    existingPaths[i] = System.IO.Path.GetFileNameWithoutExtension(existingPaths[i]);
                NameSuggestionValues = existingPaths;
                IsValid = true;
            }
        }

        public Path? Path => mapManagerService.ActivePath;
        public bool IsValid { get; private set; } = true;

        [RelayCommand]
        public void Save()
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                string? saveLocation = Settings.GetOrDefault<string>("SaveLocation");
                string? imageMD5 = mapManagerService.MapInfo?.ImageMD5;
                if (File.Exists($"{saveLocation}\\maps\\{imageMD5}\\paths\\{Name}.info"))
                {
                    ConfirmActionWindow caw = new ConfirmActionWindow($"A path with the name \"{Name}\" already exists.\nDo you wish to replace it?");
                    if (!caw.ShowDialog().GetValueOrDefault())
                        return;
                }
                PathFileDefinition pathFileDefinition = new PathFileDefinition();
                pathFileDefinition.Name = Name;
                pathFileDefinition.ImageMD5 = imageMD5;
                pathFileDefinition.Path = mapManagerService.ActivePath;
                string jsonString = JsonSerializer.Serialize(pathFileDefinition, new JsonSerializerOptions()
                {
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    WriteIndented = true
                });
                Directory.CreateDirectory($"{saveLocation}\\maps\\{imageMD5}\\paths");
                File.WriteAllText($"{saveLocation}\\maps\\{imageMD5}\\paths\\{Name}.info", jsonString);
                notificationService.DoNotify(new NotificationMessage
                {
                    Message = $"Saved path \"{pathFileDefinition.Name}\".",
                    Type = NotificationType.ShowAndHide
                });
                mapNavigationService.Navigate();
            }
        }
    }
}
