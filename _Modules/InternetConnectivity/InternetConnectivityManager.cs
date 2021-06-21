#if MERCURY_INTERNETCONNECTIVITY
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Mercury.InternetConnectivity
{
    internal static class InternetConnectivityManager
    {
        #region VARIABLES

        private static List<TcpServerEntry>           TcpServerEntries;
        private static List<TimeServerEntry>          TimeServerEntries;
        
        internal static TimeInfo TimeInfo { get; private set; }
        internal static ConnectionInfo ConnectionInfo { get; private set; }
        public static bool IsInitialized { get; private set; }
        
        private static InternetConnectivityDatabaseSO Database => MercuryLibrarySO.InternetConnectivityDatabase;

        #endregion

        internal static void Reset()
        {
            IsInitialized = false;
            ConnectionInfo = null;
            TimeInfo = null;
            TcpServerEntries = null;
            TimeServerEntries = null;
            
            Initialize();
        }
        
        public static void Initialize()
        {
            // INITIALIZATION GATE
            if (IsInitialized) return;
            
            // TCP SERVER
            ConnectionInfo = new ConnectionInfo();
            
            TcpServerEntries = new List<TcpServerEntry>();

            foreach (TcpServerEntryEditor tcpServerEntryEditor in Database.TcpServerEntries)
                TcpServerEntries.Add(new TcpServerEntry(tcpServerEntryEditor.IPAddress, tcpServerEntryEditor.Port));

            // TIME
            TimeInfo = new TimeInfo();
            
            Debug.Log("TimeInfo: " + TimeInfo);
            Debug.Log("TimeInfo: " + TimeInfo.WasFetched);
            
            TimeServerEntries = new List<TimeServerEntry>();

            foreach (var timeServerEntryEditor in Database.TimeServerEntries)
                TimeServerEntries.Add(new TimeServerEntry(timeServerEntryEditor.HostName));

            IsInitialized = true;
        }

        public static void CheckInternetConnection()
        {
            // Prevent connection request abuse
            if (1000 * (Time.realtimeSinceStartup - ConnectionInfo.LastCheckTime) > Database.TcpServerGateInMilliseconds)
            {
                bool status = CheckConnection();
                ConnectionInfo.CurrentlyConnected = status;
                
                // IF RETRY
                if (status) ConnectionInfo.ConnectionEstablished = true;
            }
        }

        #region TCP SERVER

        private static bool CheckConnection()
        {
            // STORE TCP CONNECTION REQUEST TIME
            ConnectionInfo.LastCheckTime = Time.realtimeSinceStartup;

            var tasks = new Task[Database.TcpServerEntries.Count];

            // CONNECT TO ALL ENTRY IPs
            for (var index = 0; index < tasks.Length; index++)
            {
                var threadSafeIndex = index;

                tasks[index] = Task.Run(() =>
                {
                    var entry = TcpServerEntries[threadSafeIndex];

                    var status = CheckConnectionWithServer(entry.IPAddress, entry.Port);

                    TcpServerEntries[threadSafeIndex].Status = status;
                });
            }

            // WAIT FOR FIRST COMPLETED TASK AND CHECK FOR SUCCESS
            for (var index = 0; index < tasks.Length; index++)
            {
                Task.WaitAny(tasks);

                if (TcpServerEntries[index].Status) return true;
            }

            // COULDN'T CONNECT
            return false;
        }

        internal static bool CheckConnectionWithServer(string ipAddress, int _port)
        {
            var status = false;

            using var tcpClient = new TcpClient();
            
            try
            {
                var connectTask = tcpClient.ConnectAsync(ipAddress, _port);
                
                if (connectTask.Wait(TimeSpan.FromMilliseconds((int)Database.TcpServerTimeoutMilliseconds)))
                {
                    status = true;

                    LogMessage($"Successful connection to IP address: {ipAddress}");
                }
                else
                {                
                    LogMessage($"Unsuccessful connection to IP address: {ipAddress}");
                }
            }
            catch
            {
                LogMessage($"Failed to connection to IP address: {ipAddress}");
            }

            tcpClient.Close();

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
            var year  = int.Parse(_response.Substring(7,  2));
            var month = int.Parse(_response.Substring(10, 2));
            var day   = int.Parse(_response.Substring(13, 2));

            var hour   = int.Parse(_response.Substring(16, 2));
            var minute = int.Parse(_response.Substring(19, 2));
            var second = int.Parse(_response.Substring(22, 2));

            return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified).ToLocalTime();
        }
        
        internal static TimeServerStatus GetLocalDateTimeFromServer(string _hostName)
        {
            var status = new TimeServerStatus(false, DateTime.MaxValue);

            try
            {
                using var tcpClient = new TcpClient(_hostName, 13);

                using var streamReader = new StreamReader(tcpClient.GetStream());

                var responseString = streamReader.ReadToEnd();

                var localDateTime = ParseTimeServerResponse(responseString);

                status.Success       = true;
                status.LocalDateTime = localDateTime;

                streamReader.Close();
                tcpClient.Close();
            }
            catch
            {
                LogMessage($"Failed to fetch time from server with address: {_hostName}");
            }

            return status;
        }

        internal static void FetchInternetTime()
        {
            var timeServerStatus = GetLocalDateTime();
            TimeInfo.WasRenewed = timeServerStatus.Success && TimeInfo.WasFetched;
            TimeInfo.WasFetched = timeServerStatus.Success;
            TimeInfo.DateTime = timeServerStatus.LocalDateTime;
            TimeInfo.LastFetchTime = Time.realtimeSinceStartup;
        }
        
        internal static DateTime CalculateCurrentTime()
        {
            if (!TimeInfo.WasFetched) return DateTime.Now;
        
            TimeSpan deltaTime = TimeSpan.FromSeconds(Time.realtimeSinceStartup - TimeInfo.LastFetchTime);
            return TimeInfo.DateTime.Add(deltaTime);
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

    public class TcpServerEntry
    {
        public readonly string IPAddress;
        public readonly int    Port;
        public bool            Status;

        public TcpServerEntry(string _ipAddress, int _port)
        {
            IPAddress = _ipAddress;
            Port      = _port;
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
    
    [Serializable]
    public class TimeInfo
    {
        public bool     WasFetched;
        public bool     WasRenewed;
        public float    LastFetchTime;
        public DateTime DateTime;
    }
    
    [Serializable]
    public class ConnectionInfo
    {
        public bool  ConnectionEstablished;
        public bool  CurrentlyConnected;
        public float LastCheckTime;
    }
    #endregion
}
#endif