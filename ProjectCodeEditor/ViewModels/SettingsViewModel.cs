using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.Helpers;
using ProjectCodeEditor.Services;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ProjectCodeEditor.ViewModels
{
    // Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/release/docs/UWP/pages/settings.md
    public class SettingsViewModel : Observable
    {
        private ElementTheme _elementTheme = ThemeSelectorService.Theme;

        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }

            set { Set(ref _elementTheme, value); }
        }

        private string _versionDescription;

        public string VersionDescription
        {
            get { return _versionDescription; }

            set { Set(ref _versionDescription, value); }
        }

        private ICommand _switchThemeCommand;

        public ICommand SwitchThemeCommand
        {
            get
            {
                if (_switchThemeCommand == null)
                {
                    _switchThemeCommand = new RelayCommand<ElementTheme>(
                        async (param) =>
                        {
                            ElementTheme = param;
                            await ThemeSelectorService.SetThemeAsync(param);
                        });
                }

                return _switchThemeCommand;
            }
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

        private List<string> _FontSizes = new List<string>();

        public ReadOnlyCollection<string> FontSizes;

        public SettingsViewModel()
        {
            FontList = new ReadOnlyCollection<string>(_FontList);
            FontSizes = new ReadOnlyCollection<string>(_FontSizes);
        }

        public async Task InitializeAsync()
        {
            VersionDescription = GetVersionDescription();
            foreach (var fontName in CanvasTextFormat.GetSystemFontFamilies()) { _FontList.Add(fontName); }
            EditorFont = SettingsStorageExtensions.LocalSettings.GetString(nameof(EditorFont)) ?? "Segoe UI";
            var autoSaveValue = SettingsStorageExtensions.LocalSettings.Values[nameof(AutoSave)];
            if (autoSaveValue == null || autoSaveValue.GetType() != typeof(bool)) AutoSave = false;
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
