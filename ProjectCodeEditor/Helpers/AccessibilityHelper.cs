using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor.Helpers
{
    public static class AccessibilityHelper
    {
        public static void SetProperties(Hub hub)
        {
            for (int i = 0; i < hub.Sections.Count; i++)
            {
                var section = hub.Sections[i];
                AutomationProperties.SetPositionInSet(section, i + 1);
                AutomationProperties.SetSizeOfSet(section, hub.Sections.Count);
            }
        }
    }
}
