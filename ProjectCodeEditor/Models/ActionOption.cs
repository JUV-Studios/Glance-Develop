using System;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor.Models
{
    public sealed record ActionOption(string Title, string Description, IconSource Icon, Action ActionCommand, string AccessKey = null)
    {
        public string ToolTipContent => $"{Title} ({AccessKey})";
        public override string ToString() => $"{Title}, {Description}";
    }
}
