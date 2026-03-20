using System.Reflection;
using UnityEngine;

namespace ClimbGames.Core
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
                        string path = assetPath.Value;
                        int index = path.IndexOf("Resources/");
                        if (index > -1)
                            path = path.Substring(index + "Resources/".Length);

                        index = path.LastIndexOf(".");
                        if (index > -1)
                            path = path.Substring(0, index);

                        _instance = Resources.Load<T>(path);
                    }
                }

                return _instance;
            }
        }
    }
}