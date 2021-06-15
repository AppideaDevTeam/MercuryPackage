using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mercury
{
    public class MercuryLibrarySO : MercuryScriptableObjectInstanceReferencer<MercuryLibrarySO>
    {
        
        #region UI COLORS

        internal const string color_Violet = "#ff66ff";
        internal const string color_Orange = "#ff9900";
        
        public static Color Color_Violet => ParseHexColorString(color_Violet);
        public static Color Color_Orange => ParseHexColorString(color_Orange);
        private static Color ParseHexColorString(string _hex)
        {
            ColorUtility.TryParseHtmlString(_hex, out Color color);
            return color;
        }

        #endregion
        
        #if UNITY_EDITOR
        #region VARIABLES
        public ModuleSetting Module_LocalNotifications = new ModuleSetting("Local Notifications", "MERCURY_LOCALNOTIFICATIONS");
        public ModuleSetting Module_InternetConnectivity = new ModuleSetting("Internet Connectivity", "MERCURY_INTERNETCONNECTIVITY");
        #endregion

        [Serializable]
        public class ModuleSetting
        {
            [HorizontalGroup("Group", Width = 80), TitleGroup("Group/Activated"), HideLabel, OnInspectorInit("@Activated = ActivationStatusRequired()"), OnValueChanged("ActivationStatusChanged")] public bool Activated;
            [HorizontalGroup("Group", Width = 300), TitleGroup("Group/Name"), HideLabel, ReadOnly] public string Name;
            [HorizontalGroup("Group"), TitleGroup("Group/Definition"), HideLabel, ReadOnly, GUIColor(1,0.4f,1)] public string Definition;
            
            private void ActivationStatusChanged()
            {
               if (Activated) MercuryInstaller.Install(Definition);
               else MercuryInstaller.Uninstall(Definition);
            }
            
            private bool ActivationStatusRequired()
            {
                return MercuryInstaller.IsInstalled(Definition);
            }

            public ModuleSetting(string _name, string _definition)
            {
                Name = _name;
                Definition = _definition;
            }
        }
        #endif
        
        public static T GetModuleDatabase<T>(T _so) where T : ScriptableObject
        {
            if (_so == null)
            {
                MercuryDebugger.LogMessage(LogModule.Core, $"Make sure {typeof(T).Name} reference isn't NULL!", LogType.Exception);
                return null;
            }

            return _so;
        }
        
        #region GLOBAL ACCESSORS

        #if MERCURY_LOCALNOTIFICATIONS

        public static LocalNotifications.LocalNotificationsDatabaseSO LocalNotificationsDatabase => GetModuleDatabase(Instance.localNotificationsDatabase);

        [TitleGroup("Scriptable Object References"), LabelText("Local Notifications"), LabelWidth(200)] 
        public LocalNotifications.LocalNotificationsDatabaseSO localNotificationsDatabase;
        #endif
        
        #if MERCURY_INTERNETCONNECTIVITY
        public static InternetConnectivity.InternetConnectivityDatabaseSO InternetConnectivityDatabase => GetModuleDatabase(Instance.internetConnectivityDatabase);
        
        [TitleGroup("Scriptable Object References"), LabelText("Internet Connectivity"), LabelWidth(200)] 
        public InternetConnectivity.InternetConnectivityDatabaseSO internetConnectivityDatabase;
        #endif

        #endregion
    }
}