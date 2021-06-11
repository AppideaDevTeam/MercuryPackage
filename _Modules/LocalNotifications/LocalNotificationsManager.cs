#if MERCURY_LOCALNOTIFICATIONS
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mercury.LocalNotifications
{
    internal static class LocalNotificationsManager
    {
        #region VARIABLES
        private static ILocalNotificationsManager_Platform _platformManager;
        public static  LocalNotificationsDatabaseSO       Database => MercuryLibrarySO.LocalNotificationsDatabase;

        public static bool IsInitialized { get; private set; }

        #endregion
        
        
        #region MAIN FUNCTIONS
        public static void Initialize()
        {
            if (IsInitialized) return;
            
            #if UNITY_ANDROID
            _platformManager = new LocalNotificationsManagerAndroid();
            #elif UNITY_IOS
            _platformManager = new LocalNotificationManager_iOS();
            #else
            LogMessage("Notifications for current platform are not supported.", LogType.Exception);
            #endif

            _platformManager.Initialize();

            IsInitialized = true;
            
            LogMessage("Initialized");
        }
        
        private static void ScheduleLocalNotification(LocalNotification _notification, bool _avoidQuietHours)
        {
            if (Database.QuietHoursEnabled && _avoidQuietHours)
            {
                DateTime scheduleTime = DateTime.Now.Add(_notification.FireTimeDelay);

                // PREVENT SCHEDULING NOTIFICATIONS IN PAST
                if (DateTime.Now > scheduleTime)
                {
                    LogMessage($"Title: {_notification.Title} - Notification scheduling canceled due to fire time being in the past");
                    return;
                }
                
                if (IsDateTimeInQuietHours(scheduleTime)) 
                    _notification.FireTimeDelay = ModifyDateTimeOutsideQuietHours(scheduleTime).Subtract(DateTime.Now);
            }

            _platformManager.ScheduleNotification(_notification);
        }
        #endregion

        #region SCHEDULE CALLS
        
        #region PERIODIC
        public static void SchedulePeriodicNotifications(List<NotificationInfo_Periodic> _notificationInfoPeriodic)
        {
            foreach (NotificationInfo_Periodic notificationInfo in _notificationInfoPeriodic)
            {
                foreach (var txt in Database.periodicRewardsTexts)
                {
                    if (txt.Period.Equals(notificationInfo.Period))
                    {
                        var notification = new LocalNotification
                        {
                            Title         = ProcessBufferTaggedData(txt.EditorData.Title, notificationInfo.Data),
                            Text          = ProcessBufferTaggedData(txt.EditorData.Text,  notificationInfo.Data),
                            IconSmall     = txt.EditorData.iconSmall,
                            IconLarge     = txt.EditorData.iconLarge,
                            FireTimeDelay = notificationInfo.FireTime,
                            ChannelID     = Database.RedirectAllToDefaultChannel ? Database.Channel_Default.ID : "periodic"
                        };
            
                        ScheduleLocalNotification(notification, true);
            
                        // DEBUG
                        LogMessage($"TYPE: Periodic | Period: {notificationInfo.Period} | Delivery Time: {DateTime.Now.Add(notification.FireTimeDelay).ToString_DDMMYYYYHHMMTT()}");
                        return;
                    }
                }
            }
        }
        #endregion
        
        #region CUSTOM NOTIFICATIONS
        public static void ScheduleCustomNotifications(List<LocalNotification> _notifications)
        {
            foreach (var notification in _notifications)
            {
                ScheduleLocalNotification(notification, Database.QuietHoursAffectOnCustomsEnabled);
                
                // DEBUG
                LogMessage($"TYPE: Custom | Channel: {notification.ChannelID} | Delivery Time: {DateTime.Now.Add(notification.FireTimeDelay).ToString_DDMMYYYYHHMMTT()}");
            }
        }
        #endregion

        #region FREE RESOURCES
        public static void ScheduleFreeResourcesNotifications(List<NotificationInfo_FreeResources> _notificationInfos)
        {
            var filteredNotifications = FilterFreeResourcesNotifications(_notificationInfos);

            foreach (var notificationInfo in filteredNotifications)
            {
                foreach (var txt in Database.freeResourcesTexts)
                {
                    if (txt.Identifier.Equals(notificationInfo.Identifier))
                    {
                        var notification = new LocalNotification
                        {
                            Title         = ProcessBufferTaggedData(txt.EditorData.Title, new List<DataTagValuePair>{ notificationInfo.Data }),
                            Text          = ProcessBufferTaggedData(txt.EditorData.Text,  new List<DataTagValuePair>{ notificationInfo.Data }),
                            IconSmall     = txt.EditorData.iconSmall,
                            IconLarge     = txt.EditorData.iconLarge,
                            FireTimeDelay = notificationInfo.FireTime,
                            ChannelID     = Database.RedirectAllToDefaultChannel ? Database.Channel_Default.ID : "free_resources"
                        };

                        ScheduleLocalNotification(notification, true);
                        
                        // DEBUG
                        LogMessage($"TYPE: FreeResources | Title: {notification.Title} | Delivery Time: {DateTime.Now.Add(notification.FireTimeDelay).ToString_DDMMYYYYHHMMTT()}");
                        break;
                    }
                }
            }
        }
        
        private static int GetPriorityOfFreeResourcesNotification(string _identifier)
        {
            foreach (NotificationEditorData_FreeRewards editorData in Database.freeResourcesTexts)
                if (_identifier.Equals(editorData.Identifier)) return editorData.Priority;

            return 1;
        }

        private static List<NotificationInfo_FreeResources> FilterFreeResourcesNotifications(List<NotificationInfo_FreeResources> _notificationInfos)
        {
            var sortedList = new List<NotificationInfo_FreeResources>(_notificationInfos);
            sortedList.Sort((notif1, notif2) => (int) notif1.FireTime.TotalMinutes - (int) notif2.FireTime.TotalMinutes);

            // FILTERED LIST
            List<NotificationInfo_FreeResources> filteredList = new List<NotificationInfo_FreeResources>();

            for (var comparerA = 0; comparerA < sortedList.Count; comparerA++)
            {
                // IF LAST ELEMENT
                if (comparerA + 1 == sortedList.Count) break;

                List<NotificationInfo_FreeResources> localSpamList = new List<NotificationInfo_FreeResources>();

                for (var comparerB = comparerA + 1; comparerB < sortedList.Count; comparerB++)
                {
                    int deltaMinutes = (int) sortedList[comparerA + comparerB].FireTime.Subtract(sortedList[comparerA].FireTime).TotalMinutes;

                    if (deltaMinutes < Database.FreeResourcesSpamPreventionThresholdMinutes)
                    {
                        if (!localSpamList.Contains(sortedList[comparerA])) localSpamList.Add(sortedList[comparerA]);
                        if (!localSpamList.Contains(sortedList[comparerB])) localSpamList.Add(sortedList[comparerB]);
                    }
                }

                // ADD VALUES TO FILTERED LIST
                if (localSpamList.IsNullOrEmpty())
                {
                    // IF SOLO
                    filteredList.Add(sortedList[comparerA]);
                }
                else
                {
                    // GET HIGHEST PRIORITY MEMBER AMONGST LOCAL SPAM LIST
                    localSpamList.Sort((notif1, notif2) =>
                    {
                        int priorityA = GetPriorityOfFreeResourcesNotification(notif1.Identifier);
                        int priorityB = GetPriorityOfFreeResourcesNotification(notif2.Identifier);

                        return priorityA - priorityB;
                    });

                    filteredList.Add(localSpamList.First());
                }
            }
            
            return filteredList;
        }
        

        #endregion
        
        #region ENEMY CONNECTED

        public static void ScheduleEnemyConnectedNotifications(List<string> _names)
        {
            var scheduledTimes = CalculateEnemyTime(_names.Count);

            var editorData = Database.enemyConnectedText;
            
            for (var i = 0; i < _names.Count; i++)
            {
                var notification = new LocalNotification
                {
                    Title         = editorData.EditorData.Title,
                    Text          = editorData.EditorData.Text.Replace("%Enemy%", _names[i]),
                    IconSmall     = editorData.EditorData.iconSmall,
                    IconLarge     = editorData.EditorData.iconLarge,
                    FireTimeDelay = scheduledTimes[i],
                    ChannelID     = Database.RedirectAllToDefaultChannel ? Database.Channel_Default.ID : "enemy_connected"
                };

                ScheduleLocalNotification(notification, false);
                
                // DEBUG
                LogMessage($"TYPE: EnemyConnected | Enemy: {_names[i]} | Delivery Time: {DateTime.Now.Add(notification.FireTimeDelay).ToString_DDMMYYYYHHMMTT()}");
            }    
        }

        public static List<TimeSpan> CalculateEnemyTime(int _nameCount)
        {
            var result = new List<TimeSpan>();

            var scheduledNotificationsCount = 0;
            var days                        = 0;

            var usefulRangeFrom = Database.EnemyConnectedNotificationsFromHour;
            var usefulRangeTo   = Database.EnemyConnectedNotificationsToHour;

            var intervalLength = (usefulRangeTo - usefulRangeFrom) / (float) Database.EnemyConnectedNotificationsPerDay;

            while (scheduledNotificationsCount <= _nameCount)
            {
                for (var i = 0; i < Database.EnemyConnectedNotificationsPerDay; i++)
                {
                    var randomInInterval = TimeSpan.FromMinutes(Random.Range(0f, intervalLength * 60f));

                    var possibleScheduleTime = DateTime.Today.AddDays(days).AddHours(usefulRangeFrom).AddHours(i * intervalLength).Add(randomInInterval);
                    var minimumScheduleTime  = DateTime.Now.AddHours(Database.StartEnemyConnectedNotificationsInHours);

                    if (possibleScheduleTime > minimumScheduleTime)
                    {
                        result.Add(possibleScheduleTime.Subtract(DateTime.Now));
                        scheduledNotificationsCount++;
                    }
                }

                days++;
            }

            return result;
        }

        #endregion

        #region PROCESSES


        public static void ScheduleProcessesNotifications(List<NotificationInfo_Processes> _notificationInfos)
        {
            foreach (var notificationInfo in _notificationInfos)
            {
                foreach (var txt in Database.processesTexts)
                {
                    if (txt.Identifier.Equals(notificationInfo.Identifier))
                    {
                        var notification = new LocalNotification
                        {
                            Title         = ProcessBufferTaggedData(txt.EditorData.Title, notificationInfo.Data),
                            Text          = ProcessBufferTaggedData(txt.EditorData.Text,  notificationInfo.Data),
                            IconSmall     = txt.EditorData.iconSmall,
                            IconLarge     = txt.EditorData.iconLarge,
                            FireTimeDelay = notificationInfo.FireTime,
                            ChannelID     = Database.RedirectAllToDefaultChannel ? Database.Channel_Default.ID : "processes_rewards"
                        };

                        ScheduleLocalNotification(notification, true);

                        // DEBUG
                        LogMessage($"TYPE: Processes | Title: {notification.Title} | Delivery Time: {DateTime.Now.Add(notification.FireTimeDelay).ToString_DDMMYYYYHHMMTT()}");

                        break;
                    }
                }
            }
        }
        
                
        private static string ProcessBufferTaggedData(string _buffer, List<DataTagValuePair> _data)
        {
            var processedBuffer = _buffer;

            if (!_data.IsNullOrEmpty())
                foreach (var entry in _data)
                    processedBuffer.Replace($"%{entry.Tag}%", entry.Value);

            return processedBuffer;
        }

        #endregion
        
        #region SCHEDULE REMAINDERS

        public static void ScheduleRemainderNotifications()
        {
            var startRemindingInDays = (int) Database.StartRemindingInDays;
            var remindDurationInDays = (int) Database.RemindDurationInDays;

            for (var dayIndex = startRemindingInDays; dayIndex < remindDurationInDays + startRemindingInDays; dayIndex++)
                if (Database.DifferentOnWeekends && IsWeekend(DateTime.Today.AddDays(dayIndex)))
                    // WEEKEND
                    ScheduleReminderNotificationsForDays(Database.ReminderHoursWeekends, dayIndex);
                else
                    // WEEKDAY
                    ScheduleReminderNotificationsForDays(Database.ReminderHoursWorkdays, dayIndex);
        }

        private static void ScheduleReminderNotificationsForDays(List<TimeRange> _timeRangeList, int _dayIndex)
        {
            foreach (var timeRange in _timeRangeList)
            {
                // GENERATE DATETIME ON EACH PERIOD
                var deliveryTime = GenerateDeliveryDateTimeBasedOnTodaysDate(timeRange.From, timeRange.To, _dayIndex);

                // GENERATE RANDOM INDEX FOR TEXTS
                var randIndex               = Random.Range(0, Database.reminderTexts.Count);
                var notificationTitle       = Database.reminderTexts[randIndex].Title;
                var notificationDescription = Database.reminderTexts[randIndex].Text;
                var notificationIconSmall   = Database.reminderTexts[randIndex].iconSmall;
                var notificationIconLarge   = Database.reminderTexts[randIndex].iconLarge;

                // IF GENERATED DATETIME IS LATER THAN NOW
                if (deliveryTime > DateTime.Now)
                {
                    var fireTimeDelay = deliveryTime.Subtract(DateTime.Now);

                    var notification = new LocalNotification
                    {
                        Title         = notificationTitle,
                        Text          = notificationDescription,
                        IconSmall     = notificationIconSmall,
                        IconLarge     = notificationIconLarge,
                        FireTimeDelay = fireTimeDelay
                    };

                    ScheduleLocalNotification(notification, false);

                    // DEBUG
                    LogMessage($"TYPE: Remainders | Day: {_dayIndex} | Delivery Time: {deliveryTime.ToString_DDMMYYYYHHMMTT()}");
                }
            }
        }



        public static void CancelAllNotifications() { _platformManager.CancelAllNotifications(); }

        #endregion

        #endregion

        #region TIME CALCULATIONS
        // CHECK QUIET HOURS
        private static bool IsDateTimeInQuietHours(DateTime _dateTime)
        {
            if (Database.QuietHoursEnabled)
            {
                bool quietHoursIsStretchedOverTwoDays = Database.QuietHours.To < Database.QuietHours.From;
                
                DateTime quietHoursFrom = _dateTime.Date.AddHours(Database.QuietHours.From);
                DateTime quietHoursTo = _dateTime.Date.AddHours(Database.QuietHours.To);

                if (quietHoursIsStretchedOverTwoDays) return _dateTime.TimeOfDay <= quietHoursTo.TimeOfDay || _dateTime.TimeOfDay >= quietHoursFrom.TimeOfDay;
                return _dateTime.TimeOfDay >= quietHoursFrom.TimeOfDay && _dateTime.TimeOfDay <= quietHoursTo.TimeOfDay;
            }
                

            return false;
        }

        // MODIFY DATETIME OUTSIDE QUIET HOURS
        private static DateTime ModifyDateTimeOutsideQuietHours(DateTime _dateTime)
        {
            bool quietHoursIsStretchedOverTwoDays = Database.QuietHours.To < Database.QuietHours.From;

            TimeSpan randomOffset     = TimeSpan.FromMinutes(Random.Range(0, Database.QuietHoursRandomOffsetRange));
            DateTime modifiedDateTime = _dateTime.Date.AddHours(Database.QuietHours.To).Add(randomOffset);
            
            if (quietHoursIsStretchedOverTwoDays && _dateTime.Hour >= Database.QuietHours.From) modifiedDateTime = modifiedDateTime.AddDays(1);

            return modifiedDateTime;
        }

        // SCHEDULED DATETIME
        private static DateTime GenerateDeliveryDateTimeBasedOnTodaysDate(int _minHour, int _maxHour, int _dayOffset = 0) { return DateTime.Today.AddDays(_dayOffset).Add(TimeSpan.FromHours(_minHour)).Add(GetRandomTimeOffset(TimeSpan.FromHours(_maxHour - _minHour))); }

        // GET RANDOM TIME OFFSET
        private static TimeSpan GetRandomTimeOffset(TimeSpan _timeSpan) { return TimeSpan.FromSeconds(Random.Range(0, (int) _timeSpan.TotalSeconds)); }

        // IF DATETIME IS WEEKEND
        private static bool IsWeekend(DateTime _dateTime) { return _dateTime.DayOfWeek == DayOfWeek.Saturday || _dateTime.DayOfWeek == DayOfWeek.Sunday; }

        #endregion

        #region INTENT
        public static string AppWasLaunchedViaNotificationChannel() { return _platformManager.AppWasLaunchedViaNotificationChannel(); }

        #endregion

        #region DEBUG
        public static void LogMessage(string _message, LogType _logType = LogType.Info)
        {
            if (Database.DebuggingEnabled) MercuryDebugger.LogMessage(LogModule.LocalNotifications, _message, _logType);
        }
        #endregion
    }
    
    #region ADDITIONAL CLASSES AND STRUCTS
    public class NotificationInfo_Periodic
    {
        public string           Period;
        public List<DataTagValuePair> Data;
        public TimeSpan         FireTime;
    }

    public class NotificationInfo_Processes
    {
        public string                 Identifier;
        public List<DataTagValuePair> Data;
        public TimeSpan               FireTime;
    }

    public class NotificationInfo_FreeResources
    {
        public string           Identifier;
        public DataTagValuePair Data;
        public TimeSpan         FireTime;
    }

    public struct DataTagValuePair
    {
        public string Tag;
        public string Value;

        public DataTagValuePair(string _tag, string _value)
        {
            Tag   = _tag;
            Value = _value;
        }
    }
    #endregion
}
#endif