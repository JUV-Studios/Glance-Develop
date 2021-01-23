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
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using WinRTXamlToolkit.IO.Extensions;

namespace ProjectCodeEditor
{
    public sealed partial class App : Application
    {
#if DEBUG
        private readonly Stopwatch LaunchStopwatch = new();
#endif
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

        protected override void OnLaunched(LaunchActivatedEventArgs args) => ActivateAsync(args).ConfigureAwait(false);

        protected override void OnActivated(IActivatedEventArgs args) => ActivateAsync(args).ConfigureAwait(false);

        protected override void OnFileActivated(FileActivatedEventArgs args) => ActivateAsync(args).ConfigureAwait(false);

        public async Task ActivateAsync(object activationArgs)
        {
            bool firstLaunch = false;
            ShellViewModel viewModel = Singleton<ShellViewModel>.Instance;
            if (activationArgs is IActivatedEventArgs activation)
            {
                if (!AppCenter.Configured)
                {
                    // Initialize services that you need before app activation
                    // take into account that the splash screen is shown while this code runs.
                    firstLaunch = true;
                    AppCenter.SetUserId(await AppSettings.UniqueUserIdAsync());
                    AppCenter.Start("dd9a81de-fe79-4ab8-be96-8f96c346c88e", typeof(Analytics), typeof(Crashes));
                    await JumpListHelper.InitializeAsync();
                    await Singleton<RecentsViewModel>.Instance.LoadRecentsAsync();
                    await ApplicationData.Current.TemporaryFolder.DeleteFilesAsync(true);
                    ViewService.Initialize();
                }

                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (Window.Current.Content == null) Window.Current.Content = new MainPage();

                // Ensure the current window is active
                Window.Current.Activate();
                string arguments = null;
                if (activationArgs is LaunchActivatedEventArgs launchArgs)
                {
                    arguments = launchArgs.Arguments;
                    CurrentUser = launchArgs.User;
                }
                if (arguments == "OpenFiles") Interactions.OpenFiles();
                else if (arguments == "NewFiles") Interactions.NewFile();
                if (activationArgs is FileActivatedEventArgs fileActivationArgs)
                {
                    var filesArray = new StorageFile[fileActivationArgs.Files.Count];
                    for (int i = 0; i < fileActivationArgs.Files.Count; i++)
                    {
                        if (fileActivationArgs.Files[i].IsOfType(StorageItemTypes.File)) filesArray[i] = fileActivationArgs.Files[i] as StorageFile;
                    }

                    Interactions.AddFiles(filesArray);
                }

                if (firstLaunch)
                {
#if DEBUG
                    LaunchStopwatch.Stop();
                    Debug.WriteLine($"Develop took {LaunchStopwatch.ElapsedMilliseconds} ms to launch");
#endif
                }
            }
        }
    }

    /// <summary>
    /// Represent a Metro style vertical options list binded to an immutable vector
    /// </summary>
    public sealed class ActionButtonList : ListView
    {
        public ActionButtonList()
        {
            Style = App.Current.Resources["ActionButtonListStyle"] as Style;
            ItemClick += ActionButtonList_ItemClick;
        }

        public event Func<ActionOption, bool> ShouldRunCommand;

        private void ActionButtonList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ActionOption option)
            {
                if (option.IsEnabled)
                {
                    if (ShouldRunCommand != null)
                    {
                        if (!ShouldRunCommand.Invoke(option)) return;
                    }

                    if (option.Command != null) option.Command();
                    else option.RaiseClick();
                }
            }
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            FrameworkElement source = element as FrameworkElement;
            var context = item as ActionOption;
            TextBlock toolTipBlock = new();
            toolTipBlock.SetBinding(TextBlock.TextProperty, new Binding()
            {
                Source = context,
                Mode = BindingMode.OneWay,
                Path = new PropertyPath("ToolTipContent"),
            });

            ToolTipService.SetToolTip(source, toolTipBlock);

            source.SetBinding(IsEnabledProperty, new Binding()
            {
                Source = context,
                Path = new PropertyPath("IsEnabled"),
                Mode = BindingMode.OneWay
            });

            source.SetBinding(AutomationProperties.AcceleratorKeyProperty, new Binding()
            {
                Source = context,
                Path = new PropertyPath("AccessKey"),
                Mode = BindingMode.OneWay,
                FallbackValue = string.Empty
            });
        }
    }
}

namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}
