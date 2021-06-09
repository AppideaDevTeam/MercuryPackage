#if (UNITY_EDITOR || UNITY_IOS) && MERCURY_LOCALNOTIFICATIONS

using Unity.Notifications.iOS;

namespace Mercury.LocalNotifications
{
    internal class LocalNotificationManager_IOS : ILocalNotificationManager_Platform
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
                Data                         = string.IsNullOrEmpty(_notification.ChannelID) ? MercuryLibrarySO.LocalNotificationsDatabase.Channel_Default.ID : _notification.ChannelID,
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

        public string AppWasLaunchedViaNotificationChannel()
        {
            var intentData = iOSNotificationCenter.GetLastRespondedNotification();
            return intentData != null ? intentData.Data : "";
        }
        #endregion
    }
}

#endif