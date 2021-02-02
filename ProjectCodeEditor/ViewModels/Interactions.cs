using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Dialogs;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.Services;
using ProjectCodeEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using TextEditor.Languages;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor.ViewModels
{
    public static class Interactions
    {
        private static FileSavePicker savePicker = null;
        private static FileOpenPicker openFilePicker = null;
        private static readonly SymbolIconSource fileIcon = new() { Symbol = Symbol.Delete };
        private static readonly FontIconSource projectIcon = new FontIconSource()
        {
            Glyph = "&#xE8B7;",
            FontFamily = new("Segoe MDL2 Assets")
        };

        internal static bool StorageItemAlreadyOpen(IStorageItem2 storageItem, out ShellView view)
        {
            ShellView foundItem = null;
            foreach (var item in Preferences.AppShellViewModel.Instances)
            {
                if ((item.ReferenceSource?.IsEqual(storageItem)).GetValueOrDefault())
                {
                    foundItem = item;
                    break;
                }
            }

            view = foundItem;
            return view != null;
        }

        public static void AddStorageItems(IReadOnlyList<IStorageItem2> items)
        {
            List<ShellView> viewsToAdd = new();
            foreach (var item in items)
            {
                if (StorageItemAlreadyOpen(item, out ShellView view))
                {
                    if (items.Count == 1) Preferences.AppShellViewModel.SelectedItem = view;
                }
                else
                {
                    if (item.IsOfType(StorageItemTypes.File))
                    {
                        var file = item as StorageFile;
                        viewsToAdd.Add(new ShellView(file.Name, fileIcon, new CodeEditor(file), file));
                    }
                    else viewsToAdd.Add(new ShellView(item.Name, projectIcon, new ProjectView(item as StorageFolder), item));
                }
            }

            Preferences.AppShellViewModel.AddViews(viewsToAdd);
        }

        public static async void OpenFiles()
        {
            if (DialogHelper.PreparePresentation())
            {
                if (openFilePicker == null)
                {
                    openFilePicker = new FileOpenPicker()
                    {
                        SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                        ViewMode = PickerViewMode.List
                    };

                    openFilePicker.FileTypeFilter.AddRange(Preferences.SupportedFileTypes);
                }

                var files = await openFilePicker.PickMultipleFilesAsync();
                if (files.Count != 0) AddStorageItems(files);
                DialogHelper.EndPresentation();
            }
        }

        public static async void NewFile()
        {
            if (DialogHelper.PreparePresentation())
            {
                if (savePicker == null)
                {
                    savePicker = new FileSavePicker()
                    {
                        SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                        SuggestedFileName = "UntitledFileText".GetLocalized(),
                        DefaultFileExtension = ".md",
                    };

                    savePicker.FileTypeChoices.AddRange(Preferences.SupportedFileTypes.Select(item => new KeyValuePair<string, IList<string>>(GetFileTypeDescription(item), new string[] { item })));
                }

                var file = await savePicker.PickSaveFileAsync();
                if (file != null) AddStorageItems(new StorageFile[] { file });
                DialogHelper.EndPresentation();
            }
        }

        public static string GetFileTypeDescription(string fileType)
        {
            string prefix;
            if (LanguageProvider.LocateLanguage(fileType, out var lang)) prefix = lang.Name;
            else prefix = fileType.Substring(1);
            return $"{prefix} {"FileSubItem/Text".GetLocalized().ToLower()}";
        }

        public static async void OpenProject()
        {
            if (DialogHelper.PreparePresentation())
            {
                FolderPicker picker = new()
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                    ViewMode = PickerViewMode.List
                };

                picker.FileTypeFilter.Add("*");
                var folder = await picker.PickSingleFolderAsync();
                if (folder != null) AddStorageItems(new IStorageItem2[] { folder });
                DialogHelper.EndPresentation();
            }
        }
    }
}
