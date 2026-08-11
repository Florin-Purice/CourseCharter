using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTK.Wpf;
using System;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Xml;
using WoTMapWPF.CustomControls;
using WoTMapWPF.Graphics;
using WoTMapWPF.Services;
using WoTMapWPF.Stores;

namespace WoTMapWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider serviceProvider;
        private readonly string settingsFileName = "Settings.xaml";

        [STAThread]
        public static void Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();
            host.Run();

            App app = new();
            app.InitializeComponent();
            app.MainWindow = host.Services.GetRequiredService<MainWindow>();
            app.MainWindow.Visibility = Visibility.Visible;
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
                services.AddSingleton<Scene>(s => new Scene(
                    s.GetRequiredService<GLWpfControl>(),
                    s.GetRequiredService<MapManagerService>()));
                services.AddSingleton<NotificationService>();
                services.AddSingleton<NavigationStore>();
                services.AddSingleton<INavigationService>(CreateMapNavigationService);

                services.AddTransient<NewMapControlViewModel>(s => new NewMapControlViewModel(
                    s.GetRequiredService<NotificationService>(),
                    s.GetRequiredService<MapManagerService>(),
                    CreateMapNavigationService(s)));
                services.AddTransient<LoadMapControlViewModel>(s => new LoadMapControlViewModel(
                    s.GetRequiredService<NotificationService>(),
                    s.GetRequiredService<MapManagerService>(),
                    CreateMapNavigationService(s)));
                services.AddTransient<SavePathControlViewModel>(s => new SavePathControlViewModel(
                    s.GetRequiredService<NotificationService>(),
                    s.GetRequiredService<MapManagerService>(),
                    CreateMapNavigationService(s)));
                services.AddTransient<LoadPathControlViewModel>(s => new LoadPathControlViewModel(
                    s.GetRequiredService<NotificationService>(),
                    s.GetRequiredService<MapManagerService>(),
                    CreateMapNavigationService(s)));
                services.AddSingleton<GuideControlViewModel>();
                services.AddTransient<SettingsControlViewModel>();
                services.AddSingleton<NotificationControlViewModel>(s => new NotificationControlViewModel(
                    s.GetRequiredService<NotificationService>()));
                services.AddSingleton<MainWindowViewModel>(s => new MainWindowViewModel(
                    s.GetRequiredService<NavigationStore>(),
                    s.GetRequiredService<NotificationControlViewModel>(),
                    s.GetRequiredService<NotificationService>(),
                    CreateMapNavigationService(s),
                    CreateNewMapNavigationService(s),
                    CreateLoadMapNavigationService(s),
                    CreateSavePathNavigationService(s),
                    CreateLoadPathNavigationService(s),
                    CreateGuideNavigationService(s),
                    CreateSettingsNavigationService(s)));
                services.AddSingleton<MainWindow>(s => new MainWindow(s.GetRequiredService<MainWindowViewModel>()));
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

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string appTitle = "CourseCharter";
            string saveLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + appTitle;
            Resources["SaveLocation"] = saveLocation;
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

        private static INavigationService CreateMapNavigationService(IServiceProvider provider)
        {
            throw new NotImplementedException();
        }

        private static INavigationService CreateNewMapNavigationService(IServiceProvider provider)
        {
            return new NavigationService<NewMapControlViewModel>(
                provider.GetRequiredService<NavigationStore>(),
                () => provider.GetRequiredService<NewMapControlViewModel>()
            );
        }

        private static INavigationService CreateLoadMapNavigationService(IServiceProvider provider)
        {
            return new NavigationService<LoadMapControlViewModel>(
                provider.GetRequiredService<NavigationStore>(),
                () => provider.GetRequiredService<LoadMapControlViewModel>(),
                (vm) => vm.Maps.Count > 0
            );
        }

        private static INavigationService CreateSavePathNavigationService(IServiceProvider provider)
        {
            return new NavigationService<SavePathControlViewModel>(
                provider.GetRequiredService<NavigationStore>(),
                () => provider.GetRequiredService<SavePathControlViewModel>(),
                (vm) => vm.IsValid
            );
        }

        private static INavigationService CreateLoadPathNavigationService(IServiceProvider provider)
        {
            return new NavigationService<LoadPathControlViewModel>(
                provider.GetRequiredService<NavigationStore>(),
                () => provider.GetRequiredService<LoadPathControlViewModel>(),
                (vm) => vm.Paths.Count > 0
            );
        }

        private static INavigationService CreateGuideNavigationService(IServiceProvider provider)
        {
            return new NavigationService<GuideControlViewModel>(
                provider.GetRequiredService<NavigationStore>(),
                () => provider.GetRequiredService<GuideControlViewModel>()
            );
        }

        private static INavigationService CreateSettingsNavigationService(IServiceProvider provider)
        {
            return new NavigationService<SettingsControlViewModel>(
                provider.GetRequiredService<NavigationStore>(),
                () => provider.GetRequiredService<SettingsControlViewModel>()
            );
        }
    }
}
