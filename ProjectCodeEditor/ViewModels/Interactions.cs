using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TextEditor.Languages;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor.ViewModels
{
    public static class Interactions
    {
        private static readonly ShellViewModel viewModel = Singleton<ShellViewModel>.Instance;
        private static FileSavePicker savePicker = null;

        public static ShellView CreateEditorStandalone(StorageFile file)
        {
            var options = new Dictionary<string, object>()
            {
                { "file", file }
            };

            return new(file.Name, file.Path, new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Document }, new CodeEditor(options));
        }

        internal static bool FileAlreadyOpen(StorageFile file, ref ShellView view, bool retInstanceRef = true)
        {
            ShellView foundItem = null;
            Parallel.ForEach(viewModel.Instances, item =>
            {
                if (item.Content is CodeEditor && item.Caption == file.Path) if (retInstanceRef) foundItem = item;
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
                if (FileAlreadyOpen(files[0], ref view)) viewModel.SelectedIndex = viewModel.Instances.IndexOf(view);
                else viewModel.AddLayout(CreateEditorStandalone(files[0]));
            }
            else
            {
                foreach (var file in files)
                {
                    ShellView view = null;
                    if (!FileAlreadyOpen(file, ref view, false)) viewModel.AddLayout(CreateEditorStandalone(file), true);
                }

                viewModel.SelectedIndex = viewModel.Instances.Count - 1;
            }
        }

        public static async void OpenFiles()
        {
            if (!App.AppSettings.DialogShown)
            {
                App.AppSettings.DialogShown = true;
                var picker = new FileOpenPicker()
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                    ViewMode = PickerViewMode.List
                };
                foreach (var fileType in App.AppSettings.SupportedFileTypes) picker.FileTypeFilter.Add(fileType);
                var files = await picker.PickMultipleFilesAsync();
                if (files.Count != 0) AddFiles(files);
                App.AppSettings.DialogShown = false;
            }
        }

        public static async void NewFile()
        {
            if (!App.AppSettings.DialogShown)
            {
                App.AppSettings.DialogShown = true;
                if (savePicker == null)
                {
                    savePicker = new FileSavePicker()
                    {
                        SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                        SuggestedFileName = "UntitledFileText".GetLocalized(),
                        DefaultFileExtension = ".md",
                    };

                    foreach (var fileType in App.AppSettings.SupportedFileTypes) savePicker.FileTypeChoices.Add(GetFileTypeDescription(fileType), new string[] { fileType });
                }

                var file = await savePicker.PickSaveFileAsync();
                if (file != null) AddFiles(new StorageFile[] { file });
                App.AppSettings.DialogShown = false;
            }
        }

        public static string GetFileTypeDescription(string fileType)
        {
            string fileTypeLower = fileType.ToLower();
            string prefix;
            if (LanguageProvider.CodeLanguages.ContainsKey(fileTypeLower)) prefix = LanguageProvider.CodeLanguages[fileTypeLower].Value.LanguageName;
            else prefix = fileTypeLower.Substring(1);
            return $"{prefix} {"FileSubItem/Text".GetLocalized().ToLower()}";
        }

        public static async void OpenProject()
        {
            if (!App.AppSettings.DialogShown)
            {
                App.AppSettings.DialogShown = true;
                FolderPicker picker = new()
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };
                await picker.PickSingleFolderAsync();
                App.AppSettings.DialogShown = false;
            }
        }
    }
}
