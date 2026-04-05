using UnityEngine;
using R3;
using System;
using System.Reflection;
using System.IO;

namespace ClimbGames
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        protected CompositeDisposable disposables = new();

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
                            if (assetPath != null)
                            {
                                string path = assetPath.Value;
                                int index = path.IndexOf("Resources/");
                                if (index > -1)
                                    path = path.Substring(index + "Resources/".Length);

                                index = path.LastIndexOf(".");
                                if (index > -1)
                                    path = path.Substring(0, index);

                                GameObject prefab = Resources.Load<GameObject>(path);
                                singletonObject = Instantiate(prefab);
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
            disposables.Dispose();

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