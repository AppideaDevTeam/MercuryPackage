using System;
using UnityEngine;

namespace Mercury
{
    public abstract class MercurySingletonMonoBehaviour<T> : ExternalDependencies.SingletonMonoBehaviour<T> where T : MercurySingletonMonoBehaviour<T>
    {
        public static      bool HasInstance;
        protected override bool DontDestroyFlag => false;

        protected override void Awake()
        {
            base.Awake();
            HasInstance = true;
        }

        private void OnDestroy()
        {
            HasInstance = false;
        }
    }
    
    public abstract class MercurySingletonScriptableObject<T> : ScriptableObject where T : MercurySingletonScriptableObject<T>
    {
        public static bool HasAsset;
        
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
                        throw new Exception($"Mercury Scriptable Object ({typeName}) Not Found at Resource Folder");

                    HasAsset = true;
                }

                return instance;
            }
        }
    }
}