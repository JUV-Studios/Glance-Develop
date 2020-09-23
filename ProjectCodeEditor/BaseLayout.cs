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
            get => StorageForObject.Get(0);
            set => StorageForObject.Set(0, value);
        }

        private bool Suspended
        {
            get => StorageForObject.Get(1);
            set => StorageForObject.Set(1, value);
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
