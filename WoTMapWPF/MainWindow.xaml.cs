using OpenTK.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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
                app.ChangeUserSetting("WindowTop", RestoreBounds.Top);
                app.ChangeUserSetting("WindowLeft", RestoreBounds.Left);
                app.ChangeUserSetting("WindowHeight", RestoreBounds.Height);
                app.ChangeUserSetting("WindowWidth", RestoreBounds.Width);
                app.ChangeUserSetting("IsWindowMaximized", true);
            }
            else
            {
                app.ChangeUserSetting("WindowTop", this.Top);
                app.ChangeUserSetting("WindowLeft", this.Left);
                app.ChangeUserSetting("WindowHeight", this.Height);
                app.ChangeUserSetting("WindowWidth", this.Width);
                app.ChangeUserSetting("IsWindowMaximized", false);
            }
            //save currently opened map
            app.ChangeUserSetting("LastOpenedMapName", ViewModel.MapName);
            app.ChangeUserSetting("LastOpenedMapImageMD5", ViewModel.MapImageMD5);
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            //window position/size
            if (App.Current.Resources.Contains("WindowTop"))
            {
                this.Top = (double)App.Current.Resources["WindowTop"];
            }
            if (App.Current.Resources.Contains("WindowLeft"))
            {
                this.Left = (double)App.Current.Resources["WindowLeft"];
            }
            if (App.Current.Resources.Contains("WindowHeight"))
            {
                this.Height = (double)App.Current.Resources["WindowHeight"];
            }
            if (App.Current.Resources.Contains("WindowWidth"))
            {
                this.Width = (double)App.Current.Resources["WindowWidth"];
            }
            if (App.Current.Resources.Contains("IsWindowMaximized") && (bool)App.Current.Resources["IsWindowMaximized"])
                WindowState = WindowState.Maximized;
            //load last opened map
            if (App.Current.Resources.Contains("LastOpenedMapName") && App.Current.Resources.Contains("LastOpenedMapImageMD5"))
            {
                string mapName = (string)App.Current.Resources["LastOpenedMapName"];
                string mapImageMD5 = (string)App.Current.Resources["LastOpenedMapImageMD5"];
                string mapPath = $"{saveLocation}\\maps\\{mapImageMD5}\\{mapName}.info";
                if (File.Exists(mapPath))
                    try
                    {
                        string jsonString = File.ReadAllText(mapPath);
                        MapFileDefinition map = JsonSerializer.Deserialize<MapFileDefinition>(jsonString, jsonSerializerOptions);
                        if (map != null)
                            if (LoadMap(map))
                            {
                                //only reason to call this method here is for updating title with correct map name
                                ShowDefaultPanel();
                            }
                    }
                    catch { }
            }
            //load user selected theme
            if (App.Current.Resources.Contains("ThemeName"))
            {
                string themeName = (string)App.Current.Resources["ThemeName"];
                ((App)App.Current).ChangeTheme(themeName);
            }
        }

        #region PATH
        private void Path_PathChanged(object? sender, EventArgs e)
        {
            StorePathState();
        }

        private void Path_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedIndex":
                    ChangeListViewSelectedNode(ViewModel.Path.SelectedIndex);
                    break;
            }
        }
    }
}
