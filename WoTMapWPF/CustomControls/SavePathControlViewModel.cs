using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using WoTMapWPF.Graphics;
using WoTMapWPF.Services;

namespace WoTMapWPF.CustomControls
{
    public partial class SavePathControlViewModel : ViewModelBase
    {
        private readonly INavigationManager navigationManager;
        private readonly IMessenger messenger;
        private readonly MapManagerService mapManagerService;

        public SavePathControlViewModel(
            INavigationManager navigationManager,
            IMessenger messenger,
            MapManagerService mapManagerService)
        {
            this.navigationManager = navigationManager;
            this.messenger = messenger;
            this.mapManagerService = mapManagerService;

            if (Path?.Nodes.Count < 1)
            {
                IsValid = false;
            }
            else
            {
                string? saveLocation = Settings.GetOrDefault<string>("SaveLocation");
                string? mapImageHash = mapManagerService.MapInfo?.ImageMD5;
                List<string> existingPaths = [];
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

        [ObservableProperty]
        public partial string Name { get; set; } = string.Empty;
        [ObservableProperty]
        public partial List<string> NameSuggestionValues { get; set; } = [];

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
                    ConfirmActionWindow caw = new($"A path with the name \"{Name}\" already exists.\nDo you wish to replace it?");
                    if (!caw.ShowDialog().GetValueOrDefault())
                        return;
                }
                PathFileDefinition pathFileDefinition = new()
                {
                    Name = Name,
                    ImageMD5 = imageMD5,
                    Path = mapManagerService.ActivePath
                };
                string jsonString = JsonSerializer.Serialize(pathFileDefinition, Settings.Get<JsonSerializerOptions>("JsonSerializerOptions"));
                Directory.CreateDirectory($"{saveLocation}\\maps\\{imageMD5}\\paths");
                File.WriteAllText($"{saveLocation}\\maps\\{imageMD5}\\paths\\{Name}.info", jsonString);
                messenger.Send(new NotificationMessage(
                    Message: $"Saved path \"{pathFileDefinition.Name}\".",
                    Type: NotificationType.ShowAndHide));
                navigationManager.Navigate(NavigationTarget.MapPanel);
            }
        }
    }
}
