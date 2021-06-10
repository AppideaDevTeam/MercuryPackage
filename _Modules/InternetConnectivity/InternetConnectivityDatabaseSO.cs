#if MERCURY_INTERNETCONNECTIVITY

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mercury.InternetConnectivity
{

    public enum TimeServerAddressType
    {
        Global,
        IPv4,
        IPv6
    }

    public class InternetConnectivityDatabaseSO : ScriptableObject
    {
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
            new TimeServerEntryEditor(TimeServerAddressType.Global, "",             "",                   "time.nist.gov",       "Multiple locations"),
            new TimeServerEntryEditor(TimeServerAddressType.IPv4,   "132.163.97.1", "",                   "time-a-wwv.nist.gov", "WWV, Fort Collins, Colorado"),
            new TimeServerEntryEditor(TimeServerAddressType.IPv4,   "132.163.97.2", "",                   "time-b-wwv.nist.gov", "WWV, Fort Collins, Colorado"),
            new TimeServerEntryEditor(TimeServerAddressType.IPv4,   "132.163.97.3", "",                   "time-c-wwv.nist.gov", "WWV, Fort Collins, Colorado"),
            new TimeServerEntryEditor(TimeServerAddressType.IPv4,   "132.163.97.4", "2610:20:6f97:97::4", "time-d-wwv.nist.gov", "WWV, Fort Collins, Colorado"),
            new TimeServerEntryEditor(TimeServerAddressType.IPv4,   "132.163.97.6", "2610:20:6f97:97::6", "time-e-wwv.nist.gov", "WWV, Fort Collins, Colorado"),
            new TimeServerEntryEditor(TimeServerAddressType.IPv4,   "132.163.96.1", "",                   "time-a-b.nist.gov",   "NIST, Boulder, Colorado"),
            new TimeServerEntryEditor(TimeServerAddressType.IPv4,   "132.163.96.2", "",                   "time-b-b.nist.gov",   "NIST, Boulder, Colorado"),
            new TimeServerEntryEditor(TimeServerAddressType.IPv4,   "132.163.96.3", "",                   "time-c-b.nist.gov",   "NIST, Boulder, Colorado"),
            new TimeServerEntryEditor(TimeServerAddressType.IPv4,   "132.163.96.4", "2610:20:6f96:96::4", "time-d-b.nist.gov",   "NIST, Boulder, Colorado"),
            new TimeServerEntryEditor(TimeServerAddressType.IPv4,   "132.163.96.6", "2610:20:6f96:96::6", "time-e-b.nist.gov",   "NIST, Boulder, Colorado"),
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

        [Button("Check Now")]
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
        
        [TableColumnWidth(200, Resizable = false), EnumToggleButtons]
        public TimeServerAddressType AddressType;
        public string IPAddressV4;
        public string IPAddressV6;
        public string Name;
        public string Location;
        [ReadOnly, GUIColor("@MercuryLibrarySO.Color_Violet")] public string Status;

        public TimeServerEntryEditor(TimeServerAddressType _addressType, string _ipAddressV4, string _ipAddressV6, string _name, string _location)
        {
            AddressType = _addressType;
            IPAddressV4 = _ipAddressV4;
            IPAddressV6 = _ipAddressV6;
            Name = _name;
            Location = _location;
        }
        
        [Button("Get Now")]
        public void Time()
        {
            string targetServerAddress = GetTargetServerAddress();

            if (string.IsNullOrEmpty(targetServerAddress))
            {
                Status = "<No addresses filled>";
            }
            else
            {
                TimeServerStatus status = InternetConnectivityManager.GetLocalDateTimeFromServer(targetServerAddress);

                Status = status.Success ? status.LocalDateTime.ToString_DDMMYYHHMMSS() : "<Failed to fetch>";
            }
        }

        public string GetTargetServerAddress()
        {
            switch (AddressType)
            {
                case TimeServerAddressType.Global: return Name;
                case TimeServerAddressType.IPv4: return IPAddressV4;
                case TimeServerAddressType.IPv6: return IPAddressV6;
                default: return string.Empty;
            }
        }
    }
}
#endif