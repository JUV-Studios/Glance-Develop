/* using Microsoft.Toolkit.Mvvm.ComponentModel;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.Services;
using System.Collections.Specialized;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;

namespace ProjectCodeEditor.ViewModels
{
    public sealed class HomeViewModel : ObservableObject
    {
        public Visibility RecentFilesVisibility => RecentsViewModel.RecentFiles.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

        internal RecentItem ContextedRecentFile;

        public void RemoveRecentFile()
        {
            if (ContextedRecentFile != null) RecentsViewModel.RemoveRecentFile(ContextedRecentFile);
        }

        public void OpenRecentFilePath()
        {
            if (ContextedRecentFile != null) FileService.OpenFileLocationAsync(ContextedRecentFile.Item as StorageFile).ConfigureAwait(false);
        }

        public void ShareRecentFile() => DataTransferManager.ShowShareUI();

        public HomeViewModel()
        {
            RecentsViewModel.RecentFiles.CollectionChanged += RecentFiles_CollectionChanged;
        }

        internal bool ShowContextFlyoutForRecentList(object val)
        {
            if (val == null) return false;
            if (val is RecentItem file && file != null)
            {
                ContextedRecentFile = file;
                return true;
            }

            return false;
        }

        private void RecentFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => OnPropertyChanged(nameof(RecentFilesVisibility));
    }
}
*/