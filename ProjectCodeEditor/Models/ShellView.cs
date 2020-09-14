using Windows.UI.Xaml;

namespace ProjectCodeEditor.Models
{
    public sealed class ShellView
    {
        public string DisplayName { get; set; }

        public string Caption { get; set; }

        public object Parameter { get; set; }

        public UIElement Content { get; set; }
    }
}
