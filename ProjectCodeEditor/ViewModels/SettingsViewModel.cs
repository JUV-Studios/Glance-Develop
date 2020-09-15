using Microsoft.Graphics.Canvas.Text;
using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.Helpers;
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
                SettingsStorageExtensions.LocalSettings.Values[nameof(AutoSave)] = value;
            }
        }

        private string _EditorFont;

        public string EditorFont
        {
            get => _EditorFont;
            set
            {
                Set(ref _EditorFont, value);
                SettingsStorageExtensions.LocalSettings.SaveString(nameof(EditorFont), value);
            }
        }

        private List<string> _FontList = new List<string>();

        public ReadOnlyCollection<string> FontList;

        public SettingsViewModel()
        {
            FontList = new ReadOnlyCollection<string>(_FontList);
        }

        public async Task InitializeAsync()
        {
            VersionDescription = GetVersionDescription();
            foreach (var fontName in CanvasTextFormat.GetSystemFontFamilies()) { _FontList.Add(fontName); }
            EditorFont = SettingsStorageExtensions.LocalSettings.GetString(nameof(EditorFont)) ?? "Segoe UI";
            var autoSaveValue = SettingsStorageExtensions.LocalSettings.Values[nameof(AutoSave)];
            if (autoSaveValue == null || autoSaveValue is not bool) AutoSave = false;
            else AutoSave = Convert.ToBoolean(autoSaveValue);
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
