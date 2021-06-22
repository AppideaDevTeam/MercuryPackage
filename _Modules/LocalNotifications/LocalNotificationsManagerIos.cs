#if (UNITY_EDITOR || UNITY_IOS) && MERCURY_LOCALNOTIFICATIONS

using Unity.Notifications.iOS;

namespace Mercury.LocalNotifications
{
    internal class LocalNotificationsManagerIos : ILocalNotificationsManager_Platform
    {
        #region IMPLEMENTATIONS
        public void Initialize() { }

        public void CancelAllNotifications()
        {
            iOSNotificationCenter.RemoveAllDeliveredNotifications();
            iOSNotificationCenter.RemoveAllScheduledNotifications();
        }

        public void ScheduleNotification(LocalNotification _notification)
        {
            var notification = new iOSNotification()
            {
                Title                        = _notification.Title,
                Subtitle                     = "",
                Body                         = _notification.Text,
                Data                         = _notification.Data,
                CategoryIdentifier           = _notification.ChannelID,
                ThreadIdentifier             = _notification.ChannelID,
                ShowInForeground             = true,
                ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,

                Trigger = new iOSNotificationTimeIntervalTrigger()
                {
                    TimeInterval = _notification.FireTimeDelay,
                    Repeats      = false
                }
            };
            
            iOSNotificationCenter.ScheduleNotification(notification);
        }

        public bool ApplicationLaunchedViaNotification(out string data)
        {
            var intentData = iOSNotificationCenter.GetLastRespondedNotification();

            if (intentData != null && !string.IsNullOrEmpty(intentData.Data))
            {
                data = intentData.Data;

                return true;
            }

            data = string.Empty;

            return false;
        }
        #endregion
    }
}

#endif        