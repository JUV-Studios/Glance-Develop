using ProjectCodeEditor.ViewModels;
using System.IO;
using Windows.ApplicationModel;
using Windows.Storage;

namespace ProjectCodeEditor.Services
{
    public static class Preferences
    {
        public static readonly string[] SupportedFileTypes = File.ReadAllLines(Path.Combine(Package.Current.InstalledPath, "Assets\\FileTypes"));

        public static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        public static readonly SettingsViewModel AppSettings = new();

        public static readonly ShellViewModel AppShellViewModel = new();

        public static T GetSetting<T>(string key, T fallback)
        {
            if (LocalSettings.Values.ContainsKey(key)) return (T)LocalSettings.Values[key];
            else return fallback;
        }

        public static void SetSetting(string key, object value) => LocalSettings.Values[key] = value;

        public static void ResetSettings() => LocalSettings.Values.Clear();
    }
}
