#if MERCURY_INTERNETCONNECTIVITY
using System;
using UnityEngine;

namespace Mercury.InternetConnectivity
{
    public class InternetConnectivityControllerSample : InternetConnectivityController
    {
        private static string log = "";

        public static void LogMsg(string _msg)
        {
            log += _msg + '\n';
        }
        
        private void OnGUI()
        {
            GUI.skin.label.fontSize = 60;
            GUI.Label(new Rect(20,20, 1000, 1000), log);   
        }

        private void Start()
        {
            Initialize();
            RunInternetCheckLoop();    
        }

        protected override void InternetConnectionEstablished() { LogMsg("Internet Connection Established"); }

        protected override void InternetConnectionNotEstablished(bool _retrying)
        {
            if(_retrying) LogMsg("Internet Connection NOT Established - RETRYING");
            else LogMsg("Internet Connection NOT Established FINAL VERDICT!");
        }
        protected override void InternetConnectionRestored() { LogMsg("Internet Connection Restored"); }
        protected override void InternetConnectionLost() { LogMsg("Internet Connection Lost"); }
        protected override void InternetTimeFetched(DateTime _localDateTime) { LogMsg("Internet Time Fetched"); }
        protected override void InternetTimeNotFetched() { LogMsg("Internet Time NOT Fetched"); }
        protected override void InternetTimeRenewed(DateTime _localDateTime) { LogMsg("Internet Time Renewed"); }
        protected override void InternetTimeNotRenewed() { LogMsg("Internet Time NOT Renewed"); }
    }
}
#endif
