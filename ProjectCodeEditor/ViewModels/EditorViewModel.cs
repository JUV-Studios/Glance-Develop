using Microsoft.Toolkit.HighPerformance.Extensions;
using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Helpers;
using ProjectCodeEditor.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TextEditor;
using TextEditor.Languages;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ProjectCodeEditor.ViewModels
{
    internal enum WhatToShare : byte { File, Text, Selection }

    public sealed class EditorViewModel : FlagsModel
    {
        internal object PropertyChangeReturn;

        public EditorViewModel(StorageFile file) : base(16)
        {
            WorkingFile = file;
            IsLoading = true;
            Unloaded = true;
            Saved = true;
        }

        public readonly StorageFile WorkingFile;

        public string FileLocation => FileService.GetFolderPath(WorkingFile);

        private static readonly SemaphoreSlim saveLock = new SemaphoreSlim(1, 1);

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

        internal bool PropertiesSet
        {
            get => GetFlag(5);
            set => SetFlag(5, value);
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
                if (_UserContent == null) SetProperty(ref _UserContent, value);
                else
                {
                    var valTrim = value.TrimEndings();
                    if (valTrim == OriginalString.TrimEndings())
                    {
                        SetProperty(ref _UserContent, value);
                        Saved = true;
                        Save();
                        ClearHistory();
                    }
                    else if (valTrim != _UserContent.TrimEnd())
                    {
                        SetProperty(ref _UserContent, value);
                        Saved = false;
                        // Undo/Redo support
                        OnPropertyChanged("RetriveSelection");
                        UndoStack.Push(new(value, (int)PropertyChangeReturn));
                        HistoryCleared = false;
                        UpdateHistoryProperties();
                        if (Preferences.AppSettings.AutoSave) Save();
                    }
                }
            }
        }

        internal string OriginalString;

        internal Encoding FileEncoding;

        public async Task LoadFileAsync()
        {
            var readData = await FileService.ReadTextFileAsync(WorkingFile);
            FileEncoding = readData.Item2;
            OriginalString = readData.Item1;
            UserContent = OriginalString;
            LanguageProvider.LocateLanguage(WorkingFile.FileType, out _CodeLanguage);
            OnPropertyChanged(nameof(CodeLanguage));
            IsLoading = false;
        }

        public void OpenFileLocation() => FileService.OpenFileLocationAsync(WorkingFile).ConfigureAwait(false);

        #region History

        public bool CanUndo => UndoStack.Count > 0;

        public bool CanRedo => RedoStack.Count > 0;

        public bool CanClearHistory => CanUndo || CanRedo;

        private readonly Stack<(string, int)> UndoStack = new();

        private readonly Stack<(string, int)> RedoStack = new();

        internal int HistoryRange;

        private void UpdateHistoryProperties()
        {
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
            OnPropertyChanged(nameof(CanClearHistory));
        }

        public void Undo()
        {
            if (!CanUndo) return;
            (string, int) retVal;
            // Remove current one first
            RedoStack.Push(UndoStack.Pop());
            // Return second one
            if (UndoStack.Count == 0) retVal = new(OriginalString, -1);
            else retVal = UndoStack.Pop();
            UpdateHistoryProperties();
            UserContent = retVal.Item1;
            OnPropertyChanged("HistoryItemDone");
            HistoryRange = retVal.Item2;
        }

        public void Redo()
        {
            if (!CanRedo) return;
            (string, int) retVal;
            retVal = RedoStack.Pop();
            UserContent = retVal.Item1;
            UpdateHistoryProperties();
            OnPropertyChanged("HistoryItemDone");
            HistoryRange = retVal.Item2;
        }

        public void ClearHistory() => ClearHistoryImpl(true);

        private void ClearHistoryImpl(bool setFlag)
        {
            UndoStack.Clear();
            RedoStack.Clear();
            UpdateHistoryProperties();
            if (setFlag) HistoryCleared = true;
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

                    var currentType = WorkingFile.FileType.ToLower();
                    _SaveAsPicker.FileTypeChoices.Add(Interactions.GetFileTypeDescription(currentType), new string[] { currentType });
                    _SaveAsPicker.FileTypeChoices.AddRange(Preferences.SupportedFileTypes.Where(item => item != currentType)
                        .Select(item => new KeyValuePair<string, IList<string>>(Interactions.GetFileTypeDescription(item), new string[] { item }))); 
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
            await saveLock.WaitAsync();
            try
            {
                await FileIO.WriteTextAsync(file, text);
                //await FileIO.WriteBytesAsync(file, FileEncoding.GetBytes(text));
                Saved = true;
                Debug.WriteLine($"Saved {file.Path}");
                if (sendNotification)
                {
                    NotificationHelper.SendBasicNotification("FileSaveNotificationTitle".GetLocalized(), string.Format("FileSaveNotificationContent".GetLocalized(), file.Name));
                }

                if (!isWorkingFile) Interactions.AddStorageItems(new IStorageItem2[] { file });
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
                saveLock.Release();
            }

            return result;
        }

        private async Task<bool?> SaveToFileAsync(StorageFile file)
        {
            string text;
            if (UndoStack.Count == 0 && !HistoryCleared) return null;
            else if (HistoryCleared) text = UserContent;
            else text = UndoStack.Peek().Item1.TrimEnd();
            return await SaveAsync(file, text);
        }

        public void Save() => SaveAsync().ConfigureAwait(false);

        public async Task<bool> SaveAsync()
        {
            if (!Saved)
            {
                var result = await SaveToFileAsync(WorkingFile);
                if (result == null) Saved = true;
                else if (result == false && !TabClosing) SaveAs();
                return result.GetValueOrDefault();
            }

            return false;
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
    }
}
