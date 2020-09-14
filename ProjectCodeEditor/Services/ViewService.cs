using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace ProjectCodeEditor.Services
{
    public static class ViewService
    {
        public static ApplicationView applicationView;
        public static CoreApplicationView applicationViewCore;
        public static event TypedEventHandler<CoreApplicationViewTitleBar, object> LayoutChanged;

        public static void Initialize()
        {
            applicationView = ApplicationView.GetForCurrentView();
            applicationViewCore = CoreApplication.GetCurrentView();
            applicationView.TitleBar.ButtonForegroundColor = Colors.Transparent;
            applicationView.TitleBar.ButtonBackgroundColor = new UISettings().GetColorValue(UIColorType.Background);
        }

        public static void SetTitle(string title)
        {
            if (title != null)
            {
                applicationView.Title = title;
            }
        }
    }
}
