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
    
    internal static class MercuryDebugger
    {
        public static void LogMessage(LogModule _module, string _message)
        {
            string moduleText = "Mercury_" + _module;
            
            Debug.Log($"<color={MercuryLibrarySO.color_Violet}>[{moduleText}]</color> {_message}");
        }
    }
}
