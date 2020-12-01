using System;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor.Models
{
    public sealed record ActionOption(string Title, string Description, IconSource Icon, Action ActionCommand, string AccessKey)
    {
        public override string ToString() => $"{Title}, {Description}, {AccessKey}";
    }
}
