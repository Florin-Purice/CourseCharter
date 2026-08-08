using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using WoTMapWPF.Graphics;

namespace WoTMapWPF.Services
{
    public partial class MapManagerService : ObservableObject
    {
        private readonly JsonSerializerOptions jsonSerializerOptions;
        private readonly Stack<Path> backwardStack = new Stack<Path>();
        private readonly Stack<Path> forwardStack = new Stack<Path>();

        [ObservableProperty]
        private Path? activePath;

        public MapManagerService()
        {
            jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
            jsonSerializerOptions.WriteIndented = true;
        }

        public event Action NewMapLoaded;
        public event Action? ActivePathAltered;
        public event Action<PropertyChangedEventArgs>? ActivePathPropertiesChanged;
        public event Action? AutosavePossible;

        [ObservableProperty]
        public partial Map? Map { get; private set; }

        public bool LoadMap(MapFileDefinition map)
        {
            try
            {
                string? saveLocation = Settings.Get<string>("SaveLocation");
                if (saveLocation == null)
                    return false;
                new Map($"{saveLocation}\\maps\\{map.ImageMD5}\\map_image{map.ImageExt}");
                ChangePathAndClearHistory(new Path());
                //if autosave enabled try load autosaved path
                bool? isPathAutosaveEnabled = Settings.Get<bool>("IsPathAutosaveEnabled");
                if (isPathAutosaveEnabled.GetValueOrDefault())
                    try
                    {
                        string fileName = $"{saveLocation}\\maps\\{map.ImageMD5}\\autosave\\__autosave_path__.info";
                        if (File.Exists(fileName))
                        {
                            string jsonString = File.ReadAllText(fileName);
                            PathFileDefinition? pathDef = JsonSerializer.Deserialize<PathFileDefinition>(jsonString, jsonSerializerOptions);
                            if (pathDef != null && pathDef.Path != null)
                            {
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

        public void ChangePathAndClearHistory(Path newPath)
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
            ActivePathAltered?.Invoke();
        }

        private void ActivePath_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            ActivePathPropertiesChanged?.Invoke(e);
        }
    }
}
