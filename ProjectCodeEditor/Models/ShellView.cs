namespace ProjectCodeEditor.Models
{
    public sealed class ShellView
    {
        public string Title { get; set; }

        public string Caption { get; set; }

        public object Parameter { get; set; }

        public BaseLayout Content { get; set; }
    }
}
