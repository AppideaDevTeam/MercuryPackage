using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mercury.SceneManagement
{
    public static class Ref
    {
        private static string assetPath => MercuryInstaller.mercuryResourcesPath                 +
                                            MercuryLibrarySO.Instance.Module_SceneManagement.Name + 
                                            "/" + ScenesEnumGenerator.sceneEnumContainerAssetName + ".cs";

        [MenuItem("Mercury/ReadFile")]
        public static void ReadFile()
        {
            if (!File.Exists(assetPath))
            {
                EditorUtility.DisplayDialog("OOPS!","File not found", "OK!");
                return;
            }
            
            string[] contentLines = File.ReadAllLines(assetPath);
            List<string> sceneNames = new List<string>();

            bool collectingSceneNames = false;
            
            foreach (var line in contentLines)
            {
                if (line.Contains($"public enum {ScenesEnumGenerator.enumName}"))
                {
                    collectingSceneNames = true;
                    continue;
                }
                if (line.Contains("{")) continue;
                if (collectingSceneNames && line.Contains("}")) collectingSceneNames = false;
                if (collectingSceneNames) sceneNames.Add(line.Trim('\t', ','));
            }

            string ALL_NAMES = "";
            foreach (var s in sceneNames) ALL_NAMES += s + " ";
            Debug.Log(ALL_NAMES);
        }
    }
}
