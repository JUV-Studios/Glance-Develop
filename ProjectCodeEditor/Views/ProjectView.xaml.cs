using System;
using Windows.Storage;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ProjectCodeEditor.Views
{
    public sealed partial class ProjectView : IDisposable
    {
        public ProjectView(StorageFolder folder)
        {
            WorkingFolder = folder;
            InitializeComponent();
        }

        public StorageFolder WorkingFolder { get; private set; }

        public void Dispose()
        {
        }
    }
}
