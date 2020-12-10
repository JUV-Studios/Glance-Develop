using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Helpers;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.Services;
using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TextEditor;
using TextEditorUWP.Languages;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ProjectCodeEditor.ViewModels
{
    internal enum WhatToShare : byte { File, Text, Selection }

    public sealed class EditorViewModel : FlagsModel
    {
        public event Action HistoryItemDone;

        public EditorViewModel(StorageFile file) : base(16)
        {
            Singleton<RecentsViewModel>.Instance.AddRecentFile(file);
            WorkingFile = file;
            IsLoading = true;
            Unloaded = true;
            Saved = true;
        }

        public readonly StorageFile WorkingFile;

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public bool Saved
        {
            get => GetFlag(0);
            set => SetFlag(0, value);
        }

        public bool IsLoading
        {
            get => GetFlag(1);
            set => SetFlag(1, value);
        }

        internal bool Unloaded
        {
            get => GetFlag(2);
            set => SetFlag(2, value);
        }

        internal bool TabClosing
        {
            get => GetFlag(3);
            set => SetFlag(3, value);
        }

        internal bool HistoryCleared
        {
            get => GetFlag(4);
            set => SetFlag(4, value);
        }

        private SyntaxLanguage _CodeLanguage;

        public SyntaxLanguage CodeLanguage
        {
            get => _CodeLanguage;
            set => SetProperty(ref _CodeLanguage, value);
        }

        private string _UserContent;

        public string UserContent
        {
            get => _UserContent;
            set
            {
                if (_UserContent == null)
                {
                    SetProperty(ref _UserContent, value);
                    return;
                }
                else
                {
                    if (value.TrimEnd() == FileReadData.Value.Text.TrimEnd())
                    {
                        SetProperty(ref _UserContent, value);
                        Saved = true;
                    }
                    else if (value.TrimEnd() != _UserContent.TrimEnd())
                    {
                        SetProperty(ref _UserContent, value);
                        Saved = false;

                        // Undo/Redo support
                        UndoStack.Push(value);
                        HistoryCleared = false;
                        UpdateHistoryProperties();

                        if (App.AppSettings.AutoSave) Save();
                    }
                }
            }
        }

        internal TextPlusEncoding? FileReadData;

        internal WhatToShare ShareOption = WhatToShare.File;

        public async Task LoadFileAsync()
        {
            FileReadData = await FileService.ReadTextFileAsync(WorkingFile);
            UserContent = FileReadData.Value.Text;
            string fileFormat = WorkingFile.FileType.ToLower();
            if (LanguageProvider.CodeLanguages.TryGetValue(fileFormat, out Lazy<SyntaxLanguage> syntaxLang)) CodeLanguage = syntaxLang.Value;
            else CodeLanguage = LanguageProvider.CodeLanguages[".txt"].Value;
            IsLoading = false;
        }

        #region History

        public bool CanUndo => !UndoStack.IsEmpty();

        public bool CanRedo => !RedoStack.IsEmpty();

        public bool CanClearHistory => CanUndo || CanRedo;

        private readonly Stack<string> UndoStack = new();

        private readonly Stack<string> RedoStack = new();

        private void UpdateHistoryProperties()
        {
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
            OnPropertyChanged(nameof(CanClearHistory));
            HistoryItemDone?.Invoke();
        }

        public void Undo()
        {
            if (!CanUndo) return;
            string retVal;
            // Remove current one first
            RedoStack.Push(UndoStack.Pop());
            // Return second one
            if (UndoStack.IsEmpty()) retVal = FileReadData.Value.Text;
            else retVal = UndoStack.Pop();
            UpdateHistoryProperties();
            UserContent = retVal;
        }

        public void Redo()
        {
            if (!CanRedo) return;
            UserContent = RedoStack.Pop();
            UpdateHistoryProperties();
        }

        public void ClearHistory()
        {
            UndoStack.Clear();
            RedoStack.Clear();
            UpdateHistoryProperties();
            HistoryCleared = true;
        }

        #endregion

        #region Saving

        private FileSavePicker _SaveAsPicker = null;

        private FileSavePicker SaveAsPicker
        {
            get
            {
                if (_SaveAsPicker == null)
                {
                    _SaveAsPicker = new FileSavePicker()
                    {
                        CommitButtonText = "SaveAsItem/Text".GetLocalized(),
                        DefaultFileExtension = WorkingFile.FileType,
                        SuggestedFileName = string.Format("SaveAsPickerDefaultName".GetLocalized(), WorkingFile.DisplayName),
                        SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                    };

                    foreach (var fileType in Singleton<SettingsViewModel>.Instance.SupportedFileTypes)
                    {
                        _SaveAsPicker.FileTypeChoices.Add(Interactions.GetFileTypeDescription(fileType), new string[] { fileType });
                    }
                }

                return _SaveAsPicker;
            }
        }

        /// <summary>
        /// Saves content to file
        /// </summary>
        /// <param name="file">the file to save specifed content to</param>
        /// <param name="text">the text to write</param>
        /// <returns>Returns true if successful, else false</returns>
        private async Task<bool> SaveAsync(StorageFile file, string text)
        {
            bool isWorkingFile = file.IsEqual(WorkingFile);
            bool sendNotification = true;
            bool result = true;
            if ((Singleton<SettingsViewModel>.Instance.AutoSave && isWorkingFile) || ViewService.Properties.AppClosing) sendNotification = false;
            await _semaphoreSlim.WaitAsync();
            try
            {
                await FileIO.WriteBytesAsync(file, FileReadData.Value.Encoding.GetBytes(text));
                Saved = true;
                Debug.WriteLine($"Saved {file.Path}");
                if (sendNotification)
                {
                    NotificationHelper.SendBasicNotification("FileSaveNotificationTitle".GetLocalized(), string.Format("FileSaveNotificationContent".GetLocalized(), file.Name));
                }

                if (!isWorkingFile) Interactions.AddFiles(new StorageFile[] { file });
            }
            catch (FileNotFoundException)
            {
                if (!TabClosing) result = false;
            }
            catch (FileLoadException ex)
            {
                if (!Singleton<SettingsViewModel>.Instance.AutoSave) throw ex;
            }
            catch (UnauthorizedAccessException)
            {
                if (!TabClosing) result = false;
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return result;
        }

        private async Task<bool?> SaveToFileAsync(StorageFile file)
        {
            string text;
            if (UndoStack.IsEmpty() && !HistoryCleared) return null;
            else if (HistoryCleared) text = UserContent;
            else text = UndoStack.Peek().TrimEnd();
            return await SaveAsync(file, text);
        }

        public async void Save()
        {
            if (!Saved)
            {
                var result = await SaveToFileAsync(WorkingFile);
                if (result == null) Saved = true;
                else if (result == false && !TabClosing) SaveAs();
            }
        }

        public async void SaveAs()
        {
            if (!Singleton<SettingsViewModel>.Instance.DialogShown)
            {
                Singleton<SettingsViewModel>.Instance.DialogShown = true;
                var file = await SaveAsPicker.PickSaveFileAsync();
                if (file != null) await SaveToFileAsync(file);
                Singleton<SettingsViewModel>.Instance.DialogShown = false;
            }
        }

        #endregion

        #region Sharing

        public void ShareAsFile()
        {
            ShareOption = WhatToShare.File;
            DataTransferManager.ShowShareUI();
        }

        public void ShareAsText()
        {
            ShareOption = WhatToShare.Text;
            DataTransferManager.ShowShareUI();
        }

        public void ShareAsSelection()
        {
            ShareOption = WhatToShare.Selection;
            DataTransferManager.ShowShareUI();
        }

        #endregion
    }
}
