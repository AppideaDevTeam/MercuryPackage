using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;

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
    }
}
