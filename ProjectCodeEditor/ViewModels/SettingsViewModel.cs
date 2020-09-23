using Microsoft.Graphics.Canvas.Text;
using Microsoft.Toolkit.Uwp.Extensions;
using MyToolkit.Storage;
using ProjectCodeEditor.Helpers;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace ProjectCodeEditor.ViewModels
{
    // Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/release/docs/UWP/pages/settings.md
    public class SettingsViewModel : Observable
    {
        private BitArray SmallStorage = new BitArray(2);

        private string _versionDescription;

        public string VersionDescription
        {
            get { return _versionDescription; }

            set { Set(ref _versionDescription, value); }
        }

        public bool AutoSave
        {
            get => SmallStorage.Get(0);
            set
            {
                SmallStorage.Set(0, value);
                OnPropertyChanged(nameof(AutoSave));
                ApplicationSettings.SetSetting(nameof(AutoSave), value, false, true);
            }
        }

        public bool TextModeBrowser
        {
            get => SmallStorage.Get(1);
            set
            {
                SmallStorage.Set(1, value);
                OnPropertyChanged(nameof(TextModeBrowser));
                ApplicationSettings.SetSetting(nameof(TextModeBrowser), value, false, true);
            }
        }

        private string _EditorFont;

        public string EditorFont
        {
            get => _EditorFont;
            set
            {
                Set(ref _EditorFont, value);
                ApplicationSettings.SetSetting(nameof(EditorFont), value, false, true);
            }
        }

        public StringCollection FontList = new StringCollection();

        private EditorFontSize _SelectedFontSize;

        public EditorFontSize SelectedFontSize
        {
            get => _SelectedFontSize;
            set
            {
                Set(ref _SelectedFontSize, value);
                ApplicationSettings.SetSetting(nameof(SelectedFontSize), value.PropertyName, false, true);
            }
        }

        public readonly EditorFontSize[] BindableFontSizes = new EditorFontSize[]
        {
            new EditorFontSize() { FontSizeEnum = EditorFontSizes.Small },
            new EditorFontSize() { FontSizeEnum = EditorFontSizes.Medium },
            new EditorFontSize() { FontSizeEnum = EditorFontSizes.Large }
        };

        public readonly Dependency[] AppDependencies = new Dependency[]
        {
            new Dependency() { DependencyName = "Windows UI Library", ProjectUri = new Uri("https://aka.ms/winui") },
            new Dependency() { DependencyName = "Win2D", ProjectUri = new Uri("http://microsoft.github.io/Win2D/html/Introduction.htm") },
            new Dependency() { DependencyName = "Template10", ProjectUri = new Uri("http://aka.ms/Template10-Nuget") },
            new Dependency() { DependencyName = "WinRTXamlToolkit", ProjectUri = new Uri("https://github.com/xyzzer/WinRTXamlToolkit") },
            new Dependency() { DependencyName = "MyToolkit", ProjectUri = new Uri("http://mytoolkit.io/")},
            new Dependency() { DependencyName = "XAML Behaviors", ProjectUri = new Uri("http://go.microsoft.com/fwlink/?LinkID=651678") },
            new Dependency() { DependencyName = "Monaco Editor UWP", ProjectUri = new Uri("https://github.com/hawkerm/monaco-editor-uwp") },
            new Dependency() { DependencyName = "Visual Studio App Center", ProjectUri = new Uri("https://azure.microsoft.com/en-us/services/app-center/") },
            new Dependency() { DependencyName = "LibGit2Sharp", ProjectUri = new Uri("https://github.com/libgit2/libgit2sharp/") },
            new Dependency() { DependencyName = "Windows Community Toolkit", ProjectUri = new Uri("https://github.com/windows-toolkit/WindowsCommunityToolkit") }
        };

        public SettingsViewModel()
        {
        }

        public async Task InitializeAsync()
        {
            VersionDescription = GetVersionDescription();
            foreach (var fontName in CanvasTextFormat.GetSystemFontFamilies()) { FontList.Add(fontName); }
            EditorFont = ApplicationSettings.GetSetting(nameof(EditorFont), "Segoe UI");
            AutoSave = ApplicationSettings.GetSetting(nameof(AutoSave), false);
            TextModeBrowser = ApplicationSettings.GetSetting(nameof(TextModeBrowser), false);
            var selectedFontSize = ApplicationSettings.GetSetting(nameof(SelectedFontSize), "Medium");
            foreach (var fontSizeName in BindableFontSizes)
            {
                if (selectedFontSize == fontSizeName.PropertyName)
                {
                    _SelectedFontSize = fontSizeName;
                    break;
                }
            }

            await Task.CompletedTask;

        }

        private string GetVersionDescription()
        {
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{"Settings_AboutVersionText".GetLocalized()} {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }
}
