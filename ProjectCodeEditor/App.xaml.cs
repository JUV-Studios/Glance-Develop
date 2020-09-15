using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Services;
using ProjectCodeEditor.ViewModels;
using ProjectCodeEditor.Views;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

namespace ProjectCodeEditor
{
    public sealed partial class App : Application
    {
        private Lazy<ActivationService> _activationService;
        private ActivationService ActivationService
        {
            get { return _activationService.Value; }
        }

        public static SettingsViewModel AppSettings { get; } = new SettingsViewModel();
        public static EditorShellViewModel ShellViewModel { get; } = new EditorShellViewModel();

        public App()
        {
            InitializeComponent();

            EnteredBackground += App_EnteredBackground;
            Resuming += App_Resuming;
            AppCenter.Start("dd9a81de-fe79-4ab8-be96-8f96c346c88e", typeof(Analytics), typeof(Crashes));
            UnhandledException += OnAppUnhandledException;

            // Deferred execution until used. Check https://docs.microsoft.com/dotnet/api/system.lazy-1 for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (!args.PrelaunchActivated)
            {
                await ActivationService.ActivateAsync(args);
            }
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        protected override async void OnFileActivated(FileActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        private void OnAppUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // TODO WTS: Please log and handle the exception as appropriate to your scenario
            // For more info see https://docs.microsoft.com/uwp/api/windows.ui.xaml.application.unhandledexception

            if (e.Exception is InsufficientMemoryException || e.Exception is OutOfMemoryException)
            {
                GC.Collect();
            }
        }

        private ActivationService CreateActivationService()
        {
            return new ActivationService(this, typeof(EditorShellPage));
        }

        private async void App_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            var deferral = e.GetDeferral();
            Singleton<SuspendAndResumeService>.Instance.SaveStateAsync();
            deferral.Complete();
        }

        private void App_Resuming(object sender, object e)
        {
            Singleton<SuspendAndResumeService>.Instance.ResumeApp();
        }
    }
}
