using ProjectCodeEditor.Models;
using ProjectCodeEditor.ViewModels;
using System;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor
{
    public abstract class BaseLayout : UserControl, IDisposable
    {
        public ShellView ShellInstance;

        public Guid FrameID;

        public abstract void OnLoad();

        public abstract void OnSuspend();

        public abstract void OnResume();

        public abstract void Dispose();

        public BaseLayout()
        {
            EditorShellViewModel.FrameCreated += ShellViewModel_FrameCreated;
            EditorShellViewModel.FrameChanged += ShellViewModel_FrameChanged;
            EditorShellViewModel.FrameNavigationCompleted += ShellViewModel_FrameNavigationCompleted;
            EditorShellViewModel.FrameClosedRequested += ShellViewModel_FrameClosed;
        }

        private void ShellViewModel_FrameNavigationCompleted(object sender, ShellView e)
        {
            if (ShellInstance == e)
            {
                Debug.WriteLine("Resuming");
                OnResume();
            }
        }

        private void ShellViewModel_FrameClosed(object sender, ShellView e)
        {
            Debug.WriteLine("Disposing");
            EditorShellViewModel.FrameClosedRequested -= ShellViewModel_FrameClosed;
            Dispose();
        }

        private void ShellViewModel_FrameChanged(object sender, ShellView e)
        {
            Debug.WriteLine("Suspending");
            OnSuspend();
        }
        
        private void ShellViewModel_FrameCreated(object sender, ShellView e)
        {
            if (ShellInstance != null)
            {
                throw new InvalidOperationException("Incorrect usage");
            }

            FrameID = Guid.NewGuid();
            ShellInstance = e;
            OnLoad();
            EditorShellViewModel.FrameCreated -= ShellViewModel_FrameCreated;
        }
    }
}
