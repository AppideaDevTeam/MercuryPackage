#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Mercury
{
    [InitializeOnLoad]
    internal static class MercuryInstaller
    {
        public static void Install(string _definition) => ProcessDefinition(_definition, true); 

        public static void Uninstall(string _definition) => ProcessDefinition(_definition, false);

        public static bool IsInstalled(string _definition)
        {
            string       defineBuffer = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> defineList   = defineBuffer.Split(';').ToList();

            return defineList.Contains(_definition);
        }

        private static void ProcessDefinition(string _definition, bool _install)
        {
            string       defineBuffer = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> defineList   = defineBuffer.Split(';').ToList();
            
            if (_install && !defineList.Contains(_definition)) defineList.Add(_definition);
            else if (!_install && defineList.Contains(_definition)) defineList.Remove(_definition);

            defineBuffer = string.Join(";", defineList);

            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineBuffer);
        }

        public static void CreateScriptableObject<T>() where T : ScriptableObject
        {
            const string path = "Assets/Plugins/_Mercury/Resources/";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            string fullPath = $"{path}{typeof(T).Name}.asset";
            
            if (File.Exists(fullPath)) return;

            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<T>(), fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static MercuryInstaller()
        {
            CreateScriptableObject<MercuryLibrarySO>();
        }

        #region PACKAGE UPDATER
        private static AddRequest updateRequest;
        private static ListRequest listRequest;
        private static bool packageInstallStatusValue;
        private static bool packageInstallStatusFetched;
        
        [MenuItem("Tools/Mercury ֎/Update Package %#M", priority = int.MaxValue)]
        public static async void UpdateSystemPackageRequest()
        {
            if (await IsMercuryPackageInstalled())
            {
                MercuryDebugger.LogMessage(LogModule.Core, $"Mercury Package Update In Progress!");
                
                updateRequest            =  Client.Add("https://github.com/AppideaDevTeam/MercuryPackage.git");
                EditorApplication.update += UpdateSystemPackageProgress;
            }
            else
            {
                MercuryDebugger.LogMessage(LogModule.Core, $"Mercury Should Be Installed As Package In The First Place!", LogType.Error);
            }
        }
        
        private static void UpdateSystemPackageProgress()
        {
            if (updateRequest.IsCompleted)
            {
                if (updateRequest.Status == StatusCode.Success)
                    MercuryDebugger.LogMessage(LogModule.Core, $"Mercury Package Update Successfully Installed!");
                else if (updateRequest.Status >= StatusCode.Failure)
                    MercuryDebugger.LogMessage(LogModule.Core, $"Mercury Package Update Failed!", LogType.Error);

                EditorApplication.update -= UpdateSystemPackageProgress;
            }
        }

        private static async Task<bool> IsMercuryPackageInstalled()
        {
            listRequest = Client.List();

            EditorApplication.update += MercuryPackageInstallCheckProgress;

            while (!packageInstallStatusFetched)  await Task.Delay(100);

            return packageInstallStatusValue;
        }

        private static void MercuryPackageInstallCheckProgress()
        {
            if (listRequest.IsCompleted)
            {
                if (listRequest.Status == StatusCode.Success)
                {
                    packageInstallStatusValue = listRequest.Result.ToList().Exists(package => package.name == "com.mercury.mercury.modules");
                    
                    EditorApplication.update -= MercuryPackageInstallCheckProgress;
                }
                
                packageInstallStatusFetched = true;
            }
        }
        #endregion
    }
}
#endif