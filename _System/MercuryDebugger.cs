using System;
using UnityEngine;

namespace Mercury
{
    internal enum LogModule
    {
        Core,
            
        #if MERCURY_LOCALNOTIFICATIONS
        LocalNotifications,
        #endif
            
        #if MERCURY_INTERNETCONNECTIVITY
        InternetConnectivity
        #endif
    }

    internal enum LogType
    {
        Info, Warning, Error, Exception
    }
    
    internal static class MercuryDebugger
    {
        private static Action<string> registeredLoggers;

        public static void RegisterLogger(Action<string> _action) => registeredLoggers += _action;
        public static void UnregisterLogger(Action<string> _action) => registeredLoggers -= _action;

        public static void LogMessage(LogModule _logModule, string _message, LogType _logType = LogType.Info)
        {
            string moduleText = "Mercury_" + _logModule;
            string logText = $"<color={MercuryLibrarySO.color_Violet}>[{moduleText}]</color> {_message}";
            
            switch (_logType)
            {
                case LogType.Info: Debug.Log(logText); break;
                case LogType.Warning: Debug.LogWarning(logText); break;
                case LogType.Error: Debug.LogError(logText); break;
                case LogType.Exception: throw new Exception(logText);
            }

            registeredLoggers?.Invoke(_message);
        }
    }
}
