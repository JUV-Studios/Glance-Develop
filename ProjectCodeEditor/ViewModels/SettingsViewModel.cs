using Microsoft.Graphics.Canvas.Text;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Uwp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;

namespace ProjectCodeEditor.ViewModels
{
    public sealed record Dependency(string DependencyName, Uri ProjectUri);

    public sealed class SettingsViewModel : INotifyPropertyChanged
    {
        private string[] _SupportedFileTypes = null;

        public string[] SupportedFileTypes
        {
            get
            {
                if (_SupportedFileTypes == null) _SupportedFileTypes = File.ReadAllLines(Path.Combine(Package.Current.InstalledPath, "Assets", "FileTypes"));
                return _SupportedFileTypes;
            }
        }

        public readonly string[] InstalledFonts = CanvasTextFormat.GetSystemFontFamilies();

        public ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        public T GetSetting<T>(string key, T fallback)
        {
            if (!LocalSettings.Values.ContainsKey(key)) return fallback;
            else return (T)LocalSettings.Values[key];
        }

        public void SetSetting(string key, object value)
        {
            LocalSettings.Values[key] = value;
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

        public bool ExtendGoTo
        {
            get => GetSetting(nameof(ExtendGoTo), true);
            set
            {
                if (ExtendGoTo != value)
                {
                    SetSetting(nameof(ExtendGoTo), value);
                    PropertyChanged?.Invoke(this, new(nameof(ExtendGoTo)));
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

        public readonly IEnumerable<Dependency> AppDependencies = new Dependency[]
        {
            new("Windows UI Library", new Uri("https://aka.ms/winui")), new("Win2D", new Uri("http://microsoft.github.io/Win2D/html/Introduction.htm")),
            new("WinRTXamlToolkit", new Uri("https://github.com/xyzzer/WinRTXamlToolkit")), new("XAML Behaviors", new Uri("http://go.microsoft.com/fwlink/?LinkID=651678")),
            new("Visual Studio App Center", new Uri("https://azure.microsoft.com/en-us/services/app-center/")), new("UTF.Unknown", new Uri("https://github.com/CharsetDetector/UTF-unknown")),
            new("Windows Community Toolkit", new Uri("https://github.com/windows-toolkit/WindowsCommunityToolkit")), new("Humanizer", new Uri("https://github.com/Humanizr/Humanizer")),
            new("SwordfishCollections", new Uri("https://github.com/stewienj/SwordfishCollections")), 
            new("ColorCode", new Uri("https://github.com/windows-toolkit/ColorCode-Universal")), new("IronPython", new Uri("https://github.com/IronLanguages/ironpython2"))
        };

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
