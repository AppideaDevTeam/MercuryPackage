#if MERCURY_SCENEMANAGEMENT
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mercury.SceneManagement
{
    public class SceneManagementDatabaseSO : ScriptableObject
    {
        private List<string> sceneNames;
        
        [FoldoutGroup("Scenes")]
        [InfoBox("Make sure only ONE scene is marked as BOOT!")] // TODO SHOW WARNING WHEN NOT COMPLIANT
        [TableList(ShowIndexLabels = true, ShowPaging = true, AlwaysExpanded = true)]
        public List<SceneData> scenes;

        [FoldoutGroup("Scenes")]
        [HorizontalGroup("Scenes/Buttons")]
        [Button("Generate Enums")]

        public void Button_GenerateEnums()
        {
            ScenesEnumGenerator.GenerateEnums();
        }

        [FoldoutGroup("Scenes")]
        [ReadOnly]
        [GUIColor("@MercuryLibrarySO.Color_Violet")]
        [LabelWidth(130)]
        [LabelText("Scene enums path: ")]
        public string enumAssetPath = "Assets/Plugins/_Mercury/Resources/Scene Management/ScenesEnumContainer.cs";

        [FoldoutGroup("Scenes")]
        [HorizontalGroup("Scenes/Buttons")]
        [Button("Update Scenes List")]
        public void Button_UpdateScenesList()
        {
            scenes ??= new List<SceneData>();

            for (var i = 0; i < sceneNames.Count; i++)
            {
                bool contains = scenes.Any(s => s.Scene == sceneNames[i]);
                if(contains) continue;
                
                // ELSE INSERT TO LIST
                scenes.Insert(i, new SceneData(sceneNames[i]));
            }
            
            // TODO  Reorder
        }

        public void UpdateScenesNames(string[] _names)
        {
            sceneNames = _names.ToList();
        }
    }
}
#endif
