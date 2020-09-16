using Microsoft.Toolkit.Uwp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCodeEditor.Models
{
    public class EditorFontSize
    {
        public string DisplayName
        {
            get
            {
                switch (FontSizeEnum)
                {
                    case EditorFontSizes.Small: return "SettingsFontSizeSmall".GetLocalized();
                    case EditorFontSizes.Large: return "SettingsFontSizeLarge".GetLocalized();
                    default: return "SettingsFontSizeMedium".GetLocalized();                
                }
            }
        }

        public EditorFontSizes FontSizeEnum { get; set; }

        public double FontSize
        {
            get
            {
                switch (FontSizeEnum)
                {
                    case EditorFontSizes.Small: return Convert.ToDouble(App.Current.Resources["SmallFontSize"]);
                    case EditorFontSizes.Large: return Convert.ToDouble(App.Current.Resources["LargeFontSize"]);
                    default: return Convert.ToDouble(App.Current.Resources["MediumFontSize"]);
                }
            }
        }

        public string PropertyName
        {
            get
            {
                switch (FontSizeEnum)
                {
                    case EditorFontSizes.Small: return "Small";
                    case EditorFontSizes.Large: return "Large";
                    default: return "Medium";
                }
            }
        }
    }

    public enum EditorFontSizes : byte
    {
        Small, Medium, Large
    }
}
