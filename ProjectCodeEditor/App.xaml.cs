using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Helpers;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.Services;
using ProjectCodeEditor.ViewModels;
using ProjectCodeEditor.Views;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace ProjectCodeEditor
{
    public sealed partial class App : Application
    {
#if DEBUG
        private readonly Stopwatch LaunchStopwatch = new();
#endif
        private bool AlreadyActivated = false;

        public static readonly string CancelStringId = "CancelText";

        public static User CurrentUser;

        public static readonly SettingsViewModel AppSettings = Singleton<SettingsViewModel>.Instance;

        public App()
        {
#if DEBUG
            LaunchStopwatch.Start();
#endif
            InitializeComponent();
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Maximized;
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args) => await ActivateAsync(args);

        protected override async void OnActivated(IActivatedEventArgs args) => await ActivateAsync(args);

        protected override async void OnFileActivated(FileActivatedEventArgs args) => await ActivateAsync(args);

        public async Task ActivateAsync(object activationArgs)
        {
            Frame frame = null;
            ShellViewModel viewModel = Singleton<ShellViewModel>.Instance;
            if (activationArgs is IActivatedEventArgs activation)
            {
                if (!AlreadyActivated)
                {
                    AlreadyActivated = true;
                    if (!AppCenter.Configured)
                    {
                        AppCenter.SetUserId(await AppSettings.UniqueUserId());
                        AppCenter.Start("dd9a81de-fe79-4ab8-be96-8f96c346c88e", typeof(Analytics), typeof(Crashes));
                    }

                    // Initialize services that you need before app activation
                    // take into account that the splash screen is shown while this code runs.
                    await Singleton<RecentsViewModel>.Instance.LoadRecentsAsync();
                    await JumpListHelper.InitializeAsync();
                    ViewService.Initialize();
                }

                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (Window.Current.Content == null)
                {
                    // Create a Shell or Frame to act as the navigation context
                    frame = new Frame();
                    Window.Current.Content = frame;
                    frame.NavigationFailed += (sender, e) => throw e.Exception;
                }
                else frame = Window.Current.Content as Frame;

                string arguments = null;
                if (activationArgs is LaunchActivatedEventArgs launchArgs)
                {
                    arguments = launchArgs.Arguments;
                    CurrentUser = launchArgs.User;
                }

                frame.Navigate(typeof(MainPage), arguments, new DrillInNavigationTransitionInfo());

                // Ensure the current window is active
                Window.Current.Activate();

                try
                {
                    if (viewModel.SelectedIndex != 0 && string.IsNullOrEmpty(arguments)) viewModel.SelectedIndex = 0;
                }
                catch (ArgumentException) { }

                if (activationArgs is FileActivatedEventArgs fileActivationArgs)
                {
                    var filesArray = new StorageFile[fileActivationArgs.Files.Count];
                    for (int i = 0; i < fileActivationArgs.Files.Count; i++)
                    {
                        if (fileActivationArgs.Files[i].IsOfType(StorageItemTypes.File)) filesArray[i] = fileActivationArgs.Files[i] as StorageFile;
                    }

                    Interactions.AddFiles(filesArray);
                }

                if (arguments == "OpenFiles") Interactions.OpenFiles();
                else if (arguments == "NewFiles") Interactions.NewFile();

                if (!AlreadyActivated)
                {
#if DEBUG
                    LaunchStopwatch.Stop();
                    Debug.WriteLine($"Develop took {LaunchStopwatch.ElapsedMilliseconds} ms to launch");
#endif
                }
            }
        }
    }

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

namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}
