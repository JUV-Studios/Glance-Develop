using Microsoft.Toolkit.Mvvm.ComponentModel;
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
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ProjectCodeEditor.ViewModels
{
    internal enum WhatToShare : byte { File, Text, Selection }

    public sealed class EditorViewModel : ObservableObject
    {
        public event Func<string> EditorTextRequested;

        public StorageFile WorkingFile { get; init; }

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        private bool _Saved = true;

        public bool Saved
        {
            get => _Saved;
            set => SetProperty(ref _Saved, value);
        }

        internal bool Unloaded = true;

        private bool _IsLoading = true;

        public bool IsLoading
        {
            get => _IsLoading;
            set => SetProperty(ref _IsLoading, value);
        }

        private bool _CanInteract = false;

        public bool CanInteract
        {
            get => _CanInteract;
            set => SetProperty(ref _CanInteract, value);
        }

        private SyntaxLanguage _CodeLanguage;

        public SyntaxLanguage CodeLanguage
        {
            get => _CodeLanguage;
            set
            {
                if (value != null) SetProperty(ref _CodeLanguage, value);
            }
        }

        internal TextPlusEncoding? FileReadData;

        internal WhatToShare ShareOption = WhatToShare.File;

        internal bool TabClosing = false;

        internal bool HistoryCleared = false;

        #region History

        public bool CanUndo => !UndoStack.IsEmpty();

        public bool CanRedo => !RedoStack.IsEmpty();

        internal readonly Stack<string> UndoStack = new();

        internal readonly Stack<string> RedoStack = new();

        internal void PushToUndoStack(string val)
        {
            UndoStack.Push(val);
            OnPropertyChanged(nameof(CanUndo));
            HistoryCleared = false;
        }

        internal string Undo()
        {
            string retVal;
            // Remove current one first
            RedoStack.Push(UndoStack.Pop());
            // Return second one
            if (UndoStack.IsEmpty()) retVal = FileReadData.Value.Text;
            else retVal = UndoStack.Pop();
            OnPropertyChanged(nameof(CanRedo));
            OnPropertyChanged(nameof(CanUndo));
            return retVal;
        }

        internal string Redo()
        {
            string retVal;
            retVal = RedoStack.Pop();
            OnPropertyChanged(nameof(CanRedo));
            OnPropertyChanged(nameof(CanUndo));
            return retVal;
        }

        public void ClearHistory()
        {
            UndoStack.Clear();
            RedoStack.Clear();
            OnPropertyChanged(nameof(CanRedo));
            OnPropertyChanged(nameof(CanUndo));
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
            else if (HistoryCleared) text = EditorTextRequested();
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
