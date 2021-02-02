using Microsoft.Toolkit.Uwp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Data.Json;
using Windows.System;
using Windows.UI.Xaml;
using static ProjectCodeEditor.Services.Preferences;

namespace ProjectCodeEditor.ViewModels
{
    internal sealed record Dependency(string DependencyName, Uri ProjectUri)
    {
        internal static Dependency FromJson(JsonObject obj) => new Dependency(obj.GetNamedString("Name"), new Uri(obj.GetNamedString("Url")));
    }

    public sealed class SettingsViewModel : INotifyPropertyChanged
    {
        public SettingsViewModel()
        {
            var dependencies = File.ReadAllText(Path.Combine(Package.Current.InstalledPath, "Assets", "Dependencies.json"));
            var dependenciesRoot = JsonObject.Parse(dependencies).GetNamedArray("dependencies");
            AppDependencies.AddRange(dependenciesRoot.Select(item => Dependency.FromJson(item.GetObject())));
        }

        public async Task<string> UniqueUserIdAsync()
        {
            string accountId = GetSetting("UserId", string.Empty);
            if (accountId == string.Empty)
            {
                if (App.CurrentUser != null)
                {
                    accountId = (await App.CurrentUser.GetPropertyAsync(KnownUserProperties.AccountName)).ToString();
                    if (string.IsNullOrEmpty(accountId)) accountId = (await App.CurrentUser.GetPropertyAsync(KnownUserProperties.FirstName)).ToString();
                }
                // This user wants some privacy. Let's not try to enumerate users which will need capability
                else accountId = Guid.NewGuid().ToString();
                SetSetting("UserId", accountId);
            }

            return accountId;
        }

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

        internal bool DialogShown = false;

        internal List<Dependency> AppDependencies = new List<Dependency>();

        public event PropertyChangedEventHandler PropertyChanged;

        public string AboutText
        {
            get
            {
                var packageVersion = Package.Current.Id.Version;
                string versionString = $"{"VersionText".GetLocalized()} {packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}.{packageVersion.Revision}";
                return $"Develop\r{versionString}\r{"CopyrightBlock/Text".GetLocalized()}\r{"DevelopedBlock/Text".GetLocalized()}";
            }
        }

        public string AboutTextForAutomation => AboutText.Replace("\r", ", ");
    }
}
