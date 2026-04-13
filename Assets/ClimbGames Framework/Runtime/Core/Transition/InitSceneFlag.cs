using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClimbGames
{
    public class InitSceneFlag : MonoBehaviour
    {
        private static InitSceneFlag instance;
        private static string startSceneName;

        public static bool DidCameFromInitScene => instance != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Initialize()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            startSceneName = activeScene.name;
        }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}