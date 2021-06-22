#if MERCURY_LOCALNOTIFICATIONS
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mercury.LocalNotifications
{
    public class LocalNotificationsDatabaseSO : ScriptableObject
    {
        #region ELEMENTS
        // GLOBAL SETTINGS
        [PropertyTooltip("Enable logging debug messages."), TitleGroup("Global Settings"), GUIColor("@MercuryLibrarySO.Color_Orange"), LabelWidth(200)]
        public bool DebuggingEnabled;
        [PropertyTooltip("Quiet hours, Any notification at this time range will be shifted out."), TitleGroup("Global Settings"), BoxGroup("Global Settings/Quiet Hours"), LabelText("Enabled: "), LabelWidth(200)]
        public bool QuietHoursEnabled = true;
        [PropertyTooltip("If quiet hours enabled, also affect on custom notifications"), TitleGroup("Global Settings"), BoxGroup("Global Settings/Quiet Hours"), LabelText("Affect on Custom Notifications: "), ShowIf("QuietHoursEnabled"), LabelWidth(200)]
        public bool QuietHoursAffectOnCustomsEnabled = true;
        [PropertyTooltip(""), TitleGroup("Global Settings"), BoxGroup("Global Settings/Common Features"), LabelText("Reminders Enabled: "), LabelWidth(200)]
        public bool RemindersEnabled;
        [PropertyTooltip(""), TitleGroup("Global Settings"), BoxGroup("Global Settings/Specific Features"), LabelText("Periodic Rewards Enabled: "), LabelWidth(200)]
        public bool PeriodicRewardsEnabled;
        [PropertyTooltip(""), TitleGroup("Global Settings"), BoxGroup("Global Settings/Specific Features"), LabelText("Free Resources Enabled: "), LabelWidth(200)]
        public bool FreeResourcesEnabled;
        [PropertyTooltip(""), TitleGroup("Global Settings"), BoxGroup("Global Settings/Specific Features"), LabelText("Processes Enabled: "), LabelWidth(200)]
        public bool ProcessesEnabled;
        [PropertyTooltip(""), TitleGroup("Global Settings"), BoxGroup("Global Settings/Specific Features"), LabelText("Enemy Connected Enabled: "), LabelWidth(200)]
        public bool EnemyConnectedEnabled;

        // CHANNELS
        [PropertyTooltip(""), TabGroup("Tabs", "Channels"), TitleGroup("Tabs/Channels/Settings"), GUIColor("@MercuryLibrarySO.Color_Violet"), LabelText("Redirect All To Default Channel: "), LabelWidth(200)]
        public bool RedirectAllToDefaultChannel = true;
        [PropertyTooltip(""), TabGroup("Tabs", "Channels"), TitleGroup("Tabs/Channels/Static Channels", "Android only"), GUIColor("@MercuryLibrarySO.Color_Violet"), ReadOnly, LabelText("Default")]
        public NotificationChannel Channel_Default = new NotificationChannel(true, "default", "Default Channel", "Default channel");
        [PropertyTooltip(""), TabGroup("Tabs", "Channels"), TitleGroup("Tabs/Channels/Static Channels", "Android only"), ShowIf("@this.RemindersEnabled && !this.RedirectAllToDefaultChannel"), ReadOnly, LabelText("Reminders")]
        public NotificationChannel Channel_Reminders = new NotificationChannel(true, "reminders", "Reminders", "Reminder notifications");
        [PropertyTooltip(""), TabGroup("Tabs", "Channels"), TitleGroup("Tabs/Channels/Static Channels", "Android only"), ShowIf("@this.PeriodicRewardsEnabled && !this.RedirectAllToDefaultChannel"), ReadOnly, LabelText("Periodic Rewards")]
        public NotificationChannel Channel_PeriodicRewards = new NotificationChannel(true, "periodic_rewards", "Periodic Rewards", "Periodic rewards notifications");
        [PropertyTooltip(""), TabGroup("Tabs", "Channels"), TitleGroup("Tabs/Channels/Static Channels", "Android only"), ShowIf("@this.FreeResourcesEnabled && !this.RedirectAllToDefaultChannel"), ReadOnly, LabelText("Free Resources")]
        public NotificationChannel Channel_FreeResources = new NotificationChannel(true, "free_resources", "Free Resources", "Free resources notifications");
        [PropertyTooltip(""), TabGroup("Tabs", "Channels"), TitleGroup("Tabs/Channels/Static Channels", "Android only"), ShowIf("@this.ProcessesEnabled && !this.RedirectAllToDefaultChannel"), ReadOnly, LabelText("Processes")]
        public NotificationChannel Channel_Processes = new NotificationChannel(true, "processes", "Processes", "Processes notifications");
        [PropertyTooltip(""), TabGroup("Tabs", "Channels"), TitleGroup("Tabs/Channels/Static Channels", "Android only"), ShowIf("@this.EnemyConnectedEnabled && !this.RedirectAllToDefaultChannel"), ReadOnly, LabelText("Enemy Connected")]
        public NotificationChannel Channel_EnemyConnected = new NotificationChannel(true, "enemy_connected", "Enemy Connected", "Enemy connected notifications");
        [PropertyTooltip(""), TabGroup("Tabs", "Channels"), TitleGroup("Tabs/Channels/Custom Channels", "Android only"), ShowIf("@!this.RedirectAllToDefaultChannel")]
        public List<NotificationChannel> Channels_Custom;

        // TIME RANGES
        [PropertyTooltip(""), TabGroup("Tabs", "Time Ranges"), TitleGroup("Tabs/Time Ranges/Quiet Hours"), LabelText("Random Offset Range (Minutes):"), LabelWidth(200), HideLabel, ShowIf("QuietHoursEnabled")]
        public uint QuietHoursRandomOffsetRange = 30;
        [PropertyTooltip(""), TabGroup("Tabs", "Time Ranges"), TitleGroup("Tabs/Time Ranges/Quiet Hours"), HideLabel, ShowIf("QuietHoursEnabled")]
        public TimeRange QuietHours;
        [PropertyTooltip(""), TabGroup("Tabs", "Time Ranges"), TitleGroup("Tabs/Time Ranges/Remainders", "Daily reminders"), ShowIf("RemindersEnabled"), LabelText("Start Reminding In Days: "), LabelWidth(200)]
        public uint StartRemindingInDays = 2;
        [PropertyTooltip(""), TabGroup("Tabs", "Time Ranges"), TitleGroup("Tabs/Time Ranges/Remainders", "Daily reminders"), ShowIf("RemindersEnabled"), LabelText("Remind Duration In Days: "), LabelWidth(200)]
        public uint RemindDurationInDays = 14;
        [PropertyTooltip(""), TabGroup("Tabs", "Time Ranges"), TitleGroup("Tabs/Time Ranges/Remainders"), ShowIf("RemindersEnabled"), LabelText("Different on Weekends: "), LabelWidth(200)]
        public bool DifferentOnWeekends;
        [PropertyTooltip(""), TabGroup("Tabs", "Time Ranges"), TitleGroup("Tabs/Time Ranges/Remainders"), HideLabel, Title("Workdays", "From Monday to Friday", TitleAlignment = TitleAlignments.Centered), ShowIf("RemindersEnabled"), TableList(AlwaysExpanded = false, ShowIndexLabels = true)]
        public List<TimeRange> ReminderHoursWorkdays;
        [PropertyTooltip(""), TabGroup("Tabs", "Time Ranges"), TitleGroup("Tabs/Time Ranges/Remainders"), HideLabel, Title("Weekends", "Saturday and Sunday", TitleAlignment = TitleAlignments.Centered), ShowIf("@RemindersEnabled && DifferentOnWeekends"), TableList(AlwaysExpanded = false, ShowIndexLabels = true)]
        public List<TimeRange> ReminderHoursWeekends;
        [PropertyTooltip(""), TabGroup("Tabs", "Time Ranges"), TitleGroup("Tabs/Time Ranges/Free Resources"), ShowIf("FreeResourcesEnabled"), LabelText("Spam Prevention Enabled"), LabelWidth(200)]
        public bool FreeResourcesSpamPreventionEnabled;
        [PropertyTooltip(""), TabGroup("Tabs", "Time Ranges"), TitleGroup("Tabs/Time Ranges/Free Resources"), ShowIf("@this.FreeResourcesEnabled && this.FreeResourcesSpamPreventionEnabled"), LabelText("Spam Prevention Threshold Minutes"), LabelWidth(200)]
        public int FreeResourcesSpamPreventionThresholdMinutes = 10;

        [PropertyTooltip(""), TabGroup("Tabs", "Time Ranges"), TitleGroup("Tabs/Time Ranges/Enemy Connected", "Fake Enemy Connected Notifications"), ShowIf("EnemyConnectedEnabled"), LabelText("Start Enemy Notifs. In Hours: "), LabelWidth(200), InfoBox("Hours are formatted in 24 hour notation!", InfoMessageType.Warning)]
        public uint StartEnemyConnectedNotificationsInHours = 4;
        [PropertyTooltip(""), TabGroup("Tabs", "Time Ranges"), TitleGroup("Tabs/Time Ranges/Enemy Connected"), ShowIf("EnemyConnectedEnabled"), LabelText("Enemy Notifs. Per Day: "), LabelWidth(200)]
        public uint EnemyConnectedNotificationsPerDay = 2;
        [PropertyTooltip(""), TabGroup("Tabs", "Time Ranges"), TitleGroup("Tabs/Time Ranges/Enemy Connected"), ShowIf("EnemyConnectedEnabled"), LabelText("Enemy Notifs. From Hour: "), LabelWidth(200)]
        public uint EnemyConnectedNotificationsFromHour = 9;
        [PropertyTooltip(""), TabGroup("Tabs", "Time Ranges"), TitleGroup("Tabs/Time Ranges/Enemy Connected"), ShowIf("EnemyConnectedEnabled"), LabelText("Enemy Notifs. To Hour: "), LabelWidth(200)]
        public uint EnemyConnectedNotificationsToHour = 23;

        // TEXTS
        [PropertyTooltip(""), TabGroup("Tabs", "Notification Texts"), TitleGroup("Tabs/Notification Texts/Remainders"), ShowIf("RemindersEnabled")]
        public List<NotificationEditorData> reminderTexts;
        [PropertyTooltip(""), TabGroup("Tabs", "Notification Texts"), TitleGroup("Tabs/Notification Texts/Periodic Rewards"), ShowIf("PeriodicRewardsEnabled"), InfoBox("For passing data in buffers & icons - use (Tag:String, Value:String) tuple convention!")]
        public List<NotificationEditorData_Periodic> periodicRewardsTexts;
        [PropertyTooltip(""), TabGroup("Tabs", "Notification Texts"), TitleGroup("Tabs/Notification Texts/Free Resources"), ShowIf("FreeResourcesEnabled"), InfoBox("For passing data in buffers & icons - use (Tag:String, Value:String) tuple convention!")]
        public List<NotificationEditorData_FreeRewards> freeResourcesTexts;
        [PropertyTooltip(""), TabGroup("Tabs", "Notification Texts"), TitleGroup("Tabs/Notification Texts/Processes"), ShowIf("ProcessesEnabled"), InfoBox("For passing data in buffers & icons - use (Tag:String, Value:String) tuple convention!")]
        public List<NotificationEditorData_Processes> processesTexts;
        [PropertyTooltip(""), TabGroup("Tabs", "Notification Texts"), TitleGroup("Tabs/Notification Texts/Enemy Connected"), ShowIf("EnemyConnectedEnabled"), InfoBox("Write name of the enemy in %Enemy% tag!")]
        public NotificationEditorData_EnemyConnected enemyConnectedText;
        #endregion

        #region UTILS
        public static string IntHourToTime(int _hour)
        {
            int x = _hour;
            if (x <= -12) return x + 24 + "AM";
            if (x <= 0) return x + 12   + "PM";
            if (x > 12) return x - 12   + "PM";
            return x + "AM";
        }
        #endregion
    }

    [Serializable]
    public class TimeRange
    {
        [BoxGroup("Ranges", ShowLabel = false), GUIColor("@MercuryLibrarySO.Color_Violet"), Title("$rangeString", "-24h, +24h", TitleAlignment = TitleAlignments.Centered, Bold = true)] [MinMaxSlider(-24, 24), HideLabel, SerializeField]
        private Vector2Int range = new Vector2Int(-2, 9);
        private string quietHourFromString => LocalNotificationsDatabaseSO.IntHourToTime(range.x);
        private string quietHourToString   => LocalNotificationsDatabaseSO.IntHourToTime(range.y);
        private string rangeString         => "From: " + quietHourFromString + " - To: " + quietHourToString;
        public  int    From                => (range.x >= 0) ? range.x : 24 + range.x;
        public  int    To                  => (range.y >= 0) ? range.y : 24 + range.y;
    }

    [Serializable]
    public class NotificationChannel
    {
        [HorizontalGroup("Group"), TitleGroup("Group/Enabled"), HideLabel]     public bool   Enabled;
        [HorizontalGroup("Group"), TitleGroup("Group/ID"), HideLabel]          public string ID;
        [HorizontalGroup("Group"), TitleGroup("Group/Name"), HideLabel]        public string Name;
        [HorizontalGroup("Group"), TitleGroup("Group/Description"), HideLabel] public string Description;

        public NotificationChannel(bool _enabled, string id, string name, string description)
        {
            Enabled     = _enabled;
            ID          = id;
            Name        = name;
            Description = description;
        }
    }

    #region NOTIFICATION EDITOR DATA

    [Serializable]
    public class NotificationEditorData_Periodic
    {
        [LabelWidth(180)] public string                 Period;
        public                   NotificationEditorData EditorData;
    }
    
    [Serializable]
    public class NotificationEditorData_FreeRewards
    {
        
        [HorizontalGroup("Group"), Space(32), TitleGroup("Group/Info"),  LabelWidth(80)] public string                 Identifier;
        [HorizontalGroup("Group"), TitleGroup("Group/Info"), LabelWidth(80), Range(1, 100), LabelText("Priority (%)")] public int Priority = 50;
        [HorizontalGroup("Group"), TitleGroup("Group/Data"), HideLabel ] public                   NotificationEditorData EditorData;
    }

    [Serializable]
    public class NotificationEditorData_EnemyConnected
    {
        public NotificationEditorData EditorData;
    }

    [Serializable]
    public class NotificationEditorData_Processes
    {
        public string                 Identifier;
        public NotificationEditorData EditorData;
    }

    [Serializable]
    public class NotificationEditorData
    {
        [HorizontalGroup("Group"), TitleGroup("Group/Title Buffer"), HideLabel, MultiLineProperty(3), OnValueChanged("TitleValueChanged")]
        public string titleBuffer;
        [HorizontalGroup("Group"), TitleGroup("Group/Title Preview"), HideLabel, MultiLineProperty(3), ReadOnly, GUIColor("@MercuryLibrarySO.Color_Violet")]
        public string titlePreview;
        [HorizontalGroup("Group"), TitleGroup("Group/Text Buffer"), HideLabel, MultiLineProperty(3), OnValueChanged("TextValueChanged")]
        public string textBuffer;
        [HorizontalGroup("Group"), TitleGroup("Group/Text Preview"), HideLabel, MultiLineProperty(3), ReadOnly, GUIColor("@MercuryLibrarySO.Color_Violet")]
        public string textPreview;
        [HorizontalGroup("Group"), TitleGroup("Group/Icon Small"), HideLabel] public string iconSmall;
        [HorizontalGroup("Group"), TitleGroup("Group/Icon Large"), HideLabel] public string iconLarge;

        public string Title => Uri.UnescapeDataString(titleBuffer);
        public string Text  => Uri.UnescapeDataString(textBuffer);

        private string SafeEscapeDataString(string _text)
        {
            while (true)
            {
                string unescaped = Uri.UnescapeDataString(_text);

                if (unescaped.Equals(_text)) break;

                _text = unescaped;
            }

            return Uri.EscapeDataString(_text);
        }
        
        public void TitleValueChanged(string buffer)
        {
            titlePreview = buffer;
            titleBuffer  = SafeEscapeDataString(buffer);
        }

        public void TextValueChanged(string buffer)
        {
            textPreview = buffer;
            textBuffer  = SafeEscapeDataString(buffer);
        }
    }

    #endregion
}
#endif