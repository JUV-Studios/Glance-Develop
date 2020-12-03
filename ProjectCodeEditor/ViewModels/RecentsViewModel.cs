using ProjectCodeEditor.Models;
using Swordfish.NET.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace ProjectCodeEditor.ViewModels
{
    public class RecentsViewModel
    {
        private readonly ApplicationDataContainer TimeContainer = ApplicationData.Current.LocalSettings.CreateContainer("RecentFileTime", ApplicationDataCreateDisposition.Always);

        private readonly StorageItemMostRecentlyUsedList RecentsList = StorageApplicationPermissions.MostRecentlyUsedList;

        public readonly ObservableCollection<RecentFile> RecentFiles = new();

        public async Task LoadRecentsAsync()
        {
            RecentFiles.Clear();
            List<RecentFile> filesTemp = new(RecentsList.Entries.Count());
            await Task.Run(async () =>
            {
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
            });

            var sorted = filesTemp.OrderByDescending(item => item.Time);
            foreach (var item in sorted) RecentFiles.Add(item);
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
