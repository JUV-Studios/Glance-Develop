/*using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using JUVStudios;

namespace TextEditor.UI
{
    public static class GoToDialog
    {
        private static readonly ContentDialog DialogRef;

        public static ContentDialog DisplayDialog => DialogRef;

        private static readonly NumberBox _LineBox = new()
        {
            Minimum = 1,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
            AcceptsExpression = true,
            Header = ResourceController.GetTranslation("GoToLineBox/Header"),
        };

        public static NumberBox LineBox => _LineBox;

        private static readonly CheckBox ExtendCheckBox = new()
        {
            Content = ResourceController.GetTranslation("GoToExtendCheckBox/Content")
        };

        static GoToDialog()
        {
            ExtendCheckBox.Click += (s, e) =>
            {
                ApplicationData.Current.LocalSettings.Values["ExtendToGo"] = ExtendCheckBox.IsChecked;
            };

            var layout = new StackPanel() { Spacing = 4 };
            layout.Children.Add(LineBox);
            layout.Children.Add(ExtendCheckBox);
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue("ExtendGoTo", out object value)) ExtendCheckBox.IsChecked = Convert.ToBoolean(value);
            DialogRef = new ContentDialog()
            {
                Title = ResourceController.GetTranslation("GoToOption/Title"),
                PrimaryButtonText = ResourceController.GetTranslation("GoToDialog/PrimaryButtonText"),
                SecondaryButtonText = "CancelText",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                DefaultButton = ContentDialogButton.Primary,
                Content = layout
            };
        }
    }
} */