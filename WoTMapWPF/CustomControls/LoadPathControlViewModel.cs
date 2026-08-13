using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using WoTMapWPF.Services;
using static WoTMapWPF.App;

namespace WoTMapWPF.CustomControls
{
    public partial class LoadPathControlViewModel : ViewModelBase
    {
        private readonly INavigationManager navigationManager;
        private readonly NotificationService notificationService;
        private readonly CreateNewPath createNewPath;
        private readonly MapManagerService mapManagerService;

        public LoadPathControlViewModel(
            INavigationManager navigationManager,
            NotificationService notificationService,
            CreateNewPath createNewPath,
            MapManagerService mapManagerService)
        {
            this.navigationManager = navigationManager;
            this.notificationService = notificationService;
            this.createNewPath = createNewPath;
            this.mapManagerService = mapManagerService;

            string? saveLocation = Settings.GetOrDefault<string>("SaveLocation");
            List<PathFileDefinition> paths = new List<PathFileDefinition>();
            string dir = $"{saveLocation}\\maps\\{mapManagerService.MapInfo?.ImageMD5}\\paths";
            if (Directory.Exists(dir))
                foreach (string pathInfoFile in Directory.GetFiles(dir, "*.info"))
                    try
                    {
                        string jsonString = File.ReadAllText(pathInfoFile);
                        PathFileDefinition? path = JsonSerializer.Deserialize<PathFileDefinition>(jsonString, new JsonSerializerOptions()
                        {
                            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                            WriteIndented = true
                        });
                        if (path != null)
                        {
                            //add necesary ref to MapManager
                            path.Path?.SetMapManager(mapManagerService);
                            paths.Add(path);
                        }
                    }
                    catch { }
            paths.Sort((a, b) => a == null ? 1 : a.Name.CompareTo(b.Name));
            foreach (PathFileDefinition path in paths)
                Paths.Add(path);
        }

        [ObservableProperty]
        public partial PathFileDefinition? SelectedPath { get; set; }

        public ObservableCollection<PathFileDefinition> Paths { get; set; } = new ObservableCollection<PathFileDefinition>();

        [RelayCommand]
        private void LoadPath()
        {
            if (SelectedPath != null)
            {
                mapManagerService.ChangePathAndClearHistory(createNewPath());
                try
                {
                    mapManagerService.ChangePathAndClearHistory(SelectedPath.Path);
                    mapManagerService.StorePathState();
                    notificationService.DoNotify(new NotificationMessage
                    {
                        Message = $"Loaded path \"{SelectedPath.Name}\".",
                        Type = NotificationType.ShowAndHide
                    });
                }
                catch
                {
                    notificationService.DoNotify(new NotificationMessage
                    {
                        Message = "Could not load path.",
                        Type = NotificationType.ShowError
                    });
                }
                navigationManager.Navigate(NavigationTarget.MapPanel);
            }
        }

        [RelayCommand]
        private void DeletePath()
        {
            if (SelectedPath != null)
            {
                string message = $"Are you sure you want to delete the path \"{SelectedPath.Name}\"?";
                ConfirmActionWindow caw = new ConfirmActionWindow(message);
                if (caw.ShowDialog().GetValueOrDefault())
                {
                    string? saveLocation = Settings.GetOrDefault<string>("SaveLocation");
                    List<PathFileDefinition> paths = new List<PathFileDefinition>();
                    string dir = $"{saveLocation}\\maps\\{mapManagerService.MapInfo?.ImageMD5}\\paths";
                    if (Directory.Exists(dir))
                        foreach (string pathInfoFile in Directory.GetFiles(dir, "*.info"))
                            try
                            {
                                string jsonString = File.ReadAllText(pathInfoFile);
                                PathFileDefinition? path = JsonSerializer.Deserialize<PathFileDefinition>(jsonString, new JsonSerializerOptions()
                                {
                                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                                    WriteIndented = true
                                });
                                if (path != null)
                                {
                                    if (SelectedPath.Equals(path))
                                    {
                                        File.Delete(pathInfoFile);
                                        notificationService.DoNotify(new NotificationMessage
                                        {
                                            Message = $"Deleted path \"{SelectedPath.Name}\".",
                                            Type = NotificationType.ShowAndHide
                                        });
                                    }
                                    else
                                        paths.Add(path);
                                }
                            }
                            catch { }
                    if (paths.Count > 0)
                    {
                        SelectedPath = null;
                        Paths.Clear();
                        paths.Sort((a, b) => a == null ? 1 : a.Name.CompareTo(b.Name));
                        foreach (PathFileDefinition path in paths)
                            Paths.Add(path);
                    }
                    else
                        navigationManager.Navigate(NavigationTarget.MapPanel);
                }
            }
        }
    }
}
