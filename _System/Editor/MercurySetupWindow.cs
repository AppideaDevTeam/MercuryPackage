#if UNITY_EDITOR
using System;
using System.Text;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Mercury
{
    public class MercurySetupWindow : OdinMenuEditorWindow
    {
        private   static MercurySetupWindow instance;

        [MenuItem("Tools/Mercury ֎/Mercury Setup ֎ %M", priority = 0)]
        public static void OpenWindow()
        {
            instance = GetWindow<MercurySetupWindow>();
            instance.Show();
        }
        
        #region CREATE DATABASES
        #if MERCURY_LOCALNOTIFICATIONS
        [MenuItem("Tools/Mercury ֎/Create Database/Local Notifications")]
        public static void CreateLocalNotificationDatabase() => MercuryInstaller.CreateScriptableObject<LocalNotifications.LocalNotificationsDatabaseSO>();
        #endif
        
        #if MERCURY_INTERNETCONNECTIVITY
        [MenuItem("Tools/Mercury ֎/Create Database/Internet Connectivity")]
        public static void CreateInternetConnectivityDatabase() => MercuryInstaller.CreateScriptableObject<InternetConnectivity.InternetConnectivityDatabaseSO>();
        #endif
        #endregion
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            instance = null;
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree();
            
            AddtoHierarchy<MercuryLibrarySO>(tree, "Main Config Referencer", MercuryEditorIcons.Mercury);

            #if MERCURY_LOCALNOTIFICATIONS
            AddtoHierarchy<LocalNotifications.LocalNotificationsDatabaseSO>(tree, "Modules/Local Notifications", MercuryEditorIcons.LocalNotifications);
            #endif
            
            #if MERCURY_INTERNETCONNECTIVITY
            AddtoHierarchy<InternetConnectivity.InternetConnectivityDatabaseSO>(tree, "Modules/Internet Connectivity", MercuryEditorIcons.InternetConnectivity);
            #endif
        
            return tree;
        }

        private static void AddtoHierarchy<T>(OdinMenuTree tree, string _path, string _base64Sprite = "") where T : UnityEngine.Object
        {
            var allAssetsOfTypeT = LoadAndReturnAll<T>();

            if (allAssetsOfTypeT == null)
            {
                MercuryInstaller.CreateScriptableObject<MercuryLibrarySO>();
                Debug.LogWarning("MecruryLibrarySO was not present, so it has been created an try opening Setup Window Again");
                return;
            }
        
            foreach (var item in allAssetsOfTypeT )
            {

                if (string.IsNullOrEmpty(_base64Sprite))
                {
                    tree.Add(_path, item);
                }

                else
                {
                    tree.Add(_path, item, Base64ToSprite(_base64Sprite));
                }
            }
        }
        
        public static Sprite Base64ToSprite(string _base64)
        {
            byte[]    imageBytes = Convert.FromBase64String(_base64);
            Texture2D tex        = new Texture2D(2, 2);
            tex.LoadImage( imageBytes );
            return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        }

        private static string SplitNameAndRemoveSOPrefix(string ScriptableObjectSoName)
        {
      
            var splitByUpperCase = Regex.Matches(ScriptableObjectSoName, "([A-Z][^A-Z]+)");
            if (splitByUpperCase.Count ==0) return ScriptableObjectSoName;
        
        
            var stringBuilder = new StringBuilder();
            foreach (var word in splitByUpperCase)
            {
                stringBuilder.Append(word);
                stringBuilder.Append(" ");
            }

            return stringBuilder.ToString();
        }
    
    
        private static T[] LoadAndReturnAll<T>() where T: UnityEngine.Object
        {
            var foundAssetPaths =AssetDatabase.FindAssets($"t:{typeof(T)}");
            if (foundAssetPaths.Length <= 0) return null;
        
            var resultArray = new T[foundAssetPaths.Length];
            for (var i = 0; i < foundAssetPaths.Length; i++)
            {
                resultArray[i] = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(foundAssetPaths[i]));
            }

            return resultArray;

        }
    }
}
#endif