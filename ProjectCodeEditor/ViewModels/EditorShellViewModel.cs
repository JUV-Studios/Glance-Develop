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
        public event EventHandler<ShellView> FrameCreated;

        public event EventHandler<ShellView> FrameChanged;

        public event EventHandler FrameNavigationCompleted;

        public event EventHandler<ShellView> FrameClosedRequested;

        public void InvokeFrameNavigationCompleted(object sender)
        {
            FrameNavigationCompleted?.Invoke(sender, null);
        }

        public void AddWebPage(string uriString = null)
        {
            bool contains = false;
            int index = 0;
            for (int i = 0; i < Instances.Count; i++)
            {
                if (Instances[i].Title == "TabBrowserDisplayName".GetLocalized())
                {
                    contains = true;
                    index = i;
                    break;
                }
            }

            if (contains)
            {
                var item = Instances[index];
                item.Parameter = uriString;
                SelectedItem = Instances[index];
            }
            else
            {
                var item = new ShellView()
                {
                    Title = "TabBrowserDisplayName".GetLocalized(),
                    Caption = "TabBrowserCaption".GetLocalized(),
                    Content = new BrowserPage(),
                    Parameter = uriString
                };

                FrameCreated?.Invoke(null, item);

                Instances.Add(item);

                SelectedItem = Instances.Last();
            }
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
            }
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
                if (Instances.IndexOf(SelectedItem) == 0) return false;
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

        public EditorShellViewModel()
        {
            Frame frame = new Frame()
            {
                IsNavigationStackEnabled = false
            };

            frame.Navigate(typeof(MainPage));

            Instances.Add(new ShellView()
            {
                Title = "HubTitle".GetLocalized(),
                Caption = "HubCaption".GetLocalized(),
                Content = frame
            });

            SelectedItem = Instances.Last();
        }
    }
}
