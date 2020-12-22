using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Models;
using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public RecentFilesList()
        {
            Style = App.Current.Resources["RecentListItemStyle"] as Style;
            SetBinding(ItemsSourceProperty, new Binding()
            {
                Source = Singleton<RecentsViewModel>.Instance.RecentFiles,
                Mode = BindingMode.OneWay,
            });
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            FrameworkElement source = element as FrameworkElement;
            var context = item as RecentFile;
            ToolTipService.SetToolTip(source, new TextBlock()
            {
                Text = $"{context.File.Name} ({context.File.Path})",
                TextWrapping = TextWrapping.NoWrap,
                TextTrimming = TextTrimming.CharacterEllipsis
            });
        }
    }

    public sealed class RecentsViewModel
    {
        private readonly ApplicationDataContainer TimeContainer = ApplicationData.Current.LocalSettings.CreateContainer("RecentFileTime", ApplicationDataCreateDisposition.Always);

        private readonly StorageItemMostRecentlyUsedList RecentsList = StorageApplicationPermissions.MostRecentlyUsedList;

        public readonly ObservableCollection<RecentFile> RecentFiles = new();

        public async Task LoadRecentsAsync()
        {
            RecentFiles.Clear();
            IOrderedEnumerable<RecentFile> filesSorted = null;
            await Task.Run(() =>
            {
                List<RecentFile> filesTemp = new(RecentsList.Entries.Count());
                Parallel.ForEach(RecentsList.Entries, async item =>
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
                });

                filesSorted = filesTemp.OrderByDescending(item => item.Time);
            });

            RecentFiles.AddRange(filesSorted.AsEnumerable());
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
            else LoadRecentsAsync().ConfigureAwait(false);
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
