using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mercury.ExternalDependencies
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        /// <summary>
        /// [True] Singleton Persists Amongst Scenes and Must Be Root Object (OnCreationProcess Called Once)
        /// <para>[False] Singleton Gets Destroyed and Recreated During Scene Loadings (OnCreationProcess Multiple Times)</para>
        /// </summary>
        protected virtual bool DontDestroyFlag=>false;
        
        private static T    instance;
        
        public static T Instance
        {
            get
            {
                if (instance != null) return instance;
                
                //In Case It's attached to any GameObject as Pre- created MonoBehaviour 
                var sceneRootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
                for (var i = 0; i < sceneRootObjects.Length; i++) {

                    var found = sceneRootObjects[i].GetComponentInChildren<T>(true);
                    if (found != default(T))
                    {
                        instance = found;
                        break;
                    }
                }

                //In Case it's not attached anywhere and we need to Create GameObject Automatically
                if (instance == null)
                {
                    var createdGameObject = new GameObject();
                    instance               = createdGameObject.AddComponent<T>();
                    createdGameObject.name = $"Singleton<{instance.GetType().Name}>";
                    if(instance.DontDestroyFlag) DontDestroyOnLoad(instance.gameObject);

                }

                instance?.OnCreationProcess();
                return instance;
            }

            set => instance = value;
        }
        
        /// <summary>
        /// Constructor Like Method Called on "instance" field during creation. Called before 'instance' is assigned to Instance property , so use "this." instead of directly accessing "Instance" property
        /// </summary>
        protected virtual void OnCreationProcess() { }

        //if instance is Enabled before it's Accessed , Look Up in scene Objects
        protected virtual void Awake()
        {
            if (instance == this) return;
            
            //If 2 Instances Exist (Primarly During Scene Changes , or by mistake)
            if (instance != null)
            {
                //IF Instances Have DontDestroyOnLoad => Disallow Second Instance to Exist 
                if (DontDestroyFlag)
                {
                    //Instance already exists but this is not instance (instace!=this && instance!=null) , so delete this
                    Destroy(this);
                }
                
                //IF Instances Don't Have DontDestroyOnLoad -> second instance probably got created in when we switched to new scene (means old get deleted) 
                //so we assign new instance from new scene
                else
                {
                    instance = this as T;
                    instance?.OnCreationProcess();
                }
            }
            
            //If Instance Doesn't Exist at all , just assign it 
            else
            {
                instance = this as T;
                instance?.OnCreationProcess();
            }

        }
    }
}
