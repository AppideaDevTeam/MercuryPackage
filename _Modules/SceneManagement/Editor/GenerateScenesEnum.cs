#if UNITY_EDITOR
using System;
using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class GenerateScenesEnum
{
    private static string sceneEnumContainerAssetName = "ScenesEnumContainer";
    private static string enumName = "Scenes";
    
    // Get scene names from build settings
    public static List<string> GetScenesFromBuildSettings()
    {
        List<string> scenes = new List<string>();
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            scenes.Add(System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)));
            
        return scenes;
    }
    
    [MenuItem("Tools/Appidea/Update Scenes Enum")]
    static void ExampleScript()
    {
        // GET ASSET GUIDS
        string[] guids = AssetDatabase.FindAssets(sceneEnumContainerAssetName);
        
        // IF DIDN'T FOUND
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("Scenes enum updater", 
                                        "Scenes enum container NOT FOUND! \n \n" + 
                                                               "Before generating enums make sure you have ONE script in your project named - " + sceneEnumContainerAssetName, 
                                        "OK");
            return;
        }
        if (guids.Length > 1)
        {
            EditorUtility.DisplayDialog("Scenes enum updater", "Found more than 1 asset with same name!", "OK");
            return;
        }

        List<string> assetPaths = new List<string>();
        foreach (string guid in guids)
        {
            assetPaths.Add(AssetDatabase.GUIDToAssetPath(guid));
        }

        string[] enumEntries = GetScenesFromBuildSettings().ToArray();
        string assetPath = assetPaths[0];
        string enumDataToWrite = GenerateEnumStringData(enumEntries);
        
        using ( StreamWriter streamWriter = new StreamWriter( assetPath ) ) streamWriter.Write(enumDataToWrite);
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Scenes enum updater", "Scenes enum updated successfully!", "OK");
        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetPath);
    }

    
    // Generate enum string data
    private static string GenerateEnumStringData(string[] _enumEntries)
    {
        string body = "// Use INSTEAD Tools/Appidea/UpdateScenesEnum to update this enum";
        body += "\n public enum " + enumName;
        body += "\n {";
        
        for( int i = 0; i < _enumEntries.Length; i++ )
        {
            body += "\n" + "\t" + _enumEntries[i] + ",";
        }
        
        body += "\n }";

        return body;
    }
}
#endif