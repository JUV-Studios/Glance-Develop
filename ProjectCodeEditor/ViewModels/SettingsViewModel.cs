using Microsoft.Graphics.Canvas.Text;
using Microsoft.Toolkit.Uwp.Extensions;
using MyToolkit.Storage;
using ProjectCodeEditor.Helpers;
using ProjectCodeEditor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace ProjectCodeEditor.ViewModels
{
    // Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/release/docs/UWP/pages/settings.md
    public class SettingsViewModel : Observable
    {
        private string _versionDescription;

        public string VersionDescription
        {
            get { return _versionDescription; }

            set { Set(ref _versionDescription, value); }
        }

        private bool _AutoSave = false;

        public bool AutoSave
        {
            get => _AutoSave;
            set
            {
                Set(ref _AutoSave, value);
                ApplicationSettings.SetSetting(nameof(AutoSave), value, false, true);
            }
        }

        private bool _TextModeBrowser = false;

        public bool TextModeBrowser
        {
            get => _TextModeBrowser;
            set
            {
                Set(ref _TextModeBrowser, value);
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

        private List<string> _FontList = new List<string>();

        public ReadOnlyCollection<string> FontList;

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

        public SettingsViewModel()
        {
            FontList = new ReadOnlyCollection<string>(_FontList);
        }

        public async Task InitializeAsync()
        {
            VersionDescription = GetVersionDescription();
            foreach (var fontName in CanvasTextFormat.GetSystemFontFamilies()) { _FontList.Add(fontName); }
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
