using Microsoft.Toolkit.Uwp.Extensions;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Helpers;
using ProjectCodeEditor.ViewModels;
using Swordfish.NET.Collections.Auxiliary;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ProjectCodeEditor.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public readonly string DependenciesHeaderId = "DependenciesListBlock/Text";

        public SettingsPage()
        {
            InitializeComponent();
        }

        private void FontBox_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
        {
            args.Handled = true;
            string input = args.Text.Trim();
            var contains = App.AppSettings.InstalledFonts.Where(i => i.Trim() == input.Trim());
            if (!contains.IsEmpty()) sender.SelectedValue = args.Text.Trim();
        }

        private void SpecialThanksBlock_Loaded(object sender, RoutedEventArgs e)
        {
            var specialThanksBlock = sender as TextBlock;
            var link = new Hyperlink();
            link.Inlines.Add(new Run() { Text = "Adnan Umer" });
            link.NavigateUri = new("mailto:aztnan@outlook.com");
            AutomationProperties.SetName(link, $"{"SpecialThanksBlock/Text".GetLocalized()} Adnan Umer");
            specialThanksBlock.Inlines.Add(new Run() { Text = $"{"SpecialThanksBlock/Text".GetLocalized()} " });
            specialThanksBlock.Inlines.Add(link);
            specialThanksBlock.Loaded -= SpecialThanksBlock_Loaded;
        }

        private void Layout_Loaded(object sender, RoutedEventArgs e)
        {
            var section = sender as Hub;
            AccessibilityHelper.SetProperties(section);
            section.Loaded -= Layout_Loaded;
        }
    }
}
