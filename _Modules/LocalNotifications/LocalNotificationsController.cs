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
            LocalNotificationsManager.Initialize();
        }

        public void RescheduleAll()
        {
            CancelAllNotifications();
            ReScheduleNotifications();
        }

        public void CancelAllNotifications()
        {
            LocalNotificationsManager.CancelAllNotifications();
            LocalNotificationsManager.LogMessage("All notifications canceled");
        }

        public string AppWasLaunchedViaNotificationChannel()
        {
            string channel = LocalNotificationsManager.AppWasLaunchedViaNotificationChannel();
            LocalNotificationsManager.LogMessage($"Application was launched via channel: {channel}");
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
            if (!customNotifs.IsNullOrEmpty()) LocalNotificationsManager.ScheduleCustomNotifications(customNotifs);
            
            // PERIODIC
            var periodicNotifs = Instance.GetPeriodicNotifications();
            if (MercuryLibrarySO.LocalNotificationsDatabase.PeriodicRewardsEnabled && !periodicNotifs.IsNullOrEmpty()) 
            LocalNotificationsManager.SchedulePeriodicNotifications(periodicNotifs);
 
            // FREE RESOURCES
            var freeResNotifs = Instance.GetFreeResourcesNotifications(); 
            if (MercuryLibrarySO.LocalNotificationsDatabase.FreeResourcesEnabled && !freeResNotifs.IsNullOrEmpty()) 
            LocalNotificationsManager.ScheduleFreeResourcesNotifications(freeResNotifs);
            
            // PROCESSES
            var procNotifs = Instance.GetProcessesNotifications();
            if (MercuryLibrarySO.LocalNotificationsDatabase.ProcessesEnabled && !procNotifs.IsNullOrEmpty()) 
            LocalNotificationsManager.ScheduleProcessesNotifications(procNotifs);
            
            // ENEMY CONNECTED
            var enemyNotifs = Instance.GetEnemyConnectedNotifications();
            if (MercuryLibrarySO.LocalNotificationsDatabase.EnemyConnectedEnabled && !enemyNotifs.IsNullOrEmpty()) 
            LocalNotificationsManager.ScheduleEnemyConnectedNotifications(enemyNotifs);
            
            // REMAINDERS
            if (MercuryLibrarySO.LocalNotificationsDatabase.RemindersEnabled) 
            LocalNotificationsManager.ScheduleRemainderNotifications();
            
            LocalNotificationsManager.LogMessage("All notifications rescheduled");
        }
        
        #region APPLICATION FOCUS
        private void OnApplicationFocus(bool focus)
        {
            if (!focus && LocalNotificationsManager.IsInitialized) RescheduleAll();
        }
        #endregion
    }
}
#endif
