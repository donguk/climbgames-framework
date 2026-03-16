using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace ClimbGames.Core
{
    public interface ISceneParameter
    {

    }

    public interface ITransitionHandler : IDisposable
    {
        UniTask BeginAsync(ISceneParameter sceneParameter);
        void Transition(AsyncOperation asyncOperation);
        void Complete();
        void Finally();
    }

    public enum TransitionState
    {
        None,
        LoadScene,
    }

    public class SceneTransition : MonoBehaviour
    {
        private const string EmptySceneName = "Empty";
        private static ITransitionHandler transitionHandler;
        private static TransitionState transitionState;

        public static Scene ActiveScene { get; private set; }
        public static MonoScene MonoScene { get; private set; }
        public static Camera MainCamera { get; private set; }

        public static bool IsPlaying => transitionState != TransitionState.None;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Initialize()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Scene activeScene = SceneManager.GetActiveScene();
            Debug.Log($"[SceneTransition] RuntimeInitializeLoadType.AfterSceneLoad: activeScene({activeScene.name})");

            MainCamera = Camera.main;
            Initialize(activeScene);
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            Debug.Log($"[SceneTransition] OnSceneLoaded: activeScene({scene.name})");

            MainCamera = Camera.main;
            Initialize(scene);
        }

        public static void Initialize(ITransitionHandler handler)
        {
            transitionHandler = handler;
        }

        static void Initialize(Scene activeScene)
        {
            MainCamera = Camera.main;
            ActiveScene = activeScene;
        }

        public static async UniTask<bool> TransitionAsync(string sceneName, ISceneParameter param = null)
        {
            if (Application.exitCancellationToken.IsCancellationRequested)
                return false;

            if (transitionState != TransitionState.None)
                return false;

            transitionState = TransitionState.LoadScene;
            try
            {
                if (transitionHandler != null)
                    await transitionHandler.BeginAsync(param);

                DeactivateScene();
                await LoadSceneAsync(EmptySceneName);

                var monoScene = MonoScene = await LoadSceneAsync(sceneName);
                await InitializeScene(monoScene, param);

                if (transitionHandler != null)
                    transitionHandler.Complete();

                ActivateScene(monoScene).Forget();
            }
            finally
            {
                if (transitionHandler != null)
                    transitionHandler.Finally();

                transitionState = TransitionState.None;
            }
            return true;
        }

        private static async UniTask<MonoScene> LoadSceneAsync(string key)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(key, LoadSceneMode.Single);
            if (transitionHandler != null)
                transitionHandler.Transition(asyncOperation);

            await asyncOperation;
            await UniTask.NextFrame();
            return SceneManager.GetActiveScene().FindComponentInRootObjects<MonoScene>();
        }

        private static async UniTask InitializeScene(MonoScene monoScene, ISceneParameter param)
        {
            if (monoScene != null)
                await monoScene.InitializeAsync();
        }

        private static async UniTask ActivateScene(MonoScene monoScene)
        {
            if (monoScene != null)
                await monoScene.ActivateAsync();
        }

        private static void DeactivateScene()
        {
            var monoScene = ActiveScene.FindComponentInRootObjects<MonoScene>();
            if (monoScene != null)
                monoScene.Deactivate();
        }
    }
}