using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using WoTMapWPF.Graphics;
using static WoTMapWPF.App;

namespace WoTMapWPF.Services
{
    public partial class MapManagerService : ObservableObject
    {
        private readonly Stack<Path> backwardStack = new Stack<Path>();
        private readonly Stack<Path> forwardStack = new Stack<Path>();
        private readonly CreateNewPath createNewPath;
        [ObservableProperty]
        private Path? activePath;

        public MapManagerService(CreateNewPath createNewPath)
        {
            Map = new Map();
            this.createNewPath = createNewPath;
        }

        public event Action? NewMapLoaded;
        public event Action? ActivePathAltered;
        public event Action<PropertyChangedEventArgs>? ActivePathPropertiesChanged;
        public event Action? AutosavePossible;

        [ObservableProperty]
        public partial Map? Map { get; private set; }
        [ObservableProperty]
        public partial MapFileDefinition? MapInfo { get; private set; }

        public bool LoadMap(MapFileDefinition mfd)
        {
            try
            {
                string? saveLocation = Settings.GetOrDefault<string>("SaveLocation");
                if (saveLocation == null)
                    return false;
                Map = new Map($"{saveLocation}\\maps\\{mfd.ImageMD5}\\map_image{mfd.ImageExt}");
                MapInfo = mfd;
                ChangePathAndClearHistory(createNewPath());
                //if autosave enabled try load autosaved path
                bool? isPathAutosaveEnabled = Settings.GetOrDefault<bool>("IsPathAutosaveEnabled");
                if (isPathAutosaveEnabled.GetValueOrDefault())
                    try
                    {
                        string fileName = $"{saveLocation}\\maps\\{mfd.ImageMD5}\\autosave\\__autosave_path__.info";
                        if (File.Exists(fileName))
                        {
                            string jsonString = File.ReadAllText(fileName);
                            PathFileDefinition? pathDef = JsonSerializer.Deserialize<PathFileDefinition>(jsonString, new JsonSerializerOptions()
                            {
                                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                                WriteIndented = true
                            });
                            if (pathDef != null && pathDef.Path != null)
                            {
                                //add necesary ref to MapManager
                                pathDef.Path.SetMapManager(this);
                                ChangePathAndClearHistory(pathDef.Path);
                                StorePathState();
                            }
                        }
                    }
                    catch { }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void StorePathState()
        {
            if (ActivePath != null)
            {
                //add state to undo-stack, clear the redo-stack
                backwardStack.Push((Path)ActivePath.Clone());
                forwardStack.Clear();
                OnAutosavePossible();
            }
        }

        public void UndoPathState()
        {
            if (backwardStack.Count > 1)
            {
                forwardStack.Push(backwardStack.Pop());
                Path path = backwardStack.Peek();
                ActivePath = (Path)path.Clone();
                OnAutosavePossible();
            }
        }

        public void RedoPathState()
        {
            if (forwardStack.Count > 0)
            {
                Path path = forwardStack.Pop();
                backwardStack.Push(path);
                ActivePath = (Path)path.Clone();
                OnAutosavePossible();
            }
        }

        public void ChangePathAndClearHistory(Path? newPath)
        {
            ActivePath = newPath;
            backwardStack.Clear();
            forwardStack.Clear();
        }

        private void OnAutosavePossible()
        {
            AutosavePossible?.Invoke();
        }

        partial void OnMapChanged(Map? value)
        {
            NewMapLoaded?.Invoke();
        }

        partial void OnActivePathChanged(Path? oldValue, Path? newValue)
        {
            //redo event subs
            if (oldValue != null)
            {
                oldValue.PropertyChanged -= ActivePath_PropertyChanged;
                oldValue.PathChanged -= ActivePath_PathChanged;
            }
            newValue?.PropertyChanged += ActivePath_PropertyChanged;
            newValue?.PathChanged += ActivePath_PathChanged;
        }

        private void ActivePath_PathChanged(object? sender, EventArgs e)
        {
            StorePathState();
            ActivePathAltered?.Invoke();
        }

        private void ActivePath_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            ActivePathPropertiesChanged?.Invoke(e);
        }
    }
}
