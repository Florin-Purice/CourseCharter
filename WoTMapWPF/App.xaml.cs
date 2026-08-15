using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTK.Wpf;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using WoTMapWPF.CustomControls;
using WoTMapWPF.Graphics;
using WoTMapWPF.Services;

namespace WoTMapWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly string settingsFileName = "Settings.xaml";
        private static readonly string appTitle = "CourseCharter";

        [STAThread]
        public static void Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();
            host.Start();
            //make sure GLWpfControl is created first
            GLWpfControl gLControl = host.Services.GetRequiredService<GLWpfControl>();
            GLWpfControlSettings settings = new()
            {
                MajorVersion = 2,
                MinorVersion = 1
            };
            gLControl.Start(settings);

            App app = new();
            app.InitializeComponent();

            string saveLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + appTitle;
            app.Resources["SaveLocation"] = saveLocation;
            app.Resources["AppTitle"] = appTitle;
            app.LoadSettings();

            app.MainWindow = host.Services.GetRequiredService<MainWindow>();
            app.MainWindow.Visibility = Visibility.Visible;
            host.Services.GetRequiredService<INavigationService>().Navigate();
            app.Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureServices(services =>
            {
                services.AddSingleton<MapManagerService>();
                services.AddSingleton<CreateNewPath>(s =>
                    new CreateNewPath(() =>
                        new Path(s.GetRequiredService<MapManagerService>())
                        ));
                services.AddSingleton<GLWpfControl>();
                services.AddSingleton<Scene>();
                services.AddSingleton<NotificationService>();

                services.AddSingleton<NavigationStore>();
                services.AddSingleton<INavigationService>(CreateMapNavigationService);
                services.AddSingleton<INavigationManager>(CreateNavigationManager);

                services.AddSingleton<MapControlViewModel>();
                services.AddTransient<NewMapControlViewModel>();
                services.AddTransient<LoadMapControlViewModel>();
                services.AddTransient<SavePathControlViewModel>();
                services.AddTransient<LoadPathControlViewModel>();
                services.AddSingleton<GuideControlViewModel>();
                services.AddTransient<SettingsControlViewModel>();
                services.AddSingleton<NotificationControlViewModel>();
                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton<MainWindow>();
            });

        public delegate Path CreateNewPath();

        public ResourceDictionary ThemeDictionary
        {
            get { return Resources.MergedDictionaries[0]; }
        }

        public ResourceDictionary SettingsDictionary
        {
            get { return Resources.MergedDictionaries[1]; }
        }

        public void ChangeTheme(string themeFileName)
        {
            string themePath = "Themes/" + themeFileName;
            Uri themeUri = new Uri(themePath, UriKind.RelativeOrAbsolute);
            ThemeDictionary.MergedDictionaries.Clear();
            ThemeDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = themeUri });
        }

        public void RestoreDefaultSettings()
        {
            SettingsDictionary.MergedDictionaries.Clear();
            SettingsDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("DefaultSettings.xaml", UriKind.Relative) });
        }

        public void ChangeUserSetting(string settingName, object value)
        {
            SettingsDictionary.MergedDictionaries[0][settingName] = value;
        }

        public void SaveSettings()
        {
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.Indent = true;
            writerSettings.IndentChars = "\t";
            string saveLocation = (string)Resources["SaveLocation"];
            string settingsFile = $"{saveLocation}\\{settingsFileName}";
            Directory.CreateDirectory(saveLocation);
            using (FileStream stream = File.Create(settingsFile))
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stream, writerSettings))
                {
                    ResourceDictionary resourceDictionary = SettingsDictionary.MergedDictionaries[0];
                    XamlWriter.Save(resourceDictionary, xmlWriter);
                }
            }
        }

        private void LoadSettings()
        {
            string saveLocation = (string)Resources["SaveLocation"];
            string settingsFile = $"{saveLocation}\\{settingsFileName}";
            if (File.Exists(settingsFile))
            {
                using (FileStream stream = File.OpenRead(settingsFile))
                {
                    ResourceDictionary rd = (ResourceDictionary)XamlReader.Load(stream);
                    ResourceDictionary defaultDict = new ResourceDictionary() { Source = new Uri("DefaultSettings.xaml", UriKind.Relative) };
                    foreach (string key in defaultDict.Keys)
                    {
                        if (!rd.Contains(key))
                            rd.Add(key, defaultDict[key]);
                    }
                    //replace default settings dict with user specific settings
                    SettingsDictionary.MergedDictionaries.Clear();
                    SettingsDictionary.MergedDictionaries.Add(rd);
                }
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            SaveSettings();
        }

        private static INavigationManager CreateNavigationManager(IServiceProvider provider)
        {
            NavigationManager navManager = new NavigationManager();
            navManager.Register(NavigationTarget.MapPanel, CreateMapNavigationService(provider));
            navManager.Register(NavigationTarget.NewMapPanel, CreateNewMapNavigationService(provider));
            navManager.Register(NavigationTarget.LoadMapPanel, CreateLoadMapNavigationService(provider));
            navManager.Register(NavigationTarget.SavePathPanel, CreateSavePathNavigationService(provider));
            navManager.Register(NavigationTarget.LoadPathPanel, CreateLoadPathNavigationService(provider));
            navManager.Register(NavigationTarget.GuidePanel, CreateGuideNavigationService(provider));
            navManager.Register(NavigationTarget.SettingsPanel, CreateSettingsNavigationService(provider));
            return navManager;
        }

        private static INavigationService CreateMapNavigationService(IServiceProvider provider)
        {
            return new MapNavigationService(
                provider.GetRequiredService<NavigationStore>(),
                () => provider.GetRequiredService<MapControlViewModel>(),
                provider.GetRequiredService<MapManagerService>());
        }

        private static INavigationService CreateNewMapNavigationService(IServiceProvider provider)
        {
            string windowTitle = $"{appTitle} - New Map";
            return new NavigationService<NewMapControlViewModel>(
                provider.GetRequiredService<NavigationStore>(),
                () => provider.GetRequiredService<NewMapControlViewModel>(),
                windowTitle);
        }

        private static INavigationService CreateLoadMapNavigationService(IServiceProvider provider)
        {
            string windowTitle = $"{appTitle} - Load Map";
            return new NavigationService<LoadMapControlViewModel>(
                provider.GetRequiredService<NavigationStore>(),
                () => provider.GetRequiredService<LoadMapControlViewModel>(),
                windowTitle,
                (vm) => vm.Maps.Count > 0);
        }

        private static INavigationService CreateSavePathNavigationService(IServiceProvider provider)
        {
            string windowTitle = $"{appTitle} - Save Path";
            return new NavigationService<SavePathControlViewModel>(
                provider.GetRequiredService<NavigationStore>(),
                () => provider.GetRequiredService<SavePathControlViewModel>(),
                windowTitle,
                (vm) => vm.IsValid);
        }

        private static INavigationService CreateLoadPathNavigationService(IServiceProvider provider)
        {
            string windowTitle = $"{appTitle} - Load Path";
            return new NavigationService<LoadPathControlViewModel>(
                provider.GetRequiredService<NavigationStore>(),
                () => provider.GetRequiredService<LoadPathControlViewModel>(),
                windowTitle,
                (vm) => vm.Paths.Count > 0);
        }

        private static INavigationService CreateGuideNavigationService(IServiceProvider provider)
        {
            string windowTitle = $"{appTitle} - Guide";
            return new NavigationService<GuideControlViewModel>(
                provider.GetRequiredService<NavigationStore>(),
                () => provider.GetRequiredService<GuideControlViewModel>(),
                windowTitle);
        }

        private static INavigationService CreateSettingsNavigationService(IServiceProvider provider)
        {
            string windowTitle = $"{appTitle} - Settings";
            return new NavigationService<SettingsControlViewModel>(
                provider.GetRequiredService<NavigationStore>(),
                () => provider.GetRequiredService<SettingsControlViewModel>(),
                windowTitle);
        }
    }
}
