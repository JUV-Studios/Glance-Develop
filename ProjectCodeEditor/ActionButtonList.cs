using ProjectCodeEditor.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor
{
    public sealed class ActionButtonList : ListView
    {
        public ActionButtonList()
        {
            Style = App.Current.Resources["ActionButtonListStyle"] as Style;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            FrameworkElement source = element as FrameworkElement;
            var context = item as ActionOption;
            ToolTipService.SetToolTip(source, context.ToolTipContent);
            if (!string.IsNullOrWhiteSpace(context.AccessKey)) AutomationProperties.SetAcceleratorKey(source, context.AccessKey);
        }
    }
}
