using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace WoTMapWPF
{
    internal static class Settings
    {
        private readonly static App app;

        static Settings()
        {
            app = (App)(App.Current);
        }

        public static void Set(string settingName, object value)
        {
            app.ChangeUserSetting(settingName, value);
        }

        public static object Get(string settingName)
        {
            return app.SettingsDictionary.MergedDictionaries[0][settingName];
        }

        public static bool Exists(string settingName)
        {
            ResourceDictionary dictionary = app.SettingsDictionary.MergedDictionaries[0];
            return dictionary.Contains(settingName);
        }

        public static T Get<T>(string settingName)
        {
            ResourceDictionary dictionary = app.SettingsDictionary.MergedDictionaries[0];
            return (T)app.SettingsDictionary.MergedDictionaries[0][settingName];
        }

        /// <summary>
        /// Return the value if the setting if it is defined in the resource dictionary, returns default value otherwise.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName"></param>
        /// <returns></returns>
        public static T? GetOrDefault<T>(string settingName)
        {
            ResourceDictionary dictionary = app.SettingsDictionary.MergedDictionaries[0];
            if(dictionary.Contains(settingName))
                return (T)app.SettingsDictionary.MergedDictionaries[0][settingName];
            else
                return default;
        }

        public static void RestoreDefault()
        {
            app.RestoreDefaultSettings();
        }

        public static void Save()
        {
            app.SaveSettings();
        }
    }
}
