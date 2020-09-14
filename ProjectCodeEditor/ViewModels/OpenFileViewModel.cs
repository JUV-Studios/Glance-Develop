using ProjectCodeEditor.Helpers;
using ProjectCodeEditor.Models;
using System;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.Storage.Search;

namespace ProjectCodeEditor.ViewModels
{
    public class OpenFileViewModel : Observable
    {
        private bool _ShowBackButton = false;
        private ObservableCollection<RecentItem> _SelectableFiles = new ObservableCollection<RecentItem>();
        public ReadOnlyObservableCollection<RecentItem> SelectableFiles;

        public bool ShowBackButton
        {
            get => _ShowBackButton;
            set => Set(ref _ShowBackButton, value);
        }

        public OpenFileViewModel()
        {
            SelectableFiles = new ReadOnlyObservableCollection<RecentItem>(_SelectableFiles);
        }

        public async void LoadSelectableFiles()
        {
            var library = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Documents);
            var query = library.SaveFolder.CreateFileQueryWithOptions(new QueryOptions()
            {
                IndexerOption = IndexerOption.UseIndexerWhenAvailable,
                FolderDepth = FolderDepth.Deep
            });
            var files = await query.GetFilesAsync();
            foreach (var file in files)
            {
                _SelectableFiles.Add(new RecentItem()
                {
                    FileHandle = file,
                    Token = null
                });
            }
        }
    }
}
