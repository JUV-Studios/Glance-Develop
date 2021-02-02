using Microsoft.Toolkit.Mvvm.ComponentModel;
using Windows.UI.Xaml;

namespace ProjectCodeEditor.Models
{
    public interface IViewPage
    {
        ObservableObject ViewModel { get; }

        UIElement ViewContent { get; }

        bool Standalone { get; }
    }
}
