#if MERCURY_SCENEMANAGEMENT
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mercury.SceneManagement
{
    [CreateAssetMenu(menuName = "My Assets/SceneManagementSO")]
    public class SceneManagementDatabaseSO : ScriptableObject
    {

        [Button]
        public void Generate()
        {
            ScenesEnumGenerator.ExampleScript();
        }
    }
}
#endif
