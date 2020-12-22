using Microsoft.UI.Xaml.Controls;
using System;
using Windows.UI.Xaml;

namespace ProjectCodeEditor.Models
{
    public sealed record ShellView(string Title, string Caption, IconSource Icon, UIElement Content)
    {
        public bool CanClose => Content is IDisposable;
        public override string ToString() => $"{Title}, {Caption}";
    }
}
