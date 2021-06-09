#if MERCURY_INTERNETCONNECTIVITY

using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using UnityEngine;
using Ping = System.Net.NetworkInformation.Ping;

namespace Mercury.InternetConnectivity
{
    internal static class InternetConnectivityManager
    {
        #region VARIABLES
        private static List<PingEntry> PingEntries;

        public static bool InternetConnectionEstablished { get; private set; }

        private static float LastPingTime;
        
        private static InternetConnectivityDatabaseSO Database => MercuryLibrarySO.InternetConnectivityDatabase;
        #endregion

        public static void Initialize()
        {
            PingEntries = new List<PingEntry>();

            foreach (PingEntryEditor pingEntryEditor in Database.PingEntries)
                PingEntries.Add(new PingEntry(pingEntryEditor.IPAddress));
        }

        public static bool CheckInternetConnection()
        {
            // Ppreventin pinging abuse
            if (1000 * (Time.realtimeSinceStartup - LastPingTime) > Database.PingGateInMilliseconds)
            {
                InternetConnectionEstablished =  PingAll();
            }

            return InternetConnectionEstablished;
        }
        
        private static bool PingAll()
        {
            // STORE PING TIME
            LastPingTime = Time.realtimeSinceStartup;
            
            var tasks = new Task[Database.PingEntries.Count];

            // PING ALL ENTRY IPs
            for (var index = 0; index < tasks.Length; index++)
            {
                var threadSafeIndex = index;

                tasks[index] = Task.Run(() =>
                {
                    var status = Ping(PingEntries[threadSafeIndex].IPAddress);

                    PingEntries[threadSafeIndex].Status = status;
                });
            }

            // WAIT FOR FIRST COMPLETED TASK AND CHECK FOR SUCCESS
            for (var index = 0; index < tasks.Length; index++)
            {
                Task.WaitAny(tasks);
                if (PingEntries[index].Status.Success)
                    return true;
            }

            // COULDN'T CONNECT
            return false;
        }

        internal static PingStatus Ping(string _ip)
        {
            var status = new PingStatus(false, int.MaxValue);

            try
            {
                var ping  = new Ping();
                var reply = ping.Send(_ip, Database.PingTimeoutMilliseconds);

                if (reply.Status == IPStatus.Success)
                {
                    status.Success = true;
                    status.Delay   = (int) reply.RoundtripTime;
                }
            }
            catch
            {
                Debug.Log($"Failed to ping IP address: {_ip}");
            }

            return status;
        }
    }
    
    #region DATA CLASSES
    
    public class PingEntry
    {
        public readonly string     IPAddress;
        public          PingStatus Status;

        public PingEntry(string _ipAddress) { IPAddress = _ipAddress; }
    }
    
    
    public class PingStatus
    {
        public bool Success;
        public int  Delay;

        public PingStatus(bool _success, int _delay)
        {
            Success = _success;
            Delay   = _delay;
        }
    }
    #endregion
}
#endif