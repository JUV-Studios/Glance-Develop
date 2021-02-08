using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.Storage;
using System;
using Windows.ApplicationModel;

namespace DevelopManaged
{
    public sealed class SettingsViewModel : INotifyPropertyChanged
    {
        static internal SettingsViewModel Instance = null;

        public SettingsViewModel()
        {
            if (Instance == null) Instance = this;
            else throw new InvalidOperationException();
        }

        public static ApplicationDataContainer LocalSettings => ApplicationData.Current.LocalSettings;

        private static T GetSetting<T>(string key, T fallback)
        {
            if (LocalSettings.Values.ContainsKey(key)) return (T)LocalSettings.Values[key];
            else return fallback;
        }

        private static void SetSetting(string key, object value) => LocalSettings.Values[key] = value;

        public static void ResetSettings() => LocalSettings.Values.Clear();

        public string FontFamily
        {
            get => GetSetting(nameof(FontFamily), "Consolas");
            set
            {
                if (FontFamily != value)
                {
                    SetSetting(nameof(FontFamily), value);
                    PropertyChanged?.Invoke(this, new(nameof(FontFamily)));
                }
            }
        }

        public int TabSize
        {
            get => GetSetting(nameof(TabSize), 4);
            set
            {
                if (TabSize != value)
                {
                    SetSetting(nameof(TabSize), value);
                    PropertyChanged?.Invoke(this, new(nameof(TabSize)));
                }
            }
        }

        public uint FontSize
        {
            get => GetSetting(nameof(FontSize), 18u);
            set
            {
                if (FontSize != value)
                {
                    SetSetting(nameof(FontSize), value);
                    PropertyChanged?.Invoke(this, new(nameof(FontSize)));
                }
            }
        }

        public bool AutoSave
        {
            get => GetSetting(nameof(AutoSave), false);
            set
            {
                if (AutoSave != value)
                {
                    SetSetting(nameof(AutoSave), value);
                    PropertyChanged?.Invoke(this, new(nameof(AutoSave)));
                }
            }
        }


        public bool DisableSound
        {
            get => GetSetting(nameof(DisableSound), false);
            set
            {
                if (DisableSound != value)
                {
                    SetSetting(nameof(DisableSound), value);
                    PropertyChanged?.Invoke(this, new(nameof(DisableSound)));
                    ElementSoundPlayer.State = !value ? ElementSoundPlayerState.On : ElementSoundPlayerState.Off;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static string AboutText
        {
            get
            {
                var packageVersion = Package.Current.Id.Version;
                string versionString = $"{"VersionText".GetLocalized()} {packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}.{packageVersion.Revision}";
                return $"Develop\r{versionString}\r{"DevelopedBlock/Text".GetLocalized()}\r{ "CopyrightBlock/Text".GetLocalized()}";
            }
        }
    }
}
