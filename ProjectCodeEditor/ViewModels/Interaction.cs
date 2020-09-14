using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ProjectCodeEditor.ViewModels
{
    public static class Interaction
    {
        private static bool IsFilePickerOpen = false;

        public static async void CreateFile()
        {

        }

        public static async void OpenFile()
        {
            if (!IsFilePickerOpen)
            {
                IsFilePickerOpen = true;
                var openPicker = new FileOpenPicker()
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };

                var fileTypes = await FileIO.ReadLinesAsync(Package.Current.InstalledLocation.GetFolderAsync("Assets").AsTask().Result.GetFileAsync("FileTypes").GetAwaiter().GetResult());
                foreach (var fileType in fileTypes)
                {
                    openPicker.FileTypeFilter.Add(fileType);
                }

                var files = await openPicker.PickMultipleFilesAsync();
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        App.ShellViewModel.AddFile(file, true);
                    }

                    App.ShellViewModel.SelectedItem = App.ShellViewModel.Instances.Last();
                }

                IsFilePickerOpen = false;
            }
        }
    }
}
