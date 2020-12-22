using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Uwp.Extensions;
using Microsoft.Toolkit.Uwp.Helpers;
using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.Services;
using ProjectCodeEditor.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor.ViewModels
{
    public sealed class ShellViewModel : ObservableObject
    {
        public ObservableCollection<ShellView> Instances = new();

        private int _SelectedIndex;

        public int SelectedIndex
        {
            get => _SelectedIndex;
            set
            {
                SetProperty(ref _SelectedIndex, value);
                OnPropertyChanged(nameof(CurrentContent));
            }
        }

        public UIElement CurrentContent => Instances[SelectedIndex].Content;

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

        public void AddLayout(ShellView view, bool multiple = false)
        {
            Instances.Add(view);
            if (!multiple) SelectedIndex = Instances.Count - 1;
        }

        public ShellViewModel()
        {
            AddLayout(new("HubTitle".GetLocalized(), "HubCaption".GetLocalized(), new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Home }, new HomePage()));
            AddLayout(new("SettingsHub/Header".GetLocalized(), "SettingsCaption".GetLocalized(), new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Setting }, new SettingsPage()), true);
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

        public void CloseInstance(ShellView view)
        {
            if (view.CanClose) (view.Content as IDisposable).Dispose();
        }

        internal void RemoveInstance(ShellView e)
        {
            if (Instances.IndexOf(e) == SelectedIndex) SelectedIndex = 0;
            Instances.Remove(e);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
