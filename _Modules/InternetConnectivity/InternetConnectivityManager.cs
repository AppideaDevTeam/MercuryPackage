#if MERCURY_INTERNETCONNECTIVITY
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
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
                TimeServerEntries.Add(new TimeServerEntry(timeServerEntryEditor.HostName));
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
                LogMessage($"Failed to ping IP address: {_ip}");
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
                    var entry = TimeServerEntries[threadSafeIndex];
                    
                    TimeServerStatus status = GetLocalDateTimeFromServer(entry.HostName);

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

        internal static DateTime ParseTimeServerResponse(string _response)
        {
            DateTime result = new DateTime();

            int year = int.Parse(_response.Substring(7, 2));
            int month = int.Parse(_response.Substring(10, 2));
            int day = int.Parse(_response.Substring(13, 2));
            
            int hour = int.Parse(_response.Substring(16, 2));
            int minute = int.Parse(_response.Substring(19, 2));
            int second = int.Parse(_response.Substring(22, 2));
            
            result = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc).ToLocalTime();
            
            return result;
        }

        internal static TimeServerStatus GetLocalDateTimeFromServer(string _hostName)
        {
            TimeServerStatus status = new TimeServerStatus(false, DateTime.MaxValue);
            
            try
            {
                TcpClient tcpClient = new TcpClient(_hostName, 13);

                using StreamReader streamReader = new StreamReader(tcpClient.GetStream());

                string responseString = streamReader.ReadToEnd();
                
                DateTime localDateTime = ParseTimeServerResponse(responseString);

                status.Success       = true;
                status.LocalDateTime = localDateTime;
            }
            catch
            {
                LogMessage($"Failed to fetch time from server with address: {_hostName}");
            }

            return status;
        }
        #endregion
        
        #region DEBUG
        public static void LogMessage(string _message)
        {
            if (Database.DebuggingEnabled) MercuryDebugger.LogMessage(LogModule.InternetConnectivity, _message);
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
        public string           HostName;
        public TimeServerStatus Status;

        public TimeServerEntry(string _hostName)
        {
            HostName = _hostName;
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