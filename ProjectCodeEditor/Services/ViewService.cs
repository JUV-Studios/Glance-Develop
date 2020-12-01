using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Uwp.UI.Helpers;
using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.ViewModels;
using Swordfish.NET.Collections.Auxiliary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ProjectCodeEditor.Services
{
    public sealed record KeyShortcutPressedEventArgs(KeyboardAccelerator Accelerator, AcceleratorKeyEventArgs SystemArgs);

    public enum AppViewMode : byte { Resizable, FullScreen, CompactOverlay }

    public static class ViewService
    {
        public interface IViewProperties : INotifyPropertyChanged
        {
            public AppBarClosedDisplayMode RecommendedAppBarMode { get; set; }

            public string ViewTitle { get; set; }

            public Thickness RecommendedPageMargin { get; set; }

            public bool AppClosing { get; set; }
        }

        private class ViewServiceProperties : ObservableObject, IViewProperties
        {
            private AppBarClosedDisplayMode _RecommendedAppBarMode = AppBarClosedDisplayMode.Compact;

            public AppBarClosedDisplayMode RecommendedAppBarMode
            {
                get => _RecommendedAppBarMode;
                set => SetProperty(ref _RecommendedAppBarMode, value);
            }

            public string ViewTitle
            {
                get => ApplicationView.Title;
                set
                {
                    ApplicationView.Title = value;
                    OnPropertyChanged(nameof(ViewTitle));
                }
            }

            private Thickness _RecommendedPageMargin;

            public Thickness RecommendedPageMargin
            {
                get => _RecommendedPageMargin;
                set
                {
                    SetProperty(ref _RecommendedPageMargin, value);
                }
            }

            public bool AppClosing { get; set; } = false;
        }

        private static readonly ApplicationView ApplicationView = ApplicationView.GetForCurrentView();

        private static readonly CoreApplicationView ApplicationViewCore = CoreApplication.GetCurrentView();

        private static readonly SystemNavigationManagerPreview CloseManager = SystemNavigationManagerPreview.GetForCurrentView();

        public static bool CompactOverlaySupported => ApplicationView.IsViewModeSupported(ApplicationViewMode.CompactOverlay);

        public static readonly IViewProperties Properties = new ViewServiceProperties();

        public static event EventHandler<KeyShortcutPressedEventArgs> KeyShortcutPressed;

        public static event EventHandler<AppViewMode> ViewModeChanged;

        /// <summary>
        /// Only use for saving data before close. Don't update the UI
        /// </summary>
        /// 
        public static readonly List<Action> AppClosingEvent = new();

        private static readonly ViewModePreferences CompactOverlayPreferences = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);

        private static readonly ThemeListener ThemeManager = new();

        public static void Initialize()
        {
            SetTitleBarProperties();
            SetPageMargin();
            ApplicationViewCore.TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
            ApplicationViewCore.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            ThemeManager.ThemeChanged += Instance_ThemeChanged;
            ApplicationViewCore.CoreWindow.SizeChanged += CoreWindow_SizeChanged;
            CloseManager.CloseRequested += CloseManager_CloseRequested;
            CompactOverlayPreferences.CustomSize = new Size(500, 500);
            CompactOverlayPreferences.ViewSizePreference = ViewSizePreference.Custom;
        }

        private static void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args) => SetTitleBarProperties();

        public static async Task CloseView() => await ApplicationView.TryConsolidateAsync();

        private static void CoreWindow_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            args.Handled = true;
            SetPageMargin();
            RaiseViewModeChanged();
        }

        public static void RaiseViewModeChanged()
        {
            AppViewMode viewMode = AppViewMode.Resizable;
            if (ApplicationView.IsFullScreenMode) viewMode = AppViewMode.FullScreen;
            else if (ApplicationView.ViewMode == ApplicationViewMode.CompactOverlay) viewMode = AppViewMode.CompactOverlay;
            if (viewMode == AppViewMode.CompactOverlay)
            {
                Properties.RecommendedAppBarMode = AppBarClosedDisplayMode.Minimal;
            }
            else
            {
                Properties.RecommendedAppBarMode = AppBarClosedDisplayMode.Compact;
            }
            ViewModeChanged?.Invoke(null, viewMode);
        }

        private static async void CloseManager_CloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            e.Handled = true;
            var deferral = e.GetDeferral();
            Properties.AppClosing = true;
            if (!AppClosingEvent.IsEmpty()) foreach (var func in AppClosingEvent) func();
            AppClosingEvent.Clear();
            await CloseView();
            deferral.Complete();
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
                else modifier = VirtualKeyModifiers.Windows;
                KeyShortcutPressed?.Invoke(null, new(new KeyboardAccelerator()
                {
                    IsEnabled = !Singleton<SettingsViewModel>.Instance.DialogShown,
                    Key = args.VirtualKey,
                    Modifiers = modifier
                }, args));
            }
        }

        private static void Instance_ThemeChanged(ThemeListener sender) { SetTitleBarProperties(); }

        private static void SetTitleBarProperties()
        {
            ApplicationViewCore.TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationView.TitleBar.BackgroundColor = Singleton<UISettings>.Instance.GetColorValue(UIColorType.Background);
            ApplicationView.TitleBar.ForegroundColor = Singleton<UISettings>.Instance.GetColorValue(UIColorType.Foreground);
            ApplicationView.TitleBar.ButtonBackgroundColor = Colors.Transparent;
        }

        private static void SetPageMargin()
        {
            var size = ApplicationViewCore.CoreWindow.Bounds;
            if (size.Width < 641) Properties.RecommendedPageMargin = new(12, 12, 12, 0);
            else if (size.Width < 1008) Properties.RecommendedPageMargin = new(24, 24, 24, 0);
            else Properties.RecommendedPageMargin = new(36, 36, 36, 0);
        }

        public static void ToggleFullScreen()
        {
            if (ApplicationView.IsFullScreenMode) ApplicationView.ExitFullScreenMode();
            else ApplicationView.TryEnterFullScreenMode();
        }

        public static async void ToggleCompactOverlay()
        {
            if (ApplicationView.ViewMode == ApplicationViewMode.Default) await ApplicationView.TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, CompactOverlayPreferences);
            else await ApplicationView.TryEnterViewModeAsync(ApplicationViewMode.Default);
        }
    }
}
