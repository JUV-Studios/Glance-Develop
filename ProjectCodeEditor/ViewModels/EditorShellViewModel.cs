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
        public static event EventHandler<ShellView> FrameCreated;

        public static event EventHandler<ShellView> FrameChanged;

        public static event EventHandler<ShellView> FrameNavigationCompleted;

        public void InvokeFrameNavigationCompleted(object sender, ShellView e)
        {
            FrameNavigationCompleted?.Invoke(sender, e);
        }

        public void AddLayout(ShellView view, bool multiple = false)
        {
            FrameCreated?.Invoke(this, view);

            Instances.Add(view);

            if (!multiple) SelectedItem = Instances.Last();
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

            AddLayout(new ShellView()
            {
                Title = "TabBrowserDisplayName".GetLocalized(),
                Caption = "TabBrowserCaption".GetLocalized(),
                Content = new BrowserPage(),
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

        public void TerminateSelected()
        {
            if (CanCloseSelectedItem)
            {
                var index = Instances.IndexOf(SelectedItem);
                var item = SelectedItem;
                Instances.Remove(SelectedItem);
                if (index == 0) SelectedItem = Instances[index];
                else SelectedItem = Instances[index - 1];
                item.Content.OnSuspend();
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
                ViewService.SetTitle(value?.Title);
                Set(ref _SelectedItem, value);
                FrameChanged?.Invoke(this, value);
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
