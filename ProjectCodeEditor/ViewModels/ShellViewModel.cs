using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.Services;
using ProjectCodeEditor.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor.ViewModels
{
    public sealed class ShellViewModel : ObservableObject
    {
        public RangedObservableCollection<ShellView> Instances = new();

        private ShellView _SelectedItem;

        public ShellView SelectedItem
        {
            get => _SelectedItem;
            set => SetProperty(ref _SelectedItem, value);
        }

        public void AddViews(IReadOnlyList<ShellView> views)
        {
            if (views.Count == 0) return;
            Instances.AddRange(views);
            SelectedItem = Instances.Last();
        }

        public ShellViewModel()
        {
            AddViews(new ShellView[] { new ShellView("HubTitle".GetLocalized(), new SymbolIconSource() { Symbol = Symbol.Home }, new StartPage(), null) });
            ElementSoundPlayer.State = Preferences.AppSettings.DisableSound ? ElementSoundPlayerState.On : ElementSoundPlayerState.Off;
        }

        internal void TerminateInstance(ShellView view)
        {
            if (view == null) return;
            if (view == SelectedItem) SelectedItem = Instances.First();
            Instances.Remove(view);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        internal void FastSwitch()
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
