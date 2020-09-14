using ProjectCodeEditor.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor
{
    public abstract class BaseLayout : UserControl, IDisposable
    {
        public ShellView ShellInstance;

        public Guid FrameID;

        public abstract void OnLoad();

        public abstract void OnSuspend();

        public abstract void OnExit();

        public abstract void Dispose();

        public BaseLayout()
        {
            App.ShellViewModel.FrameCreated += ShellViewModel_FrameCreated;
            App.ShellViewModel.FrameChanged += ShellViewModel_FrameChanged;
            App.ShellViewModel.FrameClosedRequested += ShellViewModel_FrameClosed;
        }

        private void ShellViewModel_FrameClosed(object sender, ShellView e) => OnExit();

        private void ShellViewModel_FrameChanged(object sender, ShellView e) => OnSuspend();
        
        private void ShellViewModel_FrameCreated(object sender, ShellView e)
        {
            if (e != null)
            {
                throw new InvalidOperationException("Incorrect usage");
            }

            ShellInstance = e;
            OnLoad();
            App.ShellViewModel.FrameCreated -= ShellViewModel_FrameCreated;
        }
    }
}
