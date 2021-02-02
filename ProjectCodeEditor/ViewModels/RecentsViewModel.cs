using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace ProjectCodeEditor.ViewModels
{
    public sealed class RecentFilesList : ListView
    {
        private bool Registered = false;
        private string LastSearchString = string.Empty;

        public RecentFilesList()
        {
            Singleton<RecentFilesList>.Register(this);
            SetBinding(ItemsSourceProperty, new Binding()
            {
                Source = RecentsViewModel.RecentFiles,
                Mode = BindingMode.OneWay,
            });
        }

        public void Search(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                ItemsSource = RecentsViewModel.RecentFiles;
                return;
            }

            IEnumerable<RecentItem> newSource = null;
            newSource = RecentsViewModel.RecentFiles.Where(item => item.Item.Name.Contains(searchQuery));
            LastSearchString = searchQuery;
            ItemsSource = newSource;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            if (!Registered)
            {
                RecentsViewModel.RecentFiles.CollectionChanged += OriginalItemSource_CollectionChanged;
                Registered = true;
            }

            FrameworkElement source = element as FrameworkElement;
            var context = item as RecentItem;
            ToolTipService.SetToolTip(source, new TextBlock()
            {
                Text = context.Item.Path,
                TextWrapping = TextWrapping.Wrap,
            });
        }

        private void OriginalItemSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ItemsSource != RecentsViewModel.RecentFiles)
            {
                ItemsSource = RecentsViewModel.RecentFiles;
                if (!string.IsNullOrWhiteSpace(LastSearchString)) Search(LastSearchString);
            }
        }
    }

    public static class RecentsViewModel
    {
        private static readonly ApplicationDataContainer TimeContainer = Preferences.LocalSettings.CreateContainer("RecentFileTime", ApplicationDataCreateDisposition.Always);

        private static readonly StorageItemMostRecentlyUsedList RecentsList = StorageApplicationPermissions.MostRecentlyUsedList;

        public static readonly RangedObservableCollection<RecentItem> RecentFiles = new();

        private static bool Loading = false;

        public static async Task LoadRecentsAsync()
        {
            if (!Loading)
            {
                Loading = true;
                RecentFiles.Clear();
                List<RecentItem> filesTemp = null;
                await Task.Run(async () =>
                {
                    filesTemp = new(RecentsList.Entries.Count());
                    foreach (var item in RecentsList.Entries)
                    {
                        try
                        {
                            DateTime time;
                            if (TimeContainer.Values.TryGetValue(item.Token, out object timeOffset)) time = ((DateTimeOffset)timeOffset).DateTime;
                            else time = DateTime.Now;
                            var file = await RecentsList.GetFileAsync(item.Token);
                            filesTemp.Add(new RecentItem()
                            {
                                Item = file,
                                Time = time,
                                Entry = item
                            });
                        }
                        catch (FileNotFoundException)
                        {
                            RecentsList.Remove(item.Token);
                        }
                    }

                    filesTemp.Sort((first, second) => second.Time.CompareTo(first.Time));
                });

                RecentFiles.AddRange(filesTemp);
                Loading = false;
            }
        }

        public static void AddRecentFiles(IReadOnlyList<StorageFile> files)
        {
            bool needsReload = false;
            var currentTime = DateTimeOffset.Now;
            foreach (var file in files)
            {
                var token = General.StorablePathName(file.Path);
                TimeContainer.Values[token] = currentTime;
                if (!RecentsList.ContainsItem(token))
                {
                    RecentsList.AddOrReplace(token, file);
                    RecentFiles.Insert(0, new RecentItem()
                    {
                        Item = file,
                        Time = currentTime.DateTime,
                        Entry = RecentsList.Entries.Where(item => item.Token == token).First(),
                    });
                }
                else needsReload = true;
            }

            if (needsReload) LoadRecentsAsync().ConfigureAwait(false);
        }

        public static void RemoveRecentFile(RecentItem file)
        {
            if (RecentFiles.Contains(file))
            {
                RecentFiles.Remove(file);
                RecentsList.Remove(file.Entry.Token);
                if (TimeContainer.Values.ContainsKey(file.Entry.Token)) TimeContainer.Values.Remove(file.Entry.Token);
            }
        }

        public static void Clear()
        {
            RecentFiles.Clear();
            RecentsList.Clear();
            TimeContainer.Values.Clear();
        }
    }
}
