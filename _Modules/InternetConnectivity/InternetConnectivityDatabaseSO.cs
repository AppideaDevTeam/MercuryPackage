#if MERCURY_INTERNETCONNECTIVITY

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mercury.InternetConnectivity
{
    public class InternetConnectivityDatabaseSO : ScriptableObject
    {
        // GLOBAL SETTINGS
        [PropertyTooltip("Enable logging debug messages."), TitleGroup("Global Settings"), GUIColor("@MercuryLibrarySO.Color_Orange"), LabelWidth(200)]
        public bool DebuggingEnabled;
        
        [TitleGroup("Ping Servers"), TableList(AlwaysExpanded = true, ShowIndexLabels = true), HideLabel] public List<PingEntryEditor> PingEntries = new List<PingEntryEditor>
        {
            new PingEntryEditor("1.1.1.1", "Cloudflare"), 
            new PingEntryEditor("8.8.8.8", "google-public-dns-a.google.com"), 
            new PingEntryEditor("8.8.4.4", "google-public-dns-b.google.com")
        };

        [TitleGroup("Ping Settings"), LabelWidth(200), InfoBox("This field will used to prevent abusing servers with ping requests.", InfoMessageType.Warning)] 
        public int PingGateInMilliseconds = 800;
        [TitleGroup("Ping Settings"), LabelWidth(200)] 
        public int PingTimeoutMilliseconds          = 1000;

        [TitleGroup("Time Servers"), LabelWidth(200), TableList(AlwaysExpanded = true, ShowIndexLabels = true), HideLabel]
        public List<TimeServerEntryEditor> TimeServerEntries = new List<TimeServerEntryEditor>
        {
            new TimeServerEntryEditor("time.nist.gov",       "Multiple locations"),
            new TimeServerEntryEditor("time-a-g.nist.gov",   "NIST, Gaithersburg, Maryland"),
            new TimeServerEntryEditor("time-b-g.nist.gov",   "NIST, Gaithersburg, Maryland"),
            new TimeServerEntryEditor("time-c-g.nist.gov",   "NIST, Gaithersburg, Maryland"),
            new TimeServerEntryEditor("time-d-g.nist.gov",   "NIST, Gaithersburg, Maryland"),
            new TimeServerEntryEditor("time-e-g.nist.gov",   "NIST, Gaithersburg, Maryland"),
            new TimeServerEntryEditor("time-a-wwv.nist.gov", "WWV, Fort Collins, Colorado"),
            new TimeServerEntryEditor("time-b-wwv.nist.gov", "WWV, Fort Collins, Colorado"),
            new TimeServerEntryEditor("time-c-wwv.nist.gov", "WWV, Fort Collins, Colorado"),
            new TimeServerEntryEditor("time-d-wwv.nist.gov", "WWV, Fort Collins, Colorado"),
            new TimeServerEntryEditor("time-e-wwv.nist.gov", "WWV, Fort Collins, Colorado"),
            new TimeServerEntryEditor("time-a-b.nist.gov",   "NIST, Boulder, Colorado"),
            new TimeServerEntryEditor("time-b-b.nist.gov",   "NIST, Boulder, Colorado"),
            new TimeServerEntryEditor("time-c-b.nist.gov",   "NIST, Boulder, Colorado"),
            new TimeServerEntryEditor("time-d-b.nist.gov",   "NIST, Boulder, Colorado"),
            new TimeServerEntryEditor("time-e-b.nist.gov",   "NIST, Boulder, Colorado")
        };
    }

    [Serializable]
    public class PingEntryEditor
    {
        public string IPAddress;
        public string Info;

        [ReadOnly, GUIColor("@MercuryLibrarySO.Color_Violet")] public string Status;

        public PingEntryEditor(string _ipAddress, string _info)
        {
            IPAddress = _ipAddress;
            Info = _info;
        }

        [TableColumnWidth(300, Resizable = false), Button("Check Now")]
        public void Ping()
        {
            if (string.IsNullOrEmpty(IPAddress))
            {
                Status = "<No address entered>";
            }
            else
            {
                PingStatus status = InternetConnectivityManager.Ping(IPAddress);

                Status = status.Success ? status.Delay.ToString() : "<Failed to ping server>";
            }
        }
    }

    [Serializable]
    public class TimeServerEntryEditor
    {
        public string HostName;
        public string Location;
        [ReadOnly, GUIColor("@MercuryLibrarySO.Color_Violet")] public string Status;

        public TimeServerEntryEditor(string _hostName, string _location)
        {
            HostName = _hostName;
            Location = _location;
        }
        
        [TableColumnWidth(300, Resizable = false), Button("Get Now")]
        public void Time()
        {
            if (string.IsNullOrEmpty(HostName))
            {
                Status = "<No addresses filled>";
            }
            else
            {
                TimeServerStatus status = InternetConnectivityManager.GetLocalDateTimeFromServer(HostName);

                Status = status.Success ? status.LocalDateTime.ToString_DDMMYYHHMMSS() : "<Failed to fetch>";
            }
        }
    }
}
#endif