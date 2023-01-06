using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.SceneManagement;

#if MERCURY_SCENEMANAGEMENT

public abstract class SceneContainerBase<EnumType> : SerializedScriptableObject
{
    [OdinSerialize] [TableList] private Dictionary<EnumType, Scene> enumToSceneDictionary = new Dictionary<EnumType, Scene>();
    
    public List<string> scenes = new List<string>();

    [Button]
    public void GetScenes()
    {
        scenes = new List<string>();
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            scenes.Add(System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)));
        }
    }
}

#endif