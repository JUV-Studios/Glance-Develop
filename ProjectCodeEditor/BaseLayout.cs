using ProjectCodeEditor.Models;
using ProjectCodeEditor.Services;
using ProjectCodeEditor.ViewModels;
using System;
using System.Collections;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor
{
    public abstract class BaseLayout : UserControl, IDisposable
    {
        public ShellView ShellInstance;

        protected DataTransferManager ShareCharm;

        private BitArray StorageForObject = new BitArray(2);

        private bool FrameLoaded
        {
            get => MemoryService.Get(0, StorageForObject);
            set => MemoryService.Set(0, value, StorageForObject);
        }

        private bool Suspended
        {
            get => MemoryService.Get(1, StorageForObject);
            set => MemoryService.Set(1, value, StorageForObject);
        }

        protected abstract void OnXamlLoad();

        protected abstract void OnLoad();

        public abstract void OnSuspend();

        public abstract void OnResume();

        public abstract void Dispose();

        public BaseLayout()
        {
            EditorShellViewModel.FrameCreated += ShellViewModel_FrameCreated;
        }

        protected void BaseLayout_Loaded(object sender, RoutedEventArgs e)
        {
            FrameLoaded = true;
            ShareCharm = DataTransferManager.GetForCurrentView();
            OnXamlLoad();
        }

        private void ShellViewModel_FrameChanged(object sender, ShellView e)
        {
            if (FrameLoaded)
            {
                if (e != ShellInstance)
                {
                    if (!Suspended)
                    {
                        Suspended = true;
                        Debug.WriteLine("Suspending");
                        OnSuspend();
                    }
                }
                else
                {
                    if (Suspended)
                    {
                        Debug.WriteLine("Resuming");
                        OnResume();
                    }
                }
            }
        }

        private void ShellViewModel_FrameCreated(object sender, ShellView e)
        {
            if (ShellInstance != null)
            {
                throw new InvalidOperationException("Incorrect usage");
            }

            ShellInstance = e;
            OnLoad();
            EditorShellViewModel.FrameCreated -= ShellViewModel_FrameCreated;
            EditorShellViewModel.FrameChanged += ShellViewModel_FrameChanged;
        }
    }
}
