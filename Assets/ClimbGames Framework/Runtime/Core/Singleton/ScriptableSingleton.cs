using System.IO;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ClimbGames
{
    public class ScriptableSingleton<T> : ScriptableObject where T : ScriptableSingleton<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var assetPath = typeof(T).GetCustomAttribute<AssetPathAttribute>();
                    if (assetPath != null)
                    {
                        int index = assetPath.Value.IndexOf("Resources/");
                        if (index > -1)
                        {
                            string resourcesPath = assetPath.Value.Substring(index + "Resources/".Length);
                            index = resourcesPath.LastIndexOf(".");
                            if (index > -1)
                                resourcesPath = resourcesPath.Substring(0, index);

                            _instance = Resources.Load<T>(resourcesPath);
                        }
                        else
                        {
                            // addressable 확인
                        }
#if UNITY_EDITOR
                        if (_instance == null)
                            _instance = AssetDatabase.LoadAssetAtPath<T>(assetPath.Value) ?? CreateAsset(assetPath.Value);
#endif
                    }
                }

                return _instance;
            }
        }

#if UNITY_EDITOR
        private static T CreateAsset(string assetPath)
        {
            if (assetPath.StartsWith("Assets/"))
            {
                string folderPath = Path.GetDirectoryName(assetPath);
                if (AssetDatabase.IsValidFolder(folderPath) == false)
                {
                    Directory.CreateDirectory(Path.Combine(Application.dataPath, "..", folderPath));
                    AssetDatabase.Refresh();
                }

                T instance = CreateInstance<T>();
                AssetDatabase.CreateAsset(instance, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return instance;
            }

            return default;
        }
#endif
    }
}