using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.Services;
using ProjectCodeEditor.Views;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MUXC = Microsoft.UI.Xaml.Controls;

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
                try
                {
                    SetProperty(ref _SelectedIndex, value);
                    OnPropertyChanged(nameof(CurrentContent));
                }
                catch (ArgumentException) { }
            }
        }

        public UIElement CurrentContent
        {
            get
            {
                if (_SelectedIndex == -1 || _SelectedIndex >= Instances.Count)
                {
                    SetProperty(ref _SelectedIndex, 0, nameof(SelectedIndex));
                    return CurrentContent;
                }
                else return Instances[_SelectedIndex].Content;
            }
        }

        public void AddLayout(ShellView view, bool multiple = false)
        {
            Instances.Add(view);
            if (!multiple) SelectedIndex = Instances.Count - 1;
        }

        public ShellViewModel()
        {
            AddLayout(new("HubTitle".GetLocalized(), "HubCaption".GetLocalized(), new MUXC.SymbolIconSource() { Symbol = Symbol.Home }, new HomePage()));
            ElementSoundPlayer.State = !Singleton<SettingsViewModel>.Instance.DisableSound ? ElementSoundPlayerState.On : ElementSoundPlayerState.Off;
        }

        public void CloseInstance(ShellView view)
        {
            if (view.CanClose) (view.Content as IDisposable).Dispose();
        }

        internal void RemoveInstance(ShellView e)
        {
            var index = Instances.IndexOf(e);
            if (index == SelectedIndex) SelectedIndex = index - 1;
            Instances.Remove(e);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
