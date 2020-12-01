using Microsoft.Toolkit.Uwp.Extensions;
using Windows.UI.Xaml;

namespace ProjectCodeEditor.Models
{
    public sealed class ShellView
    {
        public UIElement Content { get; set; }

        public string Title { get; init; }

        public string Caption { get; init; }

        public override string ToString() => $"{Title}, {Caption}, {"TabItemAutomation".GetLocalized()}";
    }
}
