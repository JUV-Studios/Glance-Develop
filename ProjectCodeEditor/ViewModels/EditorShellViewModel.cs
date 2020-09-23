using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.Helpers;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.Services;
using ProjectCodeEditor.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage;

namespace ProjectCodeEditor.ViewModels
{
    public sealed class EditorShellViewModel : Observable
    {
        public void AddLayout(ShellView view, bool multiple = false)
        {
            view.Content.Initialize(view);

            Instances.Add(view);

            if (!multiple) SelectedItem = Instances.Last();

            view.Content.OnTabAdded();
        }

        public void AddWebPage(string uriString = null)
        {
            foreach (var item in Instances)
            {
                if (item.Parameter is string)
                {
                    if (uriString == item.Parameter as string)
                    {
                        SelectedItem = item;
                        return;
                    }
                }
            }

            ILayoutView content;
            if (App.AppSettings.TextModeBrowser) content = new TextModeBrowserPage();
            else content = new BrowserPage();

            AddLayout(new ShellView()
            {
                Title = "TabBrowserDisplayName".GetLocalized(),
                Caption = "TabBrowserCaption".GetLocalized(),
                Content = content,
                Parameter = uriString
            });
        }

        public void AddFile(StorageFile file, bool multiple = false)
        {
            bool contains = false;
            int index = 0;
            for (int i = 0; i < Instances.Count; i++)
            {
                if (Instances[i].Parameter is StorageFile)
                {
                    if ((Instances[i].Parameter as StorageFile).IsEqual(file))
                    {
                        contains = true;
                        index = i;
                        break;
                    }
                }
            }

            if (contains)
            {
                SelectedItem = Instances[index];
            }
            else
            {
                AddLayout(new ShellView()
                {
                    Title = file.Name,
                    Caption = file.Path,
                    Content = new LowLatencyEditorPage(),
                    Parameter = file
                }, multiple);
            }
        }

        public void CloseSelected() => SelectedItem.Content.OnTabRemoveRequested();

        public void TerminateSelected()
        {
            if (CanCloseSelectedItem)
            {
                var index = Instances.IndexOf(SelectedItem);
                var item = SelectedItem;
                Instances.Remove(SelectedItem);
                if (index == 0) SelectedItem = Instances[index];
                else SelectedItem = Instances[index - 1];
                item.Content.Dispose();
            }
        }

        public bool CanCloseSelectedItem
        {
            get
            {
                if (SelectedItem.Caption == "HubCaption".GetLocalized()) return false;
                else return true;
            }
        }

        private ShellView _SelectedItem;

        public ShellView SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem?.Content.SaveState();
                ViewService.SetTitle(value?.Title);
                Set(ref _SelectedItem, value);
                value?.Content.RestoreState();
                OnPropertyChanged(nameof(CanCloseSelectedItem));
            }
        }

        public ObservableCollection<ShellView> Instances = new ObservableCollection<ShellView>();

        public EditorShellViewModel()
        {
            AddLayout(new ShellView()
            {
                Title = "HubTitle".GetLocalized(),
                Caption = "HubCaption".GetLocalized(),
                Content = new MainPage()
            });
        }
    }
}
