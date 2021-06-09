#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Mercury
{
    internal static class MercuryModuleInstaller
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
    }
}
#endif
