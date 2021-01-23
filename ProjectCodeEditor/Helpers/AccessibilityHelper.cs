using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using WinRTXamlToolkit.Controls.Extensions;

namespace ProjectCodeEditor.Helpers
{
    public static class AccessibilityHelper
    {
        private static readonly Dictionary<ListView, (MenuFlyout, Func<object, bool>)> ContextedListViews = new();

        public static void SetProperties(Hub hub)
        {
            for (int i = 0; i < hub.Sections.Count; i++)
            {
                var section = hub.Sections[i];
                AutomationProperties.SetPositionInSet(section, i + 1);
                AutomationProperties.SetSizeOfSet(section, hub.Sections.Count);
            }
        }

        /// <summary>
        /// Attaches a context menu with a list view
        /// </summary>
        /// <remarks>Important: Call DetachContextMenu after the list view is removed from the screen to clean up unused memory</remarks>
        /// <param name="listView">The list view that will be handled for item right click</param>
        /// <param name="flyout">The flyout to show when the user right clicks</param>
        /// <param name="shouldShow">A pointer to the function which decides whether to show a context menu or not</param>

        public static void AttachContextMenu(ListView listView, MenuFlyout flyout, Func<object, bool> shouldShow)
        {
            if (!ContextedListViews.ContainsKey(listView))
            {
                ContextedListViews.Add(listView, new(flyout, shouldShow));
                listView.ContextRequested += ListView_ContextRequested;
            }
        }

        private static void ListView_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            args.Handled = true;
            var val = ContextedListViews[sender as ListView];
            ListViewItem target = null;
            object param = null;

            // Handle keyboard context menu
            if (args.OriginalSource is ListViewItem item)
            {
                target = item;
                param = target.Content;
            }
            // Handle mouse and touch context menu
            else if (args.OriginalSource is FrameworkElement element) param = element.DataContext;
            if (val.Item2(param))
            {
                if (args.TryGetPosition(sender, out Point location)) val.Item1.ShowAt(sender, location);
                else val.Item1.ShowAt(target);
            }
        }

        public static void DetachContextMenu(ListView listView)
        {
            if (ContextedListViews.Remove(listView)) listView.ContextRequested -= ListView_ContextRequested;
        }
    }
}
