#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Mercury.SceneManagement
{
    public static class ScenesEnumGenerator
    {
        public const string sceneEnumContainerAssetName = "ScenesEnumContainer";
        public const string enumName                    = "Scenes";

        // Get scene names from build settings
        public static List<string> GetScenesFromBuildSettings()
        {
            var scenes = new List<string>();
            for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                scenes.Add(Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)));

            return scenes;
        }

        public static void GenerateEnums()
        {
            var enumContainerPath = MercuryInstaller.mercuryResourcesPath + MercuryLibrarySO.Instance.Module_SceneManagement.Name + "/" + sceneEnumContainerAssetName +".cs";

            // CREATE NEW FILE IF DOESN'T EXIST
            if (!File.Exists(enumContainerPath))
            {
                File.WriteAllText(enumContainerPath, string.Empty);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            // GET ASSET GUIDS
            var guids = AssetDatabase.FindAssets(sceneEnumContainerAssetName);

            // IF DIDN'T FOUND
            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("Scenes enum updater",
                                            "Scenes enum container NOT FOUND! \n \n"                                         +
                                            "Before generating enums make sure you have ONE script in your project named - " + sceneEnumContainerAssetName,
                                            "OK");
                return;
            }

            if (guids.Length > 1)
            {
                EditorUtility.DisplayDialog("Scenes enum updater", "Found more than 1 asset with same name!", "OK");
                return;
            }

            var assetPaths = new List<string>();
            foreach (var guid in guids) assetPaths.Add(AssetDatabase.GUIDToAssetPath(guid));

            string[] enumEntries    = GetScenesFromBuildSettings().ToArray();
            string assetPath        = assetPaths[0];
            string enumDataToWrite  = GenerateEnumStringData(enumEntries);

            MercuryLibrarySO.SceneManagementDatabaseDatabase.UpdateScenesNames(enumEntries);
            
            using (var streamWriter = new StreamWriter(assetPath))
            {
                streamWriter.Write(enumDataToWrite);
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Scenes enum updater", "Scenes enum updated successfully!", "OK");
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetPath);
        }

        // Generate enum string data
        private static string GenerateEnumStringData(string[] _enumEntries)
        {
            var body = "#if MERCURY_SCENEMANAGEMENT";
            body += "\n";
            body += "\n// Use MERCURY CONTROL PANEL to update this enum";
            body += "\nnamespace Mercury.SceneManagement \n{";
            body += "\n\tpublic enum " + enumName;
            body += "\n\t{";

            for (var i = 0; i < _enumEntries.Length; i++)
            {
                body += "\n\t\t" + _enumEntries[i];
                if (i != _enumEntries.Length - 1) body += ",";
            }

            body += "\n\t}";
            body += "\n}";
            body += "\n#endif";

            return body;
        }
    }
}
#endif