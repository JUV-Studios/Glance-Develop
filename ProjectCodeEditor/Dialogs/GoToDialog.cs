using Microsoft.Toolkit.Uwp.Extensions;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor.Dialogs
{
    internal static class GoToDialog
    {
        internal static readonly ContentDialog DialogRef;

        internal static readonly NumberBox LineBox = new()
        {
            Minimum = 1,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
            AcceptsExpression = true,
            Header = "GoToLineBox/Header".GetLocalized(),
        };

        static GoToDialog()
        {
            DialogRef = new ContentDialog()
            {
                Title = "GoToOption/Title".GetLocalized(),
                PrimaryButtonText = "GoToDialog/PrimaryButtonText".GetLocalized(),
                SecondaryButtonText = "CancelText".GetLocalized(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                DefaultButton = ContentDialogButton.Primary,
                Content = LineBox
            };
        }
    }
}
