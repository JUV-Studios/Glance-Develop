using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.Helpers;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ProjectCodeEditor.Views
{
    public sealed partial class HomePage : UserControl
    {
        public readonly HomeViewModel ViewModel = new HomeViewModel();

        public HomePage() => InitializeComponent();

        private void RecentFilesListItem_Click(object sender, ItemClickEventArgs e) => Interactions.AddFiles(new StorageFile[] { (e.ClickedItem as RecentFile).File });

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DataTransferManager.GetForCurrentView().DataRequested += HomePage_DataRequested;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            DataTransferManager.GetForCurrentView().DataRequested -= HomePage_DataRequested;
        }

        private void HomePage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var deferral = args.Request.GetDeferral();
            if (ViewModel.ContextedRecentFile != null)
            {
                args.Request.Data.Properties.Title = string.Format("ShareFileTitle".GetLocalized(), ViewModel.ContextedRecentFile.File.Name);
                args.Request.Data.Properties.Description = string.Format("ShareFileCaption".GetLocalized(), ViewModel.ContextedRecentFile.File.Name);
                args.Request.Data.SetStorageItems(new StorageFile[] { ViewModel.ContextedRecentFile.File });
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
                ViewModel.ContextedRecentFile = file;
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
