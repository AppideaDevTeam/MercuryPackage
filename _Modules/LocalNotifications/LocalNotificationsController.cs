#if MERCURY_LOCALNOTIFICATIONS
using System.Collections.Generic;
using Sirenix.Utilities;

namespace Mercury.LocalNotifications
{
    public abstract class LocalNotificationsController : MercurySingletonMonoBehaviour<LocalNotificationsController>
    {
        #region USER CALLS

        public void Initialize()
        {
            LocalNotificationManager.Initialize();
        }

        public void RescheduleAll()
        {
            CancelAllNotifications();
            ReScheduleNotifications();
        }

        public void CancelAllNotifications()
        {
            LocalNotificationManager.CancelAllNotifications();
            LocalNotificationManager.LogMessage("All notifications canceled");
        }

        public string AppWasLaunchedViaNotificationChannel()
        {
            string channel = LocalNotificationManager.AppWasLaunchedViaNotificationChannel();
            LocalNotificationManager.LogMessage($"Application was launched via channel: {channel}");
            return channel;
        }

        #endregion
        
        #region IMPLEMENTATIONS

        protected abstract List<NotificationInfo_Periodic> GetPeriodicNotifications();
        protected abstract List<NotificationInfo_FreeResources> GetFreeResourcesNotifications();
        protected abstract List<NotificationInfo_Processes> GetProcessesNotifications();
        protected abstract List<string> GetEnemyConnectedNotifications();
        protected abstract List<LocalNotification> RescheduleCustomNotifications();
        #endregion
        
        private static void ReScheduleNotifications()
        {
            // CUSTOM
            var customNotifs = Instance.RescheduleCustomNotifications();
            if (!customNotifs.IsNullOrEmpty()) LocalNotificationManager.ScheduleCustomNotifications(customNotifs);
            
            // PERIODIC
            var periodicNotifs = Instance.GetPeriodicNotifications();
            if (MercuryLibrarySO.LocalNotificationsDatabase.PeriodicRewardsEnabled && !periodicNotifs.IsNullOrEmpty()) 
            LocalNotificationManager.SchedulePeriodicNotifications(periodicNotifs);
 
            // FREE RESOURCES
            var freeResNotifs = Instance.GetFreeResourcesNotifications(); 
            if (MercuryLibrarySO.LocalNotificationsDatabase.FreeResourcesEnabled && !freeResNotifs.IsNullOrEmpty()) 
            LocalNotificationManager.ScheduleFreeResourcesNotifications(freeResNotifs);
            
            // PROCESSES
            var procNotifs = Instance.GetProcessesNotifications();
            if (MercuryLibrarySO.LocalNotificationsDatabase.ProcessesEnabled && !procNotifs.IsNullOrEmpty()) 
            LocalNotificationManager.ScheduleProcessesNotifications(procNotifs);
            
            // ENEMY CONNECTED
            var enemyNotifs = Instance.GetEnemyConnectedNotifications();
            if (MercuryLibrarySO.LocalNotificationsDatabase.EnemyConnectedEnabled && !enemyNotifs.IsNullOrEmpty()) 
            LocalNotificationManager.ScheduleEnemyConnectedNotifications(enemyNotifs);
            
            // REMAINDERS
            if (MercuryLibrarySO.LocalNotificationsDatabase.RemindersEnabled) 
            LocalNotificationManager.ScheduleRemainderNotifications();
            
            LocalNotificationManager.LogMessage("All notifications rescheduled");
        }
        
        #region APPLICATION FOCUS
        private void OnApplicationFocus(bool focus)
        {
            if (!focus && LocalNotificationManager.IsInitialized) RescheduleAll();
        }
        #endregion
    }
}
#endif
