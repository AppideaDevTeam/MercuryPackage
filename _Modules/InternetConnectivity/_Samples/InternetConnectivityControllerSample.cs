#if MERCURY_INTERNETCONNECTIVITY
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            // MANAGER RESPONCES
            GUIStyle guiStyle = new GUIStyle(GUI.skin.label);
            guiStyle.normal.textColor = Color.white;
            guiStyle.fontSize = 25;
            
            GUI.Label(new Rect(20,20, 3000, 2000), logBuffer, guiStyle);   
            
            // SHOW TIME
            if (TimeInfo.WasFetched && ConnectionInfo.ConnectionEstablished)
            {
                guiStyle.fontSize = 30;
                int vStep = 40;
                string fetchTime      = TimeInfo.DateTime.ToString();
                string calculatedTime = GetUpdatedInternetTime.ToString();
                GUI.Label(new Rect(Screen.width - 560, 20 + vStep*0, 550, 60), "Fetch Time:      " + fetchTime,      guiStyle);
                GUI.Label(new Rect(Screen.width - 560, 20 + vStep*1, 550, 60), "Calculated:       " + calculatedTime, guiStyle);
                GUI.Label(new Rect(Screen.width - 560, 20 + vStep*2, 550, 60), "Local Comp:     "   + DateTime.Now,   guiStyle);
                GUI.Label(new Rect(Screen.width - 560, 20 + vStep*3, 550, 60), "Last Fetch Time:            "   + TimeInfo.LastFetchTime,   guiStyle);
                GUI.Label(new Rect(Screen.width - 560, 20 + vStep*4, 550, 60), "RealtimeSinceStartup:   "   + Time.realtimeSinceStartup,   guiStyle);
            }
            
            // CLEAR LOGS BUTTON
            guiStyle = new GUIStyle(GUI.skin.button);
            guiStyle.fontSize = 25;
            if (GUI.Button(new Rect(Screen.width - 210, Screen.height - 70, 200, 60), "Clear Logs", guiStyle))
            {
                logBuffer = String.Empty;
            }
        }
        
        private void Start()
        {
            Debug.Log("app restarted");
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
