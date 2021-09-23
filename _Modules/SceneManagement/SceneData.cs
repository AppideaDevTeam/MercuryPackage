#if MERCURY_SCENEMANAGEMENT
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Mercury.SceneManagement
{
    public enum SceneType
    {
        None,
        Boot,
        LoadingScreen,
        Helper,
        Menu,
        Lobby,
        Gameplay,
        Overlay
    }
    
    [Serializable]
    public class SceneData
    {
        [ReadOnly] public string Scene;
        public SceneType Type;
        //[TableColumnWidth(80, Resizable = false)] public bool Standalone = true;

        public SceneData(string _scene)
        {
            Scene = _scene;
        }
    }

    [Serializable]
    public class Transition
    {
        public SceneType TransitionTarget;
        public bool LoadAsAsync;
    }
}
#endif
