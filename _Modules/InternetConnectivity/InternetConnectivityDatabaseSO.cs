#if MERCURY_INTERNETCONNECTIVITY

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mercury.InternetConnectivity
{
    public class InternetConnectivityDatabaseSO : ScriptableObject
    {
        [ShowInInspector, TitleGroup("Ping"), TableList(AlwaysExpanded = true), HideLabel] public List<PingEntryEditor> PingEntries = new List<PingEntryEditor>
        {
            new PingEntryEditor("1.1.1.1", "Cloudflare"), 
            new PingEntryEditor("8.8.8.8", "google-public-dns-a.google.com"), 
            new PingEntryEditor("8.8.4.4", "google-public-dns-b.google.com")
        };

        [TitleGroup("Ping/Global Settings"), LabelWidth(200), InfoBox("This field will used to prevent abusing servers with ping requests.", InfoMessageType.Warning)] 
        public int PingGateInMilliseconds = 800;
        [TitleGroup("Ping/Global Settings"), LabelWidth(200)] 
        public int PingTimeoutMilliseconds          = 1000;
    }

    [Serializable]
    public class PingEntryEditor
    {
        [HideLabel] public string IPAddress;
        [HideLabel] public string Info;

        [HideLabel, ReadOnly, GUIColor(1.0f, 0.4f, 1f)] public int PingDelay;

        public PingEntryEditor(string _ipAddress, string _info)
        {
            IPAddress = _ipAddress;
            Info = _info;
        }

        [Button("Check Now")]
        public void Ping()
        {
            if (!string.IsNullOrEmpty(IPAddress))
            {
                PingDelay = InternetConnectivityManager.Ping(IPAddress).Delay;
            }
        }
    }
}
#endif