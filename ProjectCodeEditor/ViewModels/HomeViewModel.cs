using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Uwp.Extensions;
using Windows.UI.Xaml.Controls;
using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.Services;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using System.Collections.Specialized;
using Swordfish.NET.Collections.Auxiliary;

namespace ProjectCodeEditor.ViewModels
{
    public sealed class HomeViewModel : ObservableObject
    {
        public readonly RecentsViewModel Recents = Singleton<RecentsViewModel>.Instance;

        public readonly ActionOption[] Actions = new ActionOption[]
        {
            new("OpenOption/Label".GetLocalized(), "OpenFileActionDescription".GetLocalized(), new SymbolIconSource() { Symbol = Symbol.OpenFile }, Interactions.OpenFiles, "Ctrl+O"),
            new("NewFileActionTitle".GetLocalized(), "NewFileActionDescription".GetLocalized(), new SymbolIconSource() { Symbol = Symbol.Add }, Interactions.NewFile, "Ctrl+N")
        };

        public Visibility RecentFilesVisibility => Recents.RecentFiles.IsEmpty() ? Visibility.Collapsed : Visibility.Visible;

        internal RecentFile ContextedRecentFile;

        public void RemoveRecentFile()
        {
            if (ContextedRecentFile != null) Recents.RemoveRecentFile(ContextedRecentFile);
        }

        public async void OpenRecentFilePath()
        {
            if (ContextedRecentFile != null) await FileService.OpenFileLocation(ContextedRecentFile.File);
        }

        public void ShareRecentFile() => DataTransferManager.ShowShareUI();

        public HomeViewModel()
        {
            Recents.RecentFiles.CollectionChanged += RecentFiles_CollectionChanged;
        }

        internal bool ShowContextFlyoutForRecentList(object val)
        {
            if (val == null) return false;
            if (val is RecentFile file && file != null)
            {
                ContextedRecentFile = file;
                return true;
            }

            return false;
        }

        private void RecentFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => OnPropertyChanged(nameof(RecentFilesVisibility));
    }
}
