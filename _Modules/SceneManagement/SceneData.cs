#if MERCURY_SCENEMANAGEMENT
using System;

namespace Mercury.SceneManagement
{
    public enum SceneType
    {
        Boot,
        LoadingScreen,
        ScriptsOnly,
        Menu,
        Gameplay,
        Overlay
    }
    
    [Serializable]
    public class SceneData
    {
        public Scenes Scene;
        public SceneType Type;
        
        public SceneData(Scenes _scene)
        {
            Scene = _scene;
        }
    }
}
#endif
