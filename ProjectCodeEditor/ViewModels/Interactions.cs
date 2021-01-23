using Microsoft.Toolkit.HighPerformance.Helpers;
using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Dialogs;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.Views;
using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TextEditor.Languages;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using MUXC = Microsoft.UI.Xaml.Controls;

namespace ProjectCodeEditor.ViewModels
{
    public static class Interactions
    {
        private static readonly ShellViewModel viewModel = Singleton<ShellViewModel>.Instance;
        private static FileSavePicker savePicker = null;
        private static FileOpenPicker openFilePicker = null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ShellView CreateEditorStandalone(StorageFile file) => new(file.Name, file.Path, new MUXC.SymbolIconSource() { Symbol = Symbol.Document }, new CodeEditor(file));

        internal static bool StorageItemAlreadyOpen(IStorageItem storageItem, ref ShellView view, bool retInstanceRef = true)
        {
            ShellView foundItem = null;
            Parallel.ForEach(viewModel.Instances, (item, state) =>
            {
                Type pageType;
                if (storageItem.IsOfType(StorageItemTypes.File)) pageType = typeof(CodeEditor);
                else pageType = typeof(ProjectView);
                if (item.Content.GetType() == pageType && item.Title == storageItem.Name && item.Caption == storageItem.Path)
                {
                    if (retInstanceRef) foundItem = item;
                    state.Break();
                }
            });

            if (foundItem != null)
            {
                if (retInstanceRef) view = foundItem;
                return true;
            }
            else return false;
        }

        public static void AddFiles(IReadOnlyList<StorageFile> files)
        {
            if (files.Count() == 1)
            {
                ShellView view = null;
                if (StorageItemAlreadyOpen(files[0], ref view)) viewModel.SelectedIndex = viewModel.Instances.IndexOf(view);
                else viewModel.AddLayout(CreateEditorStandalone(files[0]));
            }
            else
            {
                foreach (var file in files)
                {
                    ShellView view = null;
                    if (!StorageItemAlreadyOpen(file, ref view, false)) viewModel.AddLayout(CreateEditorStandalone(file), true);
                }

                viewModel.SelectedIndex = viewModel.Instances.Count - 1;
            }
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

                    openFilePicker.FileTypeFilter.AddRange(App.AppSettings.SupportedFileTypes);
                }

                var files = await openFilePicker.PickMultipleFilesAsync();
                if (files.Count != 0) AddFiles(files);
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

                    savePicker.FileTypeChoices.AddRange(App.AppSettings.SupportedFileTypes.Select(item => new KeyValuePair<string, IList<string>>(GetFileTypeDescription(item), new string[] { item })));
                }

                var file = await savePicker.PickSaveFileAsync();
                if (file != null) AddFiles(new StorageFile[] { file });
                DialogHelper.EndPresentation();
            }
        }

        public static string GetFileTypeDescription(string fileType)
        {
            string fileTypeLower = fileType.ToLower();
            string prefix;
            if (!LanguageProvider.CodeLanguages.ContainsKey(fileTypeLower)) prefix = fileTypeLower.Substring(1);
            else prefix = LanguageProvider.CodeLanguages[fileTypeLower].Value.LanguageName;
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
                if (folder != null)
                {
                    ShellView view = null;
                    if (StorageItemAlreadyOpen(folder, ref view)) viewModel.SelectedIndex = viewModel.Instances.IndexOf(view);
                    else
                    {
                        Singleton<ShellViewModel>.Instance.AddLayout(new(folder.Name, folder.Path, new MUXC.FontIconSource()
                        {
                            Glyph = "&#xE8B7;",
                            FontFamily = new("Segoe MDL2 Assets"),
                        }, new ProjectView(folder)));
                    }
                }

                DialogHelper.EndPresentation();
            }
        }
    }
}
