using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.Helpers;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.Services;
using ProjectCodeEditor.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Printing;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor
{
    public class CodeEditorBaseViewModel : Observable
    {
        public bool CanPrint = PrintManager.IsSupported();

        private string _WorkString;

        public string WorkString
        {
            get => _WorkString;
            set => Set(ref _WorkString, value);
        }

        private StorageFile _WorkingFile;

        public StorageFile WorkingFile
        {
            get => _WorkingFile;
            set => Set(ref _WorkingFile, value);
        }

        public TextPlusEncoding FileData;
    }

    public abstract class CodeEditorBase : UserControl, ILayoutView
    {
        public CodeEditorBaseViewModel ViewModel { get; } = new CodeEditorBaseViewModel();

        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        protected abstract void PerformCleanup();

        protected abstract void Exited();

        protected abstract void Suspend();

        protected abstract void Resume();

        public void ShareFile()
        {
            DataTransferManager.ShowShareUI();
        }

        private void ShareCharm_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.Properties.Title = $"Sharing {ViewModel.WorkingFile.Name}";
            args.Request.Data.Properties.Description = "This file will be shared";
            args.Request.Data.SetStorageItems(new IStorageItem[] { ViewModel.WorkingFile });
        }

        protected async void SaveFile()
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                ViewModel.WorkString = "WorkStringTaskSave".GetLocalized();
                await FileIO.WriteBytesAsync(ViewModel.WorkingFile, ViewModel.FileData.Encoding.GetBytes(ViewModel.FileData.Text));
            }
            catch (IOException)
            {

            }
            finally
            {
                _semaphoreSlim.Release();
                ViewModel.WorkString = "WorkStringReady".GetLocalized();
            }
        }

        public void Dispose() => PerformCleanup();

        protected async Task LoadFile()
        {
            ViewModel.WorkString = "WorkStringTaskLoad".GetLocalized();
            MainViewModel.RecentlyUsedList.AddOrReplace(ViewModel.WorkingFile.Path.Replace('\\', '-'), ViewModel.WorkingFile);
            ViewModel.FileData = await ViewModel.WorkingFile.ReadCodeFile();
        }

        public UIElement GetUserInterface() => this;

        public void Initialize(ShellView e)
        {
            ViewModel.WorkingFile = e.Parameter as StorageFile;
        }

        public void OnTabAdded()
        {
        }

        public void OnTabRemoveRequested() => Exited();

        public void SaveState() => Suspend();

        public void RestoreState() => Resume();
    }
}
