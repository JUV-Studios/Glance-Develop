using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.Services;
using ProjectCodeEditor.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor.ViewModels
{
    public sealed class ShellViewModel : ObservableObject
    {
        public ObservableCollection<ShellView> Instances = new();

        private ShellView _SelectedItem;

        public ShellView SelectedItem
        {
            get => _SelectedItem;
            set
            {
                SetProperty(ref _SelectedItem, value);
                OnPropertyChanged(nameof(IsCurrentClosable));
            }
        }

        private SymbolIconSource _FullScreenBtnSrc;

        public SymbolIconSource FSIconSource
        {
            get => _FullScreenBtnSrc;
            set => SetProperty(ref _FullScreenBtnSrc, value);
        }

        private string _FullScreenBtnLabel;

        public string FSLabel
        {
            get => _FullScreenBtnLabel;
            set => SetProperty(ref _FullScreenBtnLabel, value);
        }

        private string _CompactOverlayBtnLabel;

        public string CompactOverlayBtnLabel
        {
            get => _CompactOverlayBtnLabel;
            set => SetProperty(ref _CompactOverlayBtnLabel, value);
        }

        public bool IsCurrentClosable => _SelectedItem.Content is IDisposable;

        public void AddLayout(ShellView view, bool multiple = false)
        {
            Instances.Add(view);
            if (!multiple) SelectedItem = Instances.Last();
        }

        public ShellViewModel()
        {
            AddLayout(new()
            {
                Title = "HubTitle".GetLocalized(),
                Caption = "HubCaption".GetLocalized(),
                Content = new HomePage()
            });

            AddLayout(new()
            {
                Title = "SettingsTitle".GetLocalized(),
                Caption = "SettingsCaption".GetLocalized(),
                Content = new SettingsPage(),
            }, true);

            ViewService.KeyShortcutPressed += ViewService_KeyShortcutPressed;
            ViewService.ViewModeChanged += ViewService_ViewModeChanged;
            ElementSoundPlayer.State = !Singleton<SettingsViewModel>.Instance.DisableSound ? ElementSoundPlayerState.On : ElementSoundPlayerState.Off;
            ViewService.RaiseViewModeChanged();
        }

        private void ViewService_ViewModeChanged(object sender, AppViewMode e)
        {
            if (e == AppViewMode.FullScreen)
            {
                FSIconSource = new SymbolIconSource() { Symbol = Symbol.BackToWindow };
                FSLabel = "FullScreenOptionOnText".GetLocalized();
            }
            else
            {
                FSIconSource = new SymbolIconSource() { Symbol = Symbol.FullScreen };
                FSLabel = "FullScreenOptionOffText".GetLocalized();
            }

            if (e == AppViewMode.CompactOverlay) CompactOverlayBtnLabel = "PipOptionOnText".GetLocalized();
            else CompactOverlayBtnLabel = "PipOptionOffText".GetLocalized();
        }

        private void ViewService_KeyShortcutPressed(object sender, KeyShortcutPressedEventArgs e)
        {
            if (e.Accelerator.Modifiers == VirtualKeyModifiers.Control && e.Accelerator.Key == VirtualKey.Tab && e.Accelerator.IsEnabled)
            {
                e.SystemArgs.Handled = true;
                FastSwitch();
            }
        }

        public void CloseCurrentInstance() => (_SelectedItem.Content as IDisposable).Dispose();

        internal void RemoveInstance(ShellView e)
        {
            if (_SelectedItem == e)
            {
                try
                {
                    var setting = Instances[Instances.IndexOf(e) - 1];
                    if (setting.Content is SettingsPage) SelectedItem = Instances.First();
                    else SelectedItem = setting;
                }
                catch (ArgumentOutOfRangeException)
                {

                }
            }

            Instances.Remove(e);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void FastSwitch()
        {
            var index = Instances.IndexOf(SelectedItem);
            if (Instances.Count != 1)
            {
                if (index == Instances.Count - 1) SelectedItem = Instances.First();
                else SelectedItem = Instances[index + 1];
            }
        }
    }
}
