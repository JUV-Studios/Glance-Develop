using System;
using ProjectCodeEditor.ViewModels;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor.Views
{
    public sealed partial class TextModeBrowserPage : BaseLayout
    {
        public TextModeBrowserViewModel ViewModel { get; } = new TextModeBrowserViewModel();

        public TextModeBrowserPage()
        {
            InitializeComponent();
            Loaded += BaseLayout_Loaded;
        }

        protected override void OnLoad()
        {
        }

        protected override void OnXamlLoad()
        {
            Loaded -= BaseLayout_Loaded;
        }

        public override void OnResume()
        {
        }

        public override void Dispose()
        {
        }

        public override void OnSuspend()
        {

        }
    }
}
