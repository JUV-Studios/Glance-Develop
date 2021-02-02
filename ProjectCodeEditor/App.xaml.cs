using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using ProjectCodeEditor.Helpers;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.Services;
using ProjectCodeEditor.ViewModels;
using ProjectCodeEditor.Views;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace ProjectCodeEditor
{
    public sealed partial class App : Application
    {
#if DEBUG
        private readonly Stopwatch LaunchStopwatch = new();
#endif
        public static readonly string CancelStringId = "CancelText";

        public static User CurrentUser;

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
            if (!AppCenter.Configured)
            {
                // Initialize services while the splash screen is displayed
                firstLaunch = true;
                AppCenter.SetUserId(await Preferences.AppSettings.UniqueUserIdAsync());
                AppCenter.Start("dd9a81de-fe79-4ab8-be96-8f96c346c88e", typeof(Analytics), typeof(Crashes));
                await JumpListHelper.InitializeAsync();
                await RecentsViewModel.LoadRecentsAsync();
                // await ApplicationData.Current.TemporaryFolder.DeleteFilesAsync(true);
                ViewService.Initialize();
            }

            // Do not repeat app initialization when the window already has content, just ensure that the window is active
            if (Window.Current.Content == null) Window.Current.Content = new MainPage();
            Window.Current.Activate();
            string arguments = null;
            if (activationArgs is LaunchActivatedEventArgs launchArgs)
            {
                arguments = launchArgs.Arguments;
                CurrentUser = launchArgs.User;
            }
            else if (activationArgs is FileActivatedEventArgs fileActivationArgs)
            {
                Interactions.AddStorageItems(fileActivationArgs.Files.Where(item => item.IsOfType(StorageItemTypes.File)).Select(item => item as IStorageItem2).ToArray());
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

    /// <summary>
    /// Represent a Metro style vertical options list bound to an immutable vector
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
