using ProjectCodeEditor.Helpers;
using ProjectCodeEditor.Models;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;

namespace ProjectCodeEditor.ViewModels
{
    public class MainViewModel : Observable
    {
        private ObservableCollection<RecentItem> _RecentItems = new ObservableCollection<RecentItem>();
        private Thickness _ContentMargin = new Thickness(0);
        public ReadOnlyObservableCollection<RecentItem> RecentItems;
        public static StorageItemMostRecentlyUsedList RecentlyUsedList = StorageApplicationPermissions.MostRecentlyUsedList;
        public static ApplicationDataContainer RecentPagesContainer;
        public event EventHandler RecentListChanged;

        public bool IsEmpty
        {
            get => _RecentItems.Count == 0;
        }

        public Thickness ContentMargin
        {
            get => _ContentMargin;
            set => Set(ref _ContentMargin, value);
        }

        public MainViewModel()
        {
            RecentItems = new ReadOnlyObservableCollection<RecentItem>(_RecentItems);
            RecentPagesContainer = ApplicationData.Current.LocalSettings.CreateContainer("recentPages", ApplicationDataCreateDisposition.Always);
            _RecentItems.CollectionChanged += (s, e) =>
            {
                RecentListChanged?.Invoke(this, e);
            };
        }

        public async void LoadRecentItems()
        {
            _RecentItems.Clear();
            foreach (var item in RecentlyUsedList.Entries)
            {
                try
                {
                    var file = await RecentlyUsedList.GetFileAsync(item.Token);
                    _RecentItems.Add(new RecentItem()
                    {
                        Token = item.Token,
                        FileHandle = file,
                        Title = file.Name
                    });
                }
                catch (FileNotFoundException)
                {
                    continue;
                }
                catch (ArgumentException)
                {
                    continue;
                }
            }

            foreach (var values in RecentPagesContainer.Values)
            {
                try
                {
                    _RecentItems.Add(new RecentItem()
                    {
                        Token = values.Value.ToString(),
                        IsWeb = true,
                        Title = values.Key
                    });
                }
                catch
                {
                    continue;
                }
            }

            OnPropertyChanged(nameof(IsEmpty));
        }

        public void ClearRecentItems()
        {
            _RecentItems.Clear();
            Task.Run(() => RecentlyUsedList.Clear());
            Task.Run(() => RecentPagesContainer.Values.Clear());
            OnPropertyChanged(nameof(IsEmpty));
        }

        public void DisposeRecentItems() => _RecentItems.Clear();

        public void RemoveRecentItem(int itemToRemoveIndex)
        {
            var item = _RecentItems[itemToRemoveIndex];
            if (item.IsWeb) RecentPagesContainer.Values.Remove(item.Title);
            else RecentlyUsedList.Remove(item.Token);
            _RecentItems.RemoveAt(itemToRemoveIndex);
            OnPropertyChanged(nameof(IsEmpty));

        }
    }
}
