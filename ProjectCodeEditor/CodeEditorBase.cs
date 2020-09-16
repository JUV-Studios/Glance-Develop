using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.Helpers;
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

    public class CodeEditorBase : BaseLayout
    {
        public CodeEditorBaseViewModel ViewModel { get; } = new CodeEditorBaseViewModel();

        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public event EventHandler PageXamlLoaded;

        public event EventHandler Disposed;

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

        public override void Dispose()
        {
            // ShareCharm.DataRequested -= ShareCharm_DataRequested;
            // SaveFile();
            Disposed?.Invoke(this, null);
        }

        public override void OnResume()
        {
            // ShareCharm.DataRequested += ShareCharm_DataRequested;
        }

        public override void OnSuspend()
        {
        }

        protected override void OnLoad()
        {
            ViewModel.WorkingFile = ShellInstance.Parameter as StorageFile;
            // ShareCharm.DataRequested += ShareCharm_DataRequested;
        }

        protected async Task LoadFile()
        {
            ViewModel.WorkString = "WorkStringTaskLoad".GetLocalized();
            MainViewModel.RecentlyUsedList.AddOrReplace(ViewModel.WorkingFile.Path.Replace('\\', '-'), ViewModel.WorkingFile);
            ViewModel.FileData = await ViewModel.WorkingFile.ReadCodeFile();
        }

        protected override void OnXamlLoad()
        {
            PageXamlLoaded?.Invoke(this, null);
        }
    }
}
