using Microsoft.UI.Xaml.Controls;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DevelopManaged.Dialogs
{
    internal static class GoToDialog
    {
        internal static readonly ContentDialog DialogRef;

        internal static readonly NumberBox LineBox = new()
        {
            Minimum = 1,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
            AcceptsExpression = true,
            Header = "GoToLineBox/Header",
        };

        internal static readonly CheckBox ExtendCheckBox = new()
        {
            Content = "GoToExtendCheckBox/Content"
        };

        static GoToDialog()
        {
            ExtendCheckBox.Click += (s, e) =>
            {
                SettingsViewModel.LocalSettings.Values["ExtendToGo"] = ExtendCheckBox.IsChecked;
            };

            var layout = new StackPanel() { Spacing = 4 };
            layout.Children.Add(LineBox);
            layout.Children.Add(ExtendCheckBox);
            if (SettingsViewModel.LocalSettings.Values.TryGetValue("ExtendGoTo", out object value)) ExtendCheckBox.IsChecked = Convert.ToBoolean(value);
            DialogRef = new ContentDialog()
            {
                Title = "GoToOption/Title",
                PrimaryButtonText = "GoToDialog/PrimaryButtonText",
                SecondaryButtonText = "CancelText",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                DefaultButton = ContentDialogButton.Primary,
                Content = layout
            };
        }
    }
}