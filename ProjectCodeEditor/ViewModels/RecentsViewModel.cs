using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Models;
using Swordfish.NET.Collections.Auxiliary;
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
        internal ObservableCollection<RecentFile> OriginalItemSource = null;

        private string LastSearchString = string.Empty;

        public RecentFilesList()
        {
            Style = App.Current.Resources["RecentListItemStyle"] as Style;
            SetBinding(ItemsSourceProperty, new Binding()
            {
                Source = Singleton<RecentsViewModel>.Instance.RecentFiles,
                Mode = BindingMode.OneWay,
            });
        }

        public void Search(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                ItemsSource = OriginalItemSource;
                return;
            }

            IEnumerable<RecentFile> newSource = null;
            newSource = OriginalItemSource.Where(item => item.File.Name.Contains(searchQuery));
            LastSearchString = searchQuery;
            ItemsSource = newSource;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            if (OriginalItemSource == null)
            {
                OriginalItemSource = ItemsSource as ObservableCollection<RecentFile>;
                OriginalItemSource.CollectionChanged += OriginalItemSource_CollectionChanged;
            }

            FrameworkElement source = element as FrameworkElement;
            var context = item as RecentFile;
            ToolTipService.SetToolTip(source, new TextBlock()
            {
                Text = context.File.Path,
                TextWrapping = TextWrapping.Wrap,
            });
        }

        private void OriginalItemSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ItemsSource != OriginalItemSource)
            {
                ItemsSource = OriginalItemSource;
                if (!string.IsNullOrWhiteSpace(LastSearchString)) Search(LastSearchString);
            }
        }
    }

    public sealed class RecentsViewModel
    {
        private readonly ApplicationDataContainer TimeContainer = ApplicationData.Current.LocalSettings.CreateContainer("RecentFileTime", ApplicationDataCreateDisposition.Always);

        private readonly StorageItemMostRecentlyUsedList RecentsList = StorageApplicationPermissions.MostRecentlyUsedList;

        public readonly RangedObservableCollection<RecentFile> RecentFiles = new();

        private bool Loading = false;

        public async Task LoadRecentsAsync()
        {
            if (!Loading)
            {
                Loading = true;
                RecentFiles.Clear();
                IOrderedEnumerable<RecentFile> filesSorted = null;
                await Task.Run(async () =>
                {
                    List<RecentFile> filesTemp = new(RecentsList.Entries.Count());
                    foreach (var item in RecentsList.Entries)
                    {
                        try
                        {
                            DateTime time;
                            if (TimeContainer.Values.TryGetValue(item.Token, out object timeOffset)) time = ((DateTimeOffset)timeOffset).DateTime;
                            else time = DateTime.Now;
                            var file = await RecentsList.GetFileAsync(item.Token);
                            filesTemp.Add(new(file, time, item));
                        }
                        catch (FileNotFoundException)
                        {
                            RecentsList.Remove(item.Token);
                        }
                    }

                    filesSorted = filesTemp.OrderByDescending(item => item.Time);
                });

                RecentFiles.AddRange(filesSorted.AsEnumerable());
                Loading = false;
            }
        }

        public void AddRecentFile(StorageFile file)
        {
            var currentTime = DateTimeOffset.Now;
            var token = file.Path.Replace("\\", "-");
            TimeContainer.Values[token] = currentTime;
            if (!RecentsList.ContainsItem(token))
            {
                RecentsList.AddOrReplace(token, file);
                RecentFiles.Insert(0, new(file, currentTime.DateTime, RecentsList.Entries.Where(item => item.Token == token).First()));
            }
            else
            {
                var fileItem = RecentFiles.Where(item => item.File.IsEqual(file)).First();
                if (RecentFiles.Remove(fileItem)) RecentFiles.Insert(0, fileItem);
            }
        }

        public void RemoveRecentFile(RecentFile file)
        {
            if (RecentFiles.Contains(file))
            {
                RecentFiles.Remove(file);
                RecentsList.Remove(file.Entry.Token);
                if (TimeContainer.Values.ContainsKey(file.Entry.Token)) TimeContainer.Values.Remove(file.Entry.Token);
            }
        }

        public void Clear()
        {
            RecentFiles.Clear();
            RecentsList.Clear();
            foreach (var key in TimeContainer.Values.Keys) TimeContainer.Values.Remove(key);
        }
    }
}
