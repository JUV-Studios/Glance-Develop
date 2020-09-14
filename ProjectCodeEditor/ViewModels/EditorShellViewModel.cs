using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.Helpers;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.Services;
using ProjectCodeEditor.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor.ViewModels
{
    public sealed class EditorShellViewModel : Observable
    {
        public static event EventHandler<ShellView> FrameCreated;

        public static event EventHandler<ShellView> FrameChanged;

        public static event EventHandler<ShellView> FrameNavigationCompleted;

        public static event EventHandler<ShellView> FrameClosedRequested;

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

        public void AddWebPage(string uriString = "about:blank")
        {
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
            /* bool contains = false;
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
                Frame frame = new Frame()
                {
                    IsNavigationStackEnabled = false
                };

                frame.Navigate(typeof(EditorPage));

                var item = new ShellView()
                {
                    Title = file.Name,
                    Caption = file.Path,
                    Content = frame,
                    Parameter = file
                };

                FrameCreated?.Invoke(null, item);

                Instances.Add(item);


                if (!multiple)
                {
                    SelectedItem = Instances.Last();
                }
            } */
        }

        public void RemoveSelectedItem()
        {
            var index = Instances.IndexOf(SelectedItem);
            FrameClosedRequested?.Invoke(null, SelectedItem);
        }


        public bool CanCloseSelectedItem
        {
            get
            {
                if (SelectedItem.Caption == "HubCaption") return false;
                else return true;
            }
            set
            {
                OnPropertyChanged(nameof(CanCloseSelectedItem));
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
                CanCloseSelectedItem = false;
            }
        }


        public ObservableCollection<ShellView> Instances = new ObservableCollection<ShellView>();


        private void Load()
        {
            AddLayout(new ShellView()
            {
                Title = "HubTitle".GetLocalized(),
                Caption = "HubCaption".GetLocalized(),
                Content = new MainPage()
            });
        }

        public EditorShellViewModel()
        {
            Load();
        }
    }
}
