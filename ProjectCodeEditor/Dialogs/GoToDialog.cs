using Microsoft.Toolkit.Uwp.Extensions;
using Microsoft.UI.Xaml.Controls;
using ProjectCodeEditor.Services;
using System;
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

        internal static readonly CheckBox ExtendCheckBox = new()
        {
            Content = "GoToExtendCheckBox/Content".GetLocalized()
        };

        static GoToDialog()
        {
            ExtendCheckBox.Click += (s, e) =>
            {
                Preferences.LocalSettings.Values["ExtendToGo"] = ExtendCheckBox.IsChecked;
            };

            var layout = new StackPanel() { Spacing = 4 };
            layout.Children.Add(LineBox);
            layout.Children.Add(ExtendCheckBox);
            if (Preferences.LocalSettings.Values.TryGetValue("ExtendGoTo", out object value)) ExtendCheckBox.IsChecked = Convert.ToBoolean(value);
            DialogRef = new ContentDialog()
            {
                Title = "GoToOption/Title".GetLocalized(),
                PrimaryButtonText = "GoToDialog/PrimaryButtonText".GetLocalized(),
                SecondaryButtonText = "CancelText".GetLocalized(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                DefaultButton = ContentDialogButton.Primary,
                Content = layout
            };
        }
    }
}
