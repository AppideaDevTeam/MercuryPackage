#if MERCURY_LOCALNOTIFICATIONS

namespace Mercury.LocalNotifications
{
    internal interface ILocalNotificationsManager_Platform
    {
        void Initialize();
        void CancelAllNotifications();
        void ScheduleNotification(LocalNotification _notification);
        string AppWasLaunchedViaNotificationChannel();
    }
}
#endif