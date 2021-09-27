#if (UNITY_EDITOR || UNITY_ANDROID) && MERCURY_LOCALNOTIFICATIONS

using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using Unity.Notifications.Android;

namespace Mercury.LocalNotifications
{
    internal class LocalNotificationsManagerAndroid : ILocalNotificationsManager_Platform
    {
        #region IMPLEMENTATIONS
        public void Initialize()
        {
            InitializeChannels();
        } 
        
        public void ScheduleNotification(LocalNotification _notification)
        {
            var notification = new AndroidNotification()
            {
                Title = _notification.Title,
                Text = _notification.Text,
                FireTime = DateTime.Now.Add(_notification.FireTimeDelay),
                ShowTimestamp = true,
                Color = _notification.IconColor
            };

            string targetChannelID = string.IsNullOrEmpty(_notification.ChannelID) ? MercuryLibrarySO.LocalNotificationsDatabase.Channel_Default.ID : _notification.ChannelID;
            
            if (!string.IsNullOrEmpty(_notification.IconSmall)) notification.SmallIcon = _notification.IconSmall;
            if (!string.IsNullOrEmpty(_notification.IconLarge)) notification.LargeIcon = _notification.IconLarge;
            if (!string.IsNullOrEmpty(_notification.Data)) notification.IntentData = _notification.Data;

            AndroidNotificationCenter.SendNotification(notification, targetChannelID);
        }
        
        public void CancelAllNotifications() => AndroidNotificationCenter.CancelAllNotifications();
        
        public bool ApplicationLaunchedViaNotification(out string data)
        {
            var intentData = AndroidNotificationCenter.GetLastNotificationIntent();

            if (intentData != null && !string.IsNullOrEmpty(intentData.Notification.IntentData))
            {
                data = intentData.Notification.IntentData;

                return true;
            }

            data = string.Empty;

            return false;
        }
        #endregion
        
        private static void InitializeChannels()
        {
            // STATIC CHANNELS
            List<NotificationChannel> channelsToInitialize = new List<NotificationChannel>() { MercuryLibrarySO.LocalNotificationsDatabase.Channel_Default };

            if(MercuryLibrarySO.LocalNotificationsDatabase.RemindersEnabled) channelsToInitialize.Add(MercuryLibrarySO.LocalNotificationsDatabase.Channel_Reminders);
            if(MercuryLibrarySO.LocalNotificationsDatabase.PeriodicRewardsEnabled) channelsToInitialize.Add(MercuryLibrarySO.LocalNotificationsDatabase.Channel_PeriodicRewards);
            if(MercuryLibrarySO.LocalNotificationsDatabase.FreeResourcesEnabled) channelsToInitialize.Add(MercuryLibrarySO.LocalNotificationsDatabase.Channel_FreeResources);
            if(MercuryLibrarySO.LocalNotificationsDatabase.ProcessesEnabled) channelsToInitialize.Add(MercuryLibrarySO.LocalNotificationsDatabase.Channel_Processes);
            if(MercuryLibrarySO.LocalNotificationsDatabase.EnemyConnectedEnabled) channelsToInitialize.Add(MercuryLibrarySO.LocalNotificationsDatabase.Channel_EnemyConnected);
            
            // CUSTOM CHANNELS
            if (!MercuryLibrarySO.LocalNotificationsDatabase.Channels_Custom.IsNullOrEmpty()) channelsToInitialize.AddRange(MercuryLibrarySO.LocalNotificationsDatabase.Channels_Custom);

            foreach (NotificationChannel channel in channelsToInitialize)
            {
                if (channel.Enabled)
                {
                    var newChannel = new AndroidNotificationChannel()
                    {
                        Id          = channel.ID,
                        Name        = channel.Name,
                        Description = channel.Description,
                        Importance  = Importance.Default
                    };
                    
                    AndroidNotificationCenter.RegisterNotificationChannel(newChannel);
                }
            }
        }
    }
}

#endif