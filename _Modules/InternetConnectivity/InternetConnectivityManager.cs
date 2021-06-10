#if MERCURY_INTERNETCONNECTIVITY
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using Ping = System.Net.NetworkInformation.Ping;

namespace Mercury.InternetConnectivity
{
    internal static class InternetConnectivityManager
    {
        #region VARIABLES
        private static List<PingEntry> PingEntries;
        private static List<TimeServerEntry> TimeServerEntries;

        public static bool InternetConnectionEstablished { get; private set; }

        private static float LastPingTime;
        
        private static InternetConnectivityDatabaseSO Database => MercuryLibrarySO.InternetConnectivityDatabase;
        #endregion

        public static void Initialize()
        {
            PingEntries = new List<PingEntry>();

            foreach (PingEntryEditor pingEntryEditor in Database.PingEntries)
                PingEntries.Add(new PingEntry(pingEntryEditor.IPAddress));
            
            TimeServerEntries = new List<TimeServerEntry>();

            foreach (TimeServerEntryEditor timeServerEntryEditor in Database.TimeServerEntries)
            {
                var targetServerAddress = timeServerEntryEditor.GetTargetServerAddress();
                
                TimeServerEntries.Add(new TimeServerEntry(targetServerAddress));
            }
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
        
        #region PING
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
        #endregion

        #region TIME
        public static TimeServerStatus GetLocalDateTime()
        {
            var tasks = new Task[Database.TimeServerEntries.Count];
            
            for (var index = 0; index < tasks.Length; index++)
            {
                var threadSafeIndex = index;

                tasks[index] = Task.Run(() =>
                {
                    TimeServerStatus status = GetLocalDateTimeFromServer(TimeServerEntries[threadSafeIndex].ServerAddress);

                    TimeServerEntries[threadSafeIndex].Status = status;
                });
            }

            // WAIT FOR FIRST COMPLETED TASK AND CHECK FOR SUCCESS
            for (var index = 0; index < tasks.Length; index++)
            {
                Task.WaitAny(tasks);

                TimeServerEntry entry = TimeServerEntries[index]; 
                
                if (entry.Status.Success) return entry.Status;
            }

            return new TimeServerStatus(false, DateTime.MaxValue);
        }
        
        internal static TimeServerStatus GetLocalDateTimeFromServer(string _serverAddress)
        {
            TimeServerStatus status = new TimeServerStatus(false, DateTime.MaxValue);
            
            try
            {
                TcpClient tcpClient = new TcpClient(_serverAddress, 13);

                using StreamReader streamReader = new StreamReader(tcpClient.GetStream());
                
                string responseString = streamReader.ReadToEnd();

                string responseDateTime = responseString.Substring(7, 17);
            
                DateTime localDateTime = DateTime.ParseExact(responseDateTime, "yy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture,
                                                             DateTimeStyles.AssumeUniversal);

                status.Success       = true;
                status.LocalDateTime = localDateTime;
            }
            catch (Exception exception)
            {
                Debug.Log($"Failed to fetch time from server with address: {_serverAddress}");
            }

            return status;
        }
        #endregion
    }
    
    #region DATA CLASSES
    
    public class PingEntry
    {
        public readonly string     IPAddress;
        public          PingStatus Status;

        public PingEntry(string _ipAddress)
        {
            IPAddress = _ipAddress;
            Status = new PingStatus(false, int.MaxValue);
        }
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

    public class TimeServerEntry
    {
        public string           ServerAddress;
        public TimeServerStatus Status;

        public TimeServerEntry(string _serverAddress)
        {
            ServerAddress = _serverAddress;
            Status = new TimeServerStatus(false, DateTime.MaxValue);
        }
    }

    public class TimeServerStatus
    {
        public bool     Success;
        public DateTime LocalDateTime;

        public TimeServerStatus(bool _success, DateTime _localDateTime)
        {
            Success = _success;
            LocalDateTime = _localDateTime;
        }
    }
    #endregion
}
#endif