using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using ProjectCodeEditor.Helpers;
using System;
using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage.Search;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.UI.Core;
using Swordfish.NET.Collections.Auxiliary;
using Windows.System;
using Windows.ApplicationModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;

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
            ShellView view = null;
            Interactions.StorageItemAlreadyOpen(WorkingFolder, ref view);
            Singleton<ShellViewModel>.Instance.RemoveInstance(view);
        }       
    }
}
