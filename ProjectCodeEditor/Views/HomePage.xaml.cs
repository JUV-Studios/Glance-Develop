using Microsoft.Toolkit.Uwp.Extensions;
using Microsoft.Toolkit.Uwp.Helpers;
using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Helpers;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.ViewModels;
using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ProjectCodeEditor.Views
{
    public sealed partial class HomePage : UserControl
    {
        public readonly RecentsViewModel Recents = Singleton<RecentsViewModel>.Instance;

        private RecentFile ContextedRecentFile = null;

        public readonly ReadOnlyCollection<ActionOption> Actions;

        private readonly string ShareItemTextId = "ShareItem/Text";

        public HomePage()
        {
            InitializeComponent();
            Actions = new(new List<ActionOption>()
            {
                new("OpenOption/Label".GetLocalized(), "OpenFileActionDescription".GetLocalized(), new SymbolIconSource() { Symbol = Symbol.OpenFile }, Interactions.OpenFile, "Ctrl+O"),
                new("NewFileActionTitle".GetLocalized(), "NewFileActionDescription".GetLocalized(), new SymbolIconSource() { Symbol = Symbol.Add }, Interactions.NewFile, "Ctrl+N")
            });
        }

        private void RecentFilesListItem_Click(object sender, ItemClickEventArgs e) => Interactions.AddFiles(new StorageFile[] { (e.ClickedItem as RecentFile).File });

        private void RemoveRecentFile_Click(object sender, RoutedEventArgs e)
        {
            if (ContextedRecentFile != null) Recents.RemoveRecentFile(ContextedRecentFile);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetRecentFilesVisibility();
            Recents.RecentFiles.CollectionChanged += RecentFiles_CollectionChanged;
            DataTransferManager.GetForCurrentView().DataRequested += HomePage_DataRequested;
        }

        private void RecentFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => SetRecentFilesVisibility();

        private void SetRecentFilesVisibility() => RecentFilesSection.Visibility = Recents.RecentFiles.IsEmpty() ? Visibility.Collapsed : Visibility.Visible;

        private async void OpenRecentFilePath_Click(object sender, RoutedEventArgs e)
        {
            if (ContextedRecentFile != null) await Launcher.LaunchFolderPathAsync(Path.GetDirectoryName(ContextedRecentFile.File.Path));
        }

        private void ShareRecentFile_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            DataTransferManager.GetForCurrentView().DataRequested -= HomePage_DataRequested;
        }

        private void HomePage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var deferral = args.Request.GetDeferral();
            if (ContextedRecentFile != null)
            {
                args.Request.Data.Properties.Title = string.Format("ShareFileTitle".GetLocalized(), ContextedRecentFile.File.Name);
                args.Request.Data.Properties.Description = string.Format("ShareFileCaption".GetLocalized(), ContextedRecentFile.File.Name);
                args.Request.Data.SetStorageItems(new StorageFile[] { ContextedRecentFile.File });
            }

            deferral.Complete();
        }

        private void ActionList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ActionOption option) option.ActionCommand();
        }

        private void Layout_Loaded(object sender, RoutedEventArgs e)
        {
            var layout = sender as Hub;
            AccessibilityHelper.SetProperties(layout);
            layout.Loaded -= Layout_Loaded;
        }

        private void RecentFilesList_Loaded(object sender, RoutedEventArgs e)
        {
            var target = sender as ListView;
            AccessibilityHelper.AttachContextMenu(target, RecentFileContextMenu, ShowContextFlyoutForRecentList);
        }

        private bool ShowContextFlyoutForRecentList(object val)
        {
            if (val == null) return false;
            if (val is RecentFile file && file != null)
            {
                ContextedRecentFile = file;
                return true;
            }

            return false;
        }

        private void RecentFilesList_Unloaded(object sender, RoutedEventArgs e)
        {
            var target = sender as ListView;
            AccessibilityHelper.DetachContextMenu(target);
        }
    }
}
