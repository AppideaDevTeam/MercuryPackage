#if MERCURY_INTERNETCONNECTIVITY
using System;
using System.Collections.Generic;
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

        private static List<PingEntry>                PingEntries;
        private static List<TimeServerEntry>          TimeServerEntries;
        public static  bool                           InternetConnectionEstablished { get; private set; }
        private static float                          LastPingTime;
        private static InternetConnectivityDatabaseSO Database => MercuryLibrarySO.InternetConnectivityDatabase;

        #endregion

        public static void Initialize()
        {
            PingEntries = new List<PingEntry>();

            foreach (var pingEntryEditor in Database.PingEntries)
                PingEntries.Add(new PingEntry(pingEntryEditor.IPAddress));

            TimeServerEntries = new List<TimeServerEntry>();

            foreach (var timeServerEntryEditor in Database.TimeServerEntries) TimeServerEntries.Add(new TimeServerEntry(timeServerEntryEditor.HostName));
        }

        public static bool CheckInternetConnection()
        {
            // Ppreventin pinging abuse
            if (1000 * (Time.realtimeSinceStartup - LastPingTime) > Database.PingGateInMilliseconds) InternetConnectionEstablished = PingAll();

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

                    var status = GetLocalDateTimeFromServer(entry.HostName);

                    TimeServerEntries[threadSafeIndex].Status = status;
                });
            }

            // WAIT FOR FIRST COMPLETED TASK AND CHECK FOR SUCCESS
            for (var index = 0; index < tasks.Length; index++)
            {
                Task.WaitAny(tasks);

                var entry = TimeServerEntries[index];

                if (entry.Status.Success) return entry.Status;
            }

            return new TimeServerStatus(false, DateTime.MaxValue);
        }

        internal static DateTime ParseTimeServerResponse(string _response)
        {
            var result = new DateTime();

            var year  = int.Parse(_response.Substring(7,  2));
            var month = int.Parse(_response.Substring(10, 2));
            var day   = int.Parse(_response.Substring(13, 2));

            var hour   = int.Parse(_response.Substring(16, 2));
            var minute = int.Parse(_response.Substring(19, 2));
            var second = int.Parse(_response.Substring(22, 2));

            result = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc).ToLocalTime();

            return result;
        }

        internal static TimeServerStatus GetLocalDateTimeFromServer(string _hostName)
        {
            var status = new TimeServerStatus(false, DateTime.MaxValue);

            try
            {
                var tcpClient = new TcpClient(_hostName, 13);

                using var streamReader = new StreamReader(tcpClient.GetStream());

                var responseString = streamReader.ReadToEnd();

                var localDateTime = ParseTimeServerResponse(responseString);

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
            Status    = new PingStatus(false, int.MaxValue);
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
            Status   = new TimeServerStatus(false, DateTime.MaxValue);
        }
    }

    public class TimeServerStatus
    {
        public bool     Success;
        public DateTime LocalDateTime;

        public TimeServerStatus(bool _success, DateTime _localDateTime)
        {
            Success       = _success;
            LocalDateTime = _localDateTime;
        }
    }

    #endregion
}
#endif