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

        private void SpecialThanksBlock_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Layout_Loaded(object sender, RoutedEventArgs e)
        {
            var section = sender as Hub;
            AccessibilityHelper.SetProperties(section);
            section.Loaded -= Layout_Loaded;
        }
    }
}
