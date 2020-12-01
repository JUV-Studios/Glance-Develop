using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ProjectCodeEditor.Helpers
{
    public class SmartTextForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? ElementTheme.Dark : ElementTheme.Light;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
