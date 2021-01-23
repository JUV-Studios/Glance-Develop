using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Helpers;
using ProjectCodeEditor.Services;
using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TextEditor;
using TextEditor.Languages;
using UtfUnknown;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ProjectCodeEditor.ViewModels
{
    internal enum WhatToShare : byte { File, Text, Selection }

    public sealed class EditorViewModel : FlagsModel, IDisposable, ISuspendable
    {
        internal object PropertyChangeReturn;

        public EditorViewModel(StorageFile file) : base(16)
        {
            // __SuggestionListReadOnly = new(_SuggestionList);
            Singleton<RecentsViewModel>.Instance.AddRecentFile(file);
            WorkingFile = file;
            IsLoading = true;
            Unloaded = true;
            Saved = true;
        }

        public readonly StorageFile WorkingFile;

        private static readonly SemaphoreSlim saveLock = new SemaphoreSlim(1, 1);

        private static readonly SemaphoreSlim lifecycleLock = new SemaphoreSlim(1, 1);

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

        public bool Suspended
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
                if (_UserContent == null)
                {
                    SetProperty(ref _UserContent, value);
                    return;
                }
                else
                {
                    if (value.TrimEnd() == FileReadData.Value.Item1.TrimEnd())
                    {
                        SetProperty(ref _UserContent, value);
                        Saved = true;
                        Save();
                    }
                    else if (value.TrimEnd() != _UserContent.TrimEnd())
                    {
                        SetProperty(ref _UserContent, value);
                        Saved = false;

                        // Undo/Redo support
                        OnPropertyChanged("RetriveSelection");
                        UndoStack.Push(new(value, (int)PropertyChangeReturn));
                        HistoryCleared = false;
                        UpdateHistoryProperties();

                        if (App.AppSettings.AutoSave) Save();
                    }
                }
            }
        }

        internal (string, object)? FileReadData;

        internal WhatToShare ShareOption = WhatToShare.File;

        public async Task LoadFileAsync()
        {
            FileReadData = await FileService.ReadTextFileAsync(WorkingFile);
            UserContent = FileReadData.Value.Item1;
            string fileFormat = WorkingFile.FileType.ToLower();
            if (LanguageProvider.CodeLanguages.TryGetValue(fileFormat, out Lazy<SyntaxLanguage> syntaxLang)) CodeLanguage = syntaxLang.Value;
            else CodeLanguage = LanguageProvider.CodeLanguages[".txt"].Value;
            IsLoading = false;
        }

        public void OpenFileLocation() => FileService.OpenFileLocationAsync(WorkingFile).ConfigureAwait(false);

        public void Dispose()
        {
        }

        public async Task SuspendAsync()
        {
            if (!Suspended && (CanUndo || CanRedo))
            {
                await lifecycleLock.WaitAsync();
                Suspended = true;
                // Clear and write history stack collections to reduce memory usage.
                UndoSuspendFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync($"{WorkingFile.Path.Replace("\\", "-")} undo data",
                    CreationCollisionOption.ReplaceExisting);
                RedoSuspendFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync($"{WorkingFile.Path.Replace("\\", "-")} redo data",
                    CreationCollisionOption.ReplaceExisting);
                await Task.Run(() =>
                {
                    using (var undoStream = File.Open(UndoSuspendFile.Path, FileMode.Create))
                    {
                        ObjectSerialize.Write(UndoStack, undoStream);
                    }

                    using (var redoStream = File.Open(RedoSuspendFile.Path, FileMode.Create))
                    {
                        ObjectSerialize.Write(RedoStack, redoStream);
                    }
                });

                ClearHistoryImpl(false);
                UndoStack = null;
                RedoStack = null;
                lifecycleLock.Release();
            }
        }

        public async Task ResumeAsync()
        {
            if (Suspended && UndoStack == null && RedoStack == null)
            {
                await lifecycleLock.WaitAsync();
                Suspended = false;
                await Task.Run(() =>
                {
                    using (var undoStream = File.Open(UndoSuspendFile.Path, FileMode.Open, FileAccess.Read))
                    {
                        UndoStack = ObjectSerialize.Read(undoStream) as Stack<(string, int)>;
                    }

                    using (var redoStream = File.Open(RedoSuspendFile.Path, FileMode.Open, FileAccess.Read))
                    {
                        RedoStack = ObjectSerialize.Read(redoStream) as Stack<(string, int)>;
                    }
                });

                UpdateHistoryProperties();
                await FileService.DeleteFileAsync(UndoSuspendFile);
                await FileService.DeleteFileAsync(RedoSuspendFile);
                lifecycleLock.Release();
            }
        }

        private StorageFile UndoSuspendFile = null;

        private StorageFile RedoSuspendFile = null;

        #region History

        public bool CanUndo => !UndoStack.IsEmpty();

        public bool CanRedo => !RedoStack.IsEmpty();

        public bool CanClearHistory => CanUndo || CanRedo;

        private Stack<(string, int)> UndoStack = new();

        private Stack<(string, int)> RedoStack = new();

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
            if (UndoStack.IsEmpty()) retVal = new(FileReadData.Value.Item1, -1);
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
            await saveLock.WaitAsync();
            try
            {
                Encoding fileEncoding;
                if (FileReadData.Value.Item2 is DetectionResult detail) fileEncoding = detail.Detected.Encoding;
                else fileEncoding = FileReadData.Value.Item2 as Encoding;
                await FileIO.WriteBytesAsync(file, fileEncoding.GetBytes(text));
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
                saveLock.Release();
            }

            return result;
        }

        private async Task<bool?> SaveToFileAsync(StorageFile file)
        {
            string text;
            if (UndoStack.IsEmpty() && !HistoryCleared) return null;
            else if (HistoryCleared) text = UserContent;
            else text = UndoStack.Peek().Item1.TrimEnd();
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
