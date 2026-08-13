using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenTK.Wpf;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using WoTMapWPF.Graphics;
using WoTMapWPF.Services;
using static WoTMapWPF.App;

namespace WoTMapWPF.CustomControls
{
    public partial class MapControlViewModel : ViewModelBase
    {
        private readonly Scene scene;
        private readonly CreateNewPath createNewPath;
        private readonly MapManagerService mapManagerService;
        private double[] oldMousePos = [];

        public MapControlViewModel(
            Scene scene,
            CreateNewPath createNewPath,
            MapManagerService mapManagerService,
            GLWpfControl gLWpfControl)
        {
            this.scene = scene;
            this.createNewPath = createNewPath;
            this.mapManagerService = mapManagerService;
            mapManagerService.PropertyChanged += MapManagerService_PropertyChanged;
            GLControl = gLWpfControl;
            OnPropertyChanged(nameof(GLControl));
            InitializeGlControl();
        }

        public GLWpfControl GLControl { get; private set; }
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNodesInfoHidden))]
        public partial bool IsNodesInfoVisible { get; private set; } = false;
        public bool IsNodesInfoHidden => !IsNodesInfoVisible;
        public Scene Scene => scene;
        public MapManagerService MapManagerService => mapManagerService;
        public Map Map => MapManagerService.Map;
        public Path? Path => MapManagerService.ActivePath;
        public double DistanceUnitsPerPixel 
        {
            get
            {
                double x = 0;
                if(MapManagerService.MapInfo != null)
                    x = (double)MapManagerService.MapInfo.SampleUnits/MapManagerService.MapInfo.SamplePixels;
                return x;
            }  
        }

        [RelayCommand]
        public void ToggleNodesInfoVisibility() => IsNodesInfoVisible = !IsNodesInfoVisible;

        private void MapManagerService_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MapManagerService.ActivePath))
                OnPropertyChanged(nameof(Path));
        }

        #region GLCONTROL
        private void InitializeGlControl()
        {
            GLControl.Focusable = true;
            GLControl.Render += OnGLControlRender;
            GLControl.SizeChanged += OnGLControlSizeChanged;
            GLControl.MouseWheel += OnGLControlMouseWheel;
            GLControl.MouseMove += OnGLControlMouseMove;
            GLControl.KeyDown += OnGLControlKeyDown;
            GLControl.MouseUp += OnGLControlMouseUp;
            GLControl.MouseLeave += OnGLControlMouseLeave;
            GLControl.MouseLeftButtonUp += OnGLControlMouseLeftButtonUp;

            Timer timer = new(1000d / 60d)
            {
                AutoReset = true,
                Enabled = true
            };
            timer.Elapsed += (s, e) =>
            {
                try
                {
                    Application currentApp = App.Current;
                    if (currentApp != null)
                    {
                        ((App)App.Current).Dispatcher.Invoke(() => GLControl.InvalidateVisual());
                    }
                }
                catch { }
            };
        }

        private void OnGLControlRender(TimeSpan obj) => scene?.Paint();

        private void OnGLControlSizeChanged(object sender, SizeChangedEventArgs e) => scene?.Resize();

        private void OnGLControlMouseUp(object sender, MouseButtonEventArgs e) => GLControl.Focus();

        private void OnGLControlMouseLeave(object sender, MouseEventArgs e) => scene?.HandleMouseLeave();

        private void OnGLControlMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point pos = e.GetPosition(GLControl);
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Delta > 0)
                    scene?.Zoom(true, pos.X, pos.Y);
                else
                    scene?.Zoom(false, pos.X, pos.Y);
            }
        }

        private void OnGLControlMouseMove(object sender, MouseEventArgs e)
        {
            Point pos = e.GetPosition(GLControl);
            double[] newMousePos = [pos.X, pos.Y];
            if (oldMousePos.Length == 2)
            {
                double mouseMovementX = newMousePos[0] - oldMousePos[0];
                double mouseMovementY = newMousePos[1] - oldMousePos[1];
                if (e.RightButton == MouseButtonState.Pressed)
                    scene?.MoveCamera(mouseMovementX, -mouseMovementY);
                else if (e.LeftButton == MouseButtonState.Pressed)
                    scene?.MoveMarker(pos.X, pos.Y, mouseMovementX, -mouseMovementY);
            }
            oldMousePos = newMousePos;
        }

        private void OnGLControlKeyDown(object sender, KeyEventArgs e)
        {
            if (GLControl.IsMouseOver && GLControl.IsFocused)
            {
                switch (e.Key)
                {
                    case Key.Escape:
                        e.Handled = true;
                        //clear selection
                        Path?.SelectedIndex = -1;
                        break;
                    case Key.D:
                        {
                            e.Handled = true;
                            //delete selected marker
                            int? index = Path?.SelectedIndex;
                            if (index != null && index >= 0 && index < Path?.Nodes.Count)
                                Path?.RemoveNode(index.GetValueOrDefault());
                            break;
                        }
                    case Key.Delete:
                        e.Handled = true;
                        if (Path?.Nodes.Count > 0)
                        {
                            string message = "Are you sure you want to clear the current path?";
                            ConfirmActionWindow caw = new ConfirmActionWindow(message);
                            if (caw.ShowDialog().GetValueOrDefault())
                            {
                                MapManagerService.ChangePathAndClearHistory(createNewPath());
                                AutosavePath();
                            }
                        }
                        break;
                    case Key.Z:
                        if (Keyboard.Modifiers == ModifierKeys.Control)
                        {
                            e.Handled = true;
                            MapManagerService.UndoPathState();
                        }
                        break;
                    case Key.Y:
                        if (Keyboard.Modifiers == ModifierKeys.Control)
                        {
                            e.Handled = true;
                            MapManagerService.RedoPathState();
                        }
                        break;
                }
            }
        }

        private void OnGLControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(GLControl);
            if (Keyboard.IsKeyDown(Key.A))
            {
                scene?.AddMarker(pos.X, pos.Y);
                return;
            }
            scene?.HandleClick(pos.X, pos.Y);
        }

        private void AutosavePath()
        {
            //save path to file if autosave is enabled
            if (Settings.GetOrDefault<bool>("IsPathAutosaveEnabled"))
            {
                string? saveLocation = Settings.GetOrDefault<string>("SaveLocation");
                string fileName = "__autosave_path__.info";
                PathFileDefinition pathFileDefinition = new()
                {
                    Name = "_autosave_",
                    ImageMD5 = MapManagerService.MapInfo?.ImageMD5,
                    Path = this.Path
                };
                string jsonString = JsonSerializer.Serialize(pathFileDefinition, new JsonSerializerOptions()
                {
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    WriteIndented = true
                });
                Directory.CreateDirectory($"{saveLocation}\\maps\\{pathFileDefinition.ImageMD5}\\autosave");
                try
                {
                    File.WriteAllText($"{saveLocation}\\maps\\{pathFileDefinition.ImageMD5}\\autosave\\{fileName}", jsonString);
                }
                catch { }
            }
        }
        #endregion
    }
}
