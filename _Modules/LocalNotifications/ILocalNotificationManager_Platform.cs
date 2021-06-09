#if MERCURY_LOCALNOTIFICATIONS

namespace Mercury.LocalNotifications
{
    internal interface ILocalNotificationManager_Platform
    {
        void Initialize();
        void CancelAllNotifications();
        void ScheduleNotification(LocalNotification _notification);
        string AppWasLaunchedViaNotificationChannel();
    }
}
#endif