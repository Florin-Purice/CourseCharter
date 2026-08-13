using OpenTK.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WoTMapWPF.CustomControls;
using WoTMapWPF.Graphics;
using Timer = System.Timers.Timer;
using Window = System.Windows.Window;

namespace WoTMapWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            DataContext = ViewModel = viewModel;
            InitializeComponent();
        }

        public MainWindowViewModel ViewModel { get; }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App app = (App)App.Current;
            //save window position/size
            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                Settings.Set("WindowTop", RestoreBounds.Top);
                Settings.Set("WindowLeft", RestoreBounds.Left);
                Settings.Set("WindowHeight", RestoreBounds.Height);
                Settings.Set("WindowWidth", RestoreBounds.Width);
                Settings.Set("IsWindowMaximized", true);
            }
            else
            {
                Settings.Set("WindowTop", this.Top);
                Settings.Set("WindowLeft", this.Left);
                Settings.Set("WindowHeight", this.Height);
                Settings.Set("WindowWidth", this.Width);
                Settings.Set("IsWindowMaximized", false);
            }
            //save currently opened map
            if (ViewModel.MapManagerService.MapInfo != null)
            {
                Settings.Set("LastOpenedMapName", ViewModel.MapManagerService.MapInfo.Name);
                Settings.Set("LastOpenedMapImageMD5", ViewModel.MapManagerService.MapInfo.ImageMD5);
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            //window position/size
            if (Settings.Exists("WindowTop"))
            {
                this.Top = Settings.Get<double>("WindowTop");
            }
            if (Settings.Exists("WindowLeft"))
            {
                this.Left = Settings.Get<double>("WindowLeft");
            }
            if (Settings.Exists("WindowHeight"))
            {
                this.Height = Settings.Get<double>("WindowHeight");
            }
            if (Settings.Exists("WindowWidth"))
            {
                this.Width = Settings.Get<double>("WindowWidth");
            }
            if (Settings.Exists("IsWindowMaximized") && Settings.Get<bool>("IsWindowMaximized"))
                WindowState = WindowState.Maximized;
            //load last opened map
            if (Settings.Exists("LastOpenedMapName") && Settings.Exists("LastOpenedMapImageMD5"))
            {
                string mapName = Settings.Get<string>("LastOpenedMapName");
                string mapImageMD5 = Settings.Get<string>("LastOpenedMapImageMD5");
                string saveLocation = Settings.Get<string>("SaveLocation");
                string mapPath = $"{saveLocation}\\maps\\{mapImageMD5}\\{mapName}.info";
                if (File.Exists(mapPath))
                    try
                    {
                        string jsonString = File.ReadAllText(mapPath);
                        MapFileDefinition? map = JsonSerializer.Deserialize<MapFileDefinition>(jsonString, new JsonSerializerOptions()
                        {
                            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                            WriteIndented = true
                        });
                        if (map != null)
                            if (ViewModel.MapManagerService.LoadMap(map))
                            {
                                //only reason to call this method here is for updating title with correct map name
                                //ShowDefaultPanel();
                            }
                    }
                    catch { }
            }
            //load user selected theme
            if (Settings.Exists("ThemeName"))
            {
                string themeName = Settings.Get<string>("ThemeName");
                ((App)App.Current).ChangeTheme(themeName);
            }
        }
    }
}