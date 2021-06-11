using UnityEngine;

namespace Mercury
{
    public abstract class MercuryScriptableObjectInstanceReferencer<T> : ScriptableObject where T : ScriptableObject
    {
        public static bool HasAsset => instance == null;

        private static T instance;

        public static T Instance
        {
            get
            {
                string typeName = typeof(T).Name;

                if (instance == null)
                {
                    instance = Resources.Load<T>(typeName);
                    
                    if (instance == null)
                        MercuryDebugger.LogMessage(LogModule.Core, $"Instance of <color=red>{typeof(T).Name}</color> doesn't exist.", LogType.Exception);
                }

                return instance;
            }
        }
    }
}
