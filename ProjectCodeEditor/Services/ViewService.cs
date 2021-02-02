using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Uwp.Extensions;
using Microsoft.Toolkit.Uwp.UI.Helpers;
using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace ProjectCodeEditor.Services
{
    public sealed record KeyShortcutPressedEventArgs(KeyboardAccelerator Accelerator, AcceleratorKeyEventArgs SystemArgs);

    public enum AppViewMode : byte { Resizable, FullScreen, CompactOverlay }

    public static class ViewService
    {
        public interface IViewProperties : INotifyPropertyChanged
        {
            public FlowDirection FlowDirection { get; }

            public bool AppClosing { get; }
        }

        private class ViewServiceProperties : ObservableObject, IViewProperties
        {
            public FlowDirection FlowDirection
            {
                get
                {
                    // For bidirectional languages, determine flow direction for the root layout panel, and all contained UI.
                    var flowDirectionSetting = ResourceContext.GetForCurrentView().QualifierValues["LayoutDirection"];
                    if (flowDirectionSetting == "LTR") return FlowDirection.LeftToRight;
                    else return FlowDirection.RightToLeft;
                }
            }

            public bool AppClosing { get; set; } = false;
        }

        public static readonly DataTransferManager ShareCharm = DataTransferManager.GetForCurrentView();


        private static readonly ApplicationView ApplicationView = ApplicationView.GetForCurrentView();

        private static readonly CoreApplicationView ApplicationViewCore = CoreApplication.GetCurrentView();

        private static readonly SystemNavigationManagerPreview CloseManager = SystemNavigationManagerPreview.GetForCurrentView();

        public static bool CompactOverlaySupported => ApplicationView.IsViewModeSupported(ApplicationViewMode.CompactOverlay);

        private static readonly ViewServiceProperties _Properties = new ViewServiceProperties();

        public static IViewProperties Properties => _Properties;

        public static event EventHandler<KeyShortcutPressedEventArgs> KeyShortcutPressed;

        public static event EventHandler<AppViewMode> ViewModeChanged;

        /// <summary>
        /// Only use for saving data before close. Don't update the UI
        /// </summary>
        /// 
        public static readonly HashSet<Func<Task<bool>>> AppClosingEvent = new();

        public static readonly ThemeListener ThemeHandler = new();

        private static readonly ViewModePreferences CompactOverlayPreferences = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);

        public static void Initialize()
        {
            SetTitleBarProperties();
            Size minSize = new(500, 500);
            ApplicationView.SetPreferredMinSize(minSize);
            ThemeHandler.ThemeChanged += ThemeHandler_ThemeChanged;
            ApplicationViewCore.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            ApplicationViewCore.CoreWindow.SizeChanged += CoreWindow_SizeChanged;
            CloseManager.CloseRequested += CloseManager_CloseRequested;
            CompactOverlayPreferences.CustomSize = minSize;
            CompactOverlayPreferences.ViewSizePreference = ViewSizePreference.Custom;
        }

        private static void ThemeHandler_ThemeChanged(ThemeListener sender) => SetTitleBarProperties();

        private static void CoreWindow_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            args.Handled = true;
            RaiseViewModeChanged();
        }

        public static void RaiseViewModeChanged()
        {
            AppViewMode viewMode = AppViewMode.Resizable;
            if (ApplicationView.IsFullScreenMode) viewMode = AppViewMode.FullScreen;
            else if (ApplicationView.ViewMode == ApplicationViewMode.CompactOverlay) viewMode = AppViewMode.CompactOverlay;
            ViewModeChanged?.Invoke(null, viewMode);
        }

        public static void ShowShareCharm() => DataTransferManager.ShowShareUI();

        private static async void CloseManager_CloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            e.Handled = true;
            var deferral = e.GetDeferral();
            _Properties.AppClosing = true;
            if (AppClosingEvent.Count > 0)
            {
                bool sendNotification = false;
                foreach (var func in AppClosingEvent)
                {
                    if (await func())
                    {
                        sendNotification = true;
                        break;
                    }
                }

                AppClosingEvent.Clear();
                if (sendNotification) NotificationHelper.SendBasicNotification("FileSaveNotificationTitle".GetLocalized(), "MultipleFileSaveNotificationContent".GetLocalized());
            }

            if (await ApplicationView.TryConsolidateAsync()) deferral.Complete();
        }

        private static void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.EventType.ToString().Contains("Down"))
            {
                VirtualKeyModifiers modifier;
                var ctrl = ApplicationViewCore.CoreWindow.GetKeyState(VirtualKey.Control);
                var alt = ApplicationViewCore.CoreWindow.GetKeyState(VirtualKey.Menu);
                var shift = ApplicationViewCore.CoreWindow.GetKeyState(VirtualKey.Shift);
                if (ctrl.HasFlag(CoreVirtualKeyStates.Down)) modifier = VirtualKeyModifiers.Control;
                else if (alt.HasFlag(CoreVirtualKeyStates.Down)) modifier = VirtualKeyModifiers.Menu;
                else if (shift.HasFlag(CoreVirtualKeyStates.Down)) modifier = VirtualKeyModifiers.Shift;
                else modifier = VirtualKeyModifiers.None;
                KeyShortcutPressed?.Invoke(null, new(new KeyboardAccelerator()
                {
                    IsEnabled = !Preferences.AppSettings.DialogShown,
                    Key = args.VirtualKey,
                    Modifiers = modifier
                }, args));
            }
        }

        private static void SetTitleBarProperties()
        {
            // Use default options
            var uiSettings = Singleton<UISettings>.Instance;
            ApplicationViewCore.TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationView.TitleBar.BackgroundColor = uiSettings.GetColorValue(UIColorType.Background);
            ApplicationView.TitleBar.ForegroundColor = uiSettings.GetColorValue(UIColorType.Foreground);
            ApplicationView.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            ApplicationView.TitleBar.ButtonForegroundColor = ApplicationView.TitleBar.ForegroundColor;
            ApplicationView.TitleBar.ButtonInactiveBackgroundColor = null;
            ApplicationView.TitleBar.ButtonInactiveForegroundColor = null;
            ApplicationView.TitleBar.ButtonHoverBackgroundColor = null;
            ApplicationView.TitleBar.ButtonHoverForegroundColor = null;
            ApplicationView.TitleBar.ButtonPressedBackgroundColor = null;
            ApplicationView.TitleBar.ButtonPressedForegroundColor = null;
        }
    }
}
