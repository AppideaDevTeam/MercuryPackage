using System;
using Sirenix.OdinInspector;

namespace Mercury
{
    public class MercuryLibrarySO : MercurySingletonScriptableObject<MercuryLibrarySO>
    {
        #region Simple Singleton Pattern with Hidden Instance

        public const string MercuryUIColor = "#ff66ff";

        public ModuleSetting Module_LocalNotifications = new ModuleSetting("Local Notifications", "MERCURY_LOCALNOTIFICATIONS");
        public ModuleSetting Module_InternetConnectivity = new ModuleSetting("Internet Connectivity", "MERCURY_INTERNETCONNECTIVITY");
        #endregion

        [Serializable]
        public class ModuleSetting
        {
            public string abaShecvale = "asdada";
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
        
        #region GLOBAL ACCESSORS

        #if MERCURY_LOCALNOTIFICATIONS
        public static LocalNotifications.LocalNotificationsDatabaseSO LocalNotificationsDatabase => Instance.localNotificationsDatabase;
        
        [TitleGroup("Scriptable Object References"), LabelText("Local Notifications"), LabelWidth(200)] 
        public LocalNotifications.LocalNotificationsDatabaseSO localNotificationsDatabase;
        #endif
        
        #if MERCURY_INTERNETCONNECTIVITY
        public static InternetConnectivity.InternetConnectivityDatabaseSO InternetConnectivityDatabase => Instance.internetConnectivityDatabase;
        
        [TitleGroup("Scriptable Object References"), LabelText("Internet Connectivity"), LabelWidth(200)] 
        public InternetConnectivity.InternetConnectivityDatabaseSO internetConnectivityDatabase;
        #endif

        #endregion
    }
}