using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace ClimbGames
{
    public static class SceneTransition
    {
        public enum State
        {
            None,
            LoadScene,
        }

        private const string EmptySceneName = "EmptyScene";
        private static ITransitionHandler transitionHandler;
        private static State transitionState;

        public static MonoScene MonoScene { get; private set; }
        public static bool IsPlaying => transitionState != State.None;

        public static void Initialize(ITransitionHandler handler)
        {
            transitionHandler = handler;
        }

        public static async UniTask<bool> LoadAsync(string sceneName, ISceneParameter parameter = null)
        {
            return await TransitionAsync(sceneName, parameter, null);
        }

        public static async UniTask<bool> TransitionAsync(string sceneName, ISceneParameter parameter = null)
        {
            return await TransitionAsync(sceneName, parameter, transitionHandler);
        }

        private static async UniTask<bool> TransitionAsync(string sceneName, ISceneParameter parameter, ITransitionHandler handler)
        {
            if (Application.exitCancellationToken.IsCancellationRequested)
                return false;

            if (transitionState != State.None)
                return false;

            transitionState = State.LoadScene;
            try
            {
                if (handler != null)
                    await handler.BeginAsync(parameter);

                DeactivateScene();
                await LoadEmptySceneIfUsed();

                MonoScene = await LoadSceneAsync(sceneName);
                await InitializeScene(MonoScene, parameter);

                if (handler != null)
                    handler.Complete();

                ActivateScene(MonoScene).Forget();
            }
            finally
            {
                if (handler != null)
                    handler.Finally();

                transitionState = State.None;
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

        private static async UniTask<MonoScene> LoadSceneAsync(string key, ITransitionHandler handler = null)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(key, LoadSceneMode.Single);
            if (handler != null)
                handler.Transition(asyncOperation);

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