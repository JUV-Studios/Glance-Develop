using System;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor.Models
{
    public sealed record ShellView(string Title, IconSource Icon, UIElement Content, IStorageItem2 ReferenceSource)
    {
        public bool CanClose => Content is IDisposable;
        public override string ToString() => $"{Title}";
    }
}
