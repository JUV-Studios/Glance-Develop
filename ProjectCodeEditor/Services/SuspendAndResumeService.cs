using ProjectCodeEditor.Activation;
using ProjectCodeEditor.Views;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

namespace ProjectCodeEditor.Services
{
    // The SuspendAndResumeService allows you to save the App data before the App is being suspended (or enters in background state).
    // In case the App is terminated during suspension, the data is restored during App launch by this ActivationHandler.
    // In case the App is resumed without being terminated no data should be lost, a resume event is fired that allows you to refresh App data that might
    // be outdated (e.g data from online feeds)
    // Documentation:
    //     * How to implement and test: https://github.com/Microsoft/WindowsTemplateStudio/blob/release/docs/UWP/features/suspend-and-resume.md
    //     * Application Lifecycle: https://docs.microsoft.com/windows/uwp/launch-resume/app-lifecycle
    internal class SuspendAndResumeService : ActivationHandler<LaunchActivatedEventArgs>
    {
        public bool SaveStateAsync()
        {
            App.ShellViewModel.SelectedItem.Content.SaveState();
            return true;
        }

        // This method allows subscribers to refresh data that might be outdated when the App is resuming from suspension.
        // If the App was terminated during suspension this event will not fire, data restore is handled by the method HandleInternalAsync.
        public void ResumeApp()
        {
            App.ShellViewModel.SelectedItem.Content.RestoreState();
        }

        // This method restores application state when the App is launched after termination, it navigates to the stored Page passing the recovered state data.

        protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
        {
            // Application State must only be restored if the App was terminated during suspension.
            return args.PreviousExecutionState == ApplicationExecutionState.Terminated;
        }

        protected override Task HandleInternalAsync(LaunchActivatedEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
