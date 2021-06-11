using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mercury
{
    public abstract class MercuryMonoBehaviorInstanceReferencer<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static bool HasInstance => instance == null;
        
        private static T instance;
        
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    MercuryDebugger.LogMessage(LogModule.Core, $"Instance of <color=red>{typeof(T).Name}</color> doesn't exist.", LogType.Exception);
                }

                return instance;
            }
        }

        protected void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
            }
        }
    }
}
