using Microsoft.Toolkit.Uwp.Notifications;
using System;
using Windows.UI.Notifications;

namespace ProjectCodeEditor.Helpers
{
    public static class NotificationHelper
    {
        private static readonly ToastNotifier Notifier = ToastNotificationManager.CreateToastNotifier();

        /// <summary>
        /// Send a basic notification with title and a message
        /// </summary>
        /// <param name="title">The title of the notification</param>
        /// <param name="message">A short and sweet message shown to the user</param>
        /// <returns>Returns true if the notification was sent, else false</returns>
        /// <remarks>Throws InvalidOperationException if the specified title or message is not valid</remarks>
        public static bool SendBasicNotification(string title, string message)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message)) throw new InvalidOperationException();

            if (Notifier.Setting != NotificationSetting.Enabled) return false;

            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText() { Text = title }, new AdaptiveText() { Text = message }
                        }
                    }
                }
            };

            // Create the toast notification
            var toastNotif = new ToastNotification(toastContent.GetXml());

            // And send the notification
            Notifier.Show(toastNotif);
            return true;
        }
    }
}
