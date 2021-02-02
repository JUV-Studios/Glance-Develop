using System;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DevelopManaged
{
    public sealed class ShellView
    {
        public string Title { get; set; }

        public string Caption
        {
            get
            {
                if (ReferenceSource == null) return string.Empty;
                return Path.GetDirectoryName(ReferenceSource.Path);
            }
        }

        public IconSource IconSource { get; set; }

        public UIElement Content { get; set; }

        public IStorageItem2 ReferenceSource { get; set; }

        public bool CanClose => Content is IDisposable;
        public override string ToString() => $"{Title}";
    }
}
