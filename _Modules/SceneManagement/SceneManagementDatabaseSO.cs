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

        [HorizontalGroup("Scenes", Width = 600)]
        [TableList(ShowIndexLabels = true, ShowPaging = true, AlwaysExpanded = true)]
        public List<SceneData> scenes;

        [Button]
        [HorizontalGroup("Buttons in Boxes", Width = 307)]
        public void GenerateEnums()
        {
            ScenesEnumGenerator.GenerateEnums();
        }
        
        [Button]
        [HorizontalGroup("Buttons in Boxes", Width = 293)]
        public void UpdateScenesList()
        {
            scenes ??= new List<SceneData>();

            foreach (int i in Enum.GetValues(typeof(Scenes)))
            {
                // IF ALREADY CONTAINS
                bool contains = scenes.Any(s => s.Scene == (Scenes) i);
                if(contains) continue;
                
                // ELSE INSERT TO LIST
                scenes.Insert(i,new SceneData((Scenes) i));
            }
        }
    }
}
#endif
