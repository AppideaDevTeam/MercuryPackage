#if MERCURY_INTERNETCONNECTIVITY
using System;
using UnityEngine;

namespace Mercury.InternetConnectivity
{
    public class InternetConnectivityControllerSample : InternetConnectivityController
    {
        private static string logBuffer = "";

        private static void LogMessage(string _message)
        {
            logBuffer += $"{_message}\n";
        }
        
        private void OnGUI()
        {
            GUI.skin.label.fontSize = 35;
            GUI.Label(new Rect(20,20, 3000, 2000), logBuffer);   
        }
        
        private void Start()
        {
            MercuryDebugger.RegisterLogger(LogMessage);
            Initialize();
            RunInternetCheckLoop();
        }

        protected override void InternetConnectionEstablished() {}

        protected override void InternetConnectionNotEstablished(bool _retrying)
        {
            if(_retrying) InternetConnectivityManager.LogMessage("Internet Connection NOT Established - RETRYING");
            else InternetConnectivityManager.LogMessage("Internet Connection NOT Established FINAL VERDICT!");
        }
        protected override void InternetConnectionRestored() { }
        protected override void InternetConnectionLost(bool _retrying) { }
        protected override void InternetTimeFetched(DateTime _localDateTime) { }
        protected override void InternetTimeNotFetched() { }
        protected override void InternetTimeRenewed(DateTime _localDateTime) { }
        protected override void InternetTimeNotRenewed() { }
    }
}
#endif
