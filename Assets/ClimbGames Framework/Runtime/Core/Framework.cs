using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClimbGames
{
    public static class Framework
    {
        private static Camera _mainCamera;

        public static Camera MainCamera
        {
            get
            {
                if (_mainCamera == null)
                    return _mainCamera = Camera.main;

                return _mainCamera;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void RuntimeInitialize_AfterSceneLoad()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RuntimeInitialize_BeforeSceneLoad()
        {
            if (FrameworkSettings.Instance.UseDefaultTransition)
                SceneTransition.Initialize(DefaultTransition.Instance);
        }
        static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            Debug.Log($"[ClimbGames] OnSceneLoaded: {scene.name}/ {loadSceneMode}");
            _mainCamera = Camera.main;
        }
    }
}