using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ClimbGames
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        public static bool IsValid
        {
            get
            {
                return _applicationIsQuitting == false && _instance != null;
            }
        }

        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting) return null;

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (T)FindFirstObjectByType(typeof(T));
                        if (_instance == null)
                        {
                            GameObject singletonObject = null;

                            var assetPath = typeof(T).GetCustomAttribute<AssetPathAttribute>();
                            if (assetPath != null && string.IsNullOrEmpty(assetPath.Value) == false)
                            {
                                int index = assetPath.Value.IndexOf("Resources/");
                                if (index > -1)
                                {
                                    string path = assetPath.Value.Substring(index + "Resources/".Length);
                                    index = path.LastIndexOf(".");
                                    if (index > -1)
                                        path = path.Substring(0, index);

                                    GameObject prefab = Resources.Load<GameObject>(path);
                                    singletonObject = Instantiate(prefab);
                                }
                                else
                                {
                                    singletonObject = AssetManager.Instantiate(assetPath.Value);
#if UNITY_EDITOR
                                    if (singletonObject == null)
                                        singletonObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath.Value);
#endif
                                }
                            }

                            if (singletonObject == null)
                                singletonObject = new GameObject();

                            _instance = singletonObject.GetOrAddComponent<T>();
                            singletonObject.name = typeof(T).Name + " (Singleton)";
                        }
                    }

                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;

                var config = typeof(T).GetCustomAttribute<SingletonConfigAttribute>();
                if (config != null && config.DontDestroy)
                    DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            // 인스턴스가 파괴될 때 참조를 해제 (앱 종료가 아닐 때를 대비)
            if (_instance == this)
                _instance = null;
        }

        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}