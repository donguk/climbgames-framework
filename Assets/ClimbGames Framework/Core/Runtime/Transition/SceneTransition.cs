using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using ClimbGames.UI;
using System.Threading.Tasks;

namespace ClimbGames
{
    public enum TransitionState
    {
        None,
        LoadScene,
    }

    public static class SceneTransition
    {
        private const string EmptySceneName = "EmptyScene";
        private static ITransitionHandler transitionHandler;
        private static TransitionState transitionState;

        public static MonoScene MonoScene { get; private set; }
        public static bool IsPlaying => transitionState != TransitionState.None;

        public static void Initialize(ITransitionHandler handler)
        {
            transitionHandler = handler;
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
                await LoadEmptySceneIfUsed();

                MonoScene = await LoadSceneAsync(sceneName);
                await InitializeScene(MonoScene, param);

                if (transitionHandler != null)
                    transitionHandler.Complete();

                ActivateScene(MonoScene).Forget();
            }
            finally
            {
                if (transitionHandler != null)
                    transitionHandler.Finally();

                transitionState = TransitionState.None;
            }
            return true;
        }

        private static async UniTask LoadEmptySceneIfUsed()
        {
            if (FrameworkSettings.Instance.UseEmptyScene)
            {
                await SceneManager.LoadSceneAsync(EmptySceneName, LoadSceneMode.Single);
                await UniTask.NextFrame();
            }
        }

        public static async UniTask<MonoScene> LoadSceneAsync(string key)
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
            if (MonoScene == null)
                MonoScene = SceneManager.GetActiveScene().FindComponentInRootObjects<MonoScene>();

            if (MonoScene != null)
                MonoScene.Deactivate();
        }
    }
}