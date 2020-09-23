using ProjectCodeEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ProjectCodeEditor
{
    public interface ILayoutView : IDisposable
    {
        public UIElement GetUserInterface();

        public void Initialize(ShellView e);

        public void OnTabAdded();

        public void OnTabRemoveRequested();

        public void SaveState();

        public void RestoreState();
    }
}
