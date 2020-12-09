using Microsoft.Graphics.Canvas.Text;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Uwp.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;

namespace ProjectCodeEditor.ViewModels
{
    public sealed record Dependency(string DependencyName, Uri ProjectUri);

    public sealed class SettingsViewModel : ObservableObject
    {
        public readonly string[] SupportedFileTypes = File.ReadAllLines(Path.Combine(Package.Current.InstalledPath, "Assets", "FileTypes"));

        public readonly string[] InstalledFonts = CanvasTextFormat.GetSystemFontFamilies();

        private readonly ApplicationDataContainer SettingsContainer = ApplicationData.Current.LocalSettings;

        public async Task<string> UniqueUserId()
        {
            string accountId;
            if (SettingsContainer.Values.ContainsKey("UserId")) accountId = SettingsContainer.Values["UserId"].ToString();
            else
            {
                if (App.CurrentUser != null)
                {
                    accountId = (await App.CurrentUser.GetPropertyAsync(KnownUserProperties.AccountName)).ToString();
                    if (string.IsNullOrEmpty(accountId)) accountId = (await App.CurrentUser.GetPropertyAsync(KnownUserProperties.FirstName)).ToString();
                }
                // This user wants some privacy. Let's not try to enumerate users which will need capability
                else accountId = Guid.NewGuid().ToString();
                SettingsContainer.Values["UserId"] = accountId;
            }

            return accountId;
        }

        public string FontFamily
        {
            get
            {
                if (SettingsContainer.Values.ContainsKey(nameof(FontFamily))) return SettingsContainer.Values[nameof(FontFamily)].ToString();
                else return "Consolas";
            }
            set
            {
                if (FontFamily != value)
                {
                    SettingsContainer.Values[nameof(FontFamily)] = value;
                    OnPropertyChanged(nameof(FontFamily));
                }
            }
        }

        public int TabSize
        {
            get
            {
                if (SettingsContainer.Values.ContainsKey(nameof(TabSize))) return Convert.ToInt32(SettingsContainer.Values[nameof(TabSize)]);
                else return 4;
            }
            set
            {
                if (TabSize != value)
                {
                    SettingsContainer.Values[nameof(TabSize)] = value;
                    OnPropertyChanged(nameof(TabSize));
                }
            }
        }

        public uint FontSize
        {
            get
            {
                if (SettingsContainer.Values.ContainsKey(nameof(FontSize))) return Convert.ToUInt32(SettingsContainer.Values[nameof(FontSize)]);
                else return 18;
            }
            set
            {
                if (TabSize != value)
                {
                    SettingsContainer.Values[nameof(FontSize)] = value;
                    OnPropertyChanged(nameof(FontSize));
                }
            }
        }

        public bool AutoSave
        {
            get
            {
                if (SettingsContainer.Values.ContainsKey(nameof(AutoSave))) return Convert.ToBoolean(SettingsContainer.Values[nameof(AutoSave)]);
                else return false;
            }
            set
            {
                if (AutoSave != value)
                {
                    SettingsContainer.Values[nameof(AutoSave)] = value;
                    OnPropertyChanged(nameof(AutoSave));
                }
            }
        }

        public bool DisableSound
        {
            get
            {
                bool value;
                if (SettingsContainer.Values.ContainsKey(nameof(DisableSound))) value = Convert.ToBoolean(SettingsContainer.Values[nameof(DisableSound)]);
                else value = false;
                return value;
            }
            set
            {
                if (DisableSound != value)
                {
                    SettingsContainer.Values[nameof(DisableSound)] = value;
                    OnPropertyChanged(nameof(DisableSound));
                    ElementSoundPlayer.State = !value ? ElementSoundPlayerState.On : ElementSoundPlayerState.Off;
                }
            }
        }

        public bool DialogShown = false;

        public readonly IEnumerable<Dependency> AppDependencies = new Dependency[]
        {
            new("Windows UI Library", new Uri("https://aka.ms/winui")), new("Win2D", new Uri("http://microsoft.github.io/Win2D/html/Introduction.htm")),
            new("WinRTXamlToolkit", new Uri("https://github.com/xyzzer/WinRTXamlToolkit")), new("XAML Behaviors", new Uri("http://go.microsoft.com/fwlink/?LinkID=651678")),
            new("Visual Studio App Center", new Uri("https://azure.microsoft.com/en-us/services/app-center/")),
            new("Windows Community Toolkit", new Uri("https://github.com/windows-toolkit/WindowsCommunityToolkit")), new("TextEncodingDetect", new Uri("https://github.com/AutoItConsulting/text-encoding-detect")),
            new("SwordfishCollections", new Uri("https://github.com/stewienj/SwordfishCollections"))
        };

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
