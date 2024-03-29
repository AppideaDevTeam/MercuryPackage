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
        
        [TitleGroup("Tcp Servers"), TableList(AlwaysExpanded = true, ShowIndexLabels = true), HideLabel] 
        public List<TcpServerEntryEditor> TcpServerEntries = new List<TcpServerEntryEditor>
        {
            new TcpServerEntryEditor("1.1.1.1", 53, "Cloudflare"), 
            new TcpServerEntryEditor("8.8.8.8", 53, "google-public-dns-a.google.com"), 
            new TcpServerEntryEditor("8.8.4.4", 53, "google-public-dns-b.google.com")
        };

        [TitleGroup("Tcp Server Settings"), LabelWidth(200), InfoBox("This field will used to prevent abusing servers with connection requests.", InfoMessageType.Warning)] 
        public uint TcpServerGateInMilliseconds  = 800;
        [TitleGroup("Tcp Server Settings"), LabelWidth(200)] 
        public uint TcpServerTimeoutMilliseconds = 1000;

        [TitleGroup("Internet Connection System"), LabelText("Connection Checking in Loop:"), LabelWidth(200)]
        public bool InternetConnectionCheckingInLoop = true;
        [TitleGroup("Internet Connection System"), LabelText("Auto Retry on Establish Fail:"), LabelWidth(200)]
        public bool InternetConnectionAutoRetryOnFail = true;
        [TitleGroup("Internet Connection System"), ShowIf("InternetConnectionCheckingInLoop"), LabelText("Auto Retry on Lost:"), LabelWidth(200)]
        public bool InternetConnectionAutoRetryOnLost = true;
        [TitleGroup("Internet Connection System"), LabelText("Maximum Auto Retry:"), LabelWidth(200)]
        public uint InternetConnectionMaxAutoRetry = 4;
        [TitleGroup("Internet Connection System"), ShowIf("InternetConnectionCheckingInLoop"), LabelText("Connection Loop Interval:"), LabelWidth(200)]
        public uint InternetConnectionCheckingLoopInterval = 5;

        [TitleGroup("Time Settings"), LabelText("Periodic Renewal Enabled:"), LabelWidth(200)]
        public bool TimePeriodicRenewal = false;
        [TitleGroup("Time Settings"), ShowIf("TimePeriodicRenewal"), LabelText("Periodic Renewal Interval:"), LabelWidth(200), InfoBox("Some time servers have restrictions of minimum interval (Default ones provided here have 4 sec.)", InfoMessageType.Warning)]
        public uint TimePeriodicRenewalInterval = 30;

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
    public class TcpServerEntryEditor
    {
        public string IPAddress;
        public int    Port;
        public string Info;

        [ReadOnly, GUIColor("@MercuryLibrarySO.Color_Violet")] public string Status;

        public TcpServerEntryEditor(string _ipAddress, int _port, string _info)
        {
            IPAddress = _ipAddress;
            Port = _port;
            Info = _info;
        }

        [TableColumnWidth(300, Resizable = false), Button("Check Now")]
        public void Check()
        {
            if (string.IsNullOrEmpty(IPAddress))
            {
                Status = "<No address entered>";
            }
            else
            {
                var status = InternetConnectivityManager.CheckConnectionWithServer(IPAddress, Port);

                Status = status ? "OK" : "<Failed to connect>";
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

                Status = status.Success ? status.LocalDateTime.ToString_DDMMYYYYHHMMSS() : "<Failed to fetch>";
            }
        }
    }
}
#endif