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
        static EditorShellViewModel() { }
        public static EditorShellViewModel Instance { get; } = new EditorShellViewModel();

        public static event EventHandler<ShellView> FrameChanged;

        public static event EventHandler FrameNavigationCompleted;

        public static event EventHandler<ShellView> FrameClosed;

        public static void InvokeFrameNavigationCompleted(object sender)
        {
            FrameNavigationCompleted?.Invoke(sender, null);
        }

        public static void AddWebPage(string uriString = null)
        {
            EditorShellViewModel instance = Instance;
            bool contains = false;
            int index = 0;
            for (int i = 0; i < instance.Instances.Count; i++)
            {
                if (instance.Instances[i].DisplayName == "TabBrowserDisplayName".GetLocalized())
                {
                    contains = true;
                    index = i;
                    break;
                }
            }

            if (contains)
            {
                instance.SelectedItem = instance.Instances[index];
            }
            else
            {
                Frame frame = new Frame()
                {
                    IsNavigationStackEnabled = false
                };

                frame.Navigate(typeof(BrowserPage));

                instance.Instances.Add(new ShellView()
                {
                    DisplayName = "TabBrowserDisplayName".GetLocalized(),
                    Caption = "TabBrowserCaption".GetLocalized(),
                    Content = frame,
                    Parameter = uriString
                });
            }

            instance.SelectedItem = instance.Instances.Last();

            instance = null;
        }

        public static void AddFile(StorageFile file, EditorShellViewModel instanceToUse = null)
        {
            EditorShellViewModel instance;
            if (instanceToUse == null)
            {
                instance = Instance;
            }
            else
            {
                instance = instanceToUse;
            }

            bool contains = false;
            int index = 0;
            for (int i = 0; i < instance.Instances.Count; i++)
            {
                if (instance.Instances[i].Parameter is StorageFile)
                {
                    if ((instance.Instances[i].Parameter as StorageFile).IsEqual(file))
                    {
                        contains = true;
                        index = i;
                        break;
                    }
                }
            }

            if (contains)
            {
                instance.SelectedItem = instance.Instances[index];
            }
            else
            {
                Frame frame = new Frame()
                {
                    IsNavigationStackEnabled = false
                };

                frame.Navigate(typeof(EditorPage), file);

                instance.Instances.Add(new ShellView()
                {
                    DisplayName = file.Name,
                    Caption = file.Path,
                    Parameter = file,
                    Content = frame
                });
            }

            instance.SelectedItem = instance.Instances.Last();

            instance = null;
        }

        public static void RemoveSelectedItem()
        {
            EditorShellViewModel instance = Instance;
            var index = instance.Instances.IndexOf(instance.SelectedItem);
            FrameClosed?.Invoke(null, instance.SelectedItem);
            instance.Instances.Remove(instance.SelectedItem);
            if (index < instance.Instances.Count)
            {
                instance.SelectedItem = instance.Instances[index];
            }
            else
            {
                instance.SelectedItem = instance.Instances[index - 1];
            }

            instance = null;
            GC.Collect();
        }


        public bool CanCloseSelectedItem
        {
            get
            {
                if (Instances.IndexOf(SelectedItem) == 0) return false;
                else return true;
            }
            set
            { }
        }

        private ShellView _SelectedItem;

        public ShellView SelectedItem
        {
            get => _SelectedItem;
            set
            {
                ViewService.SetTitle(value?.DisplayName);
                Set(ref _SelectedItem, value);
                FrameChanged?.Invoke(this, value);
                OnPropertyChanged(nameof(CanCloseSelectedItem));
            }
        }

        public ObservableCollection<ShellView> Instances = new ObservableCollection<ShellView>();

        private EditorShellViewModel()
        {
            Frame frame = new Frame()
            {
                IsNavigationStackEnabled = false
            };

            frame.Navigate(typeof(MainPage));

            Instances.Add(new ShellView()
            {
                DisplayName = "HubTitle".GetLocalized(),
                Caption = "HubCaption".GetLocalized(),
                Content = frame
            });

            SelectedItem = Instances.Last();
        }
    }
}
