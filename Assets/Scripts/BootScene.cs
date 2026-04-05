using System.Threading;
using System.Threading.Tasks;
using ClimbGames.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ClimbGames.Tset
{
    public class InitStep : TaskSequencer.StapBase
    {
        public override async UniTask<bool> Run(CancellationToken cancellationToken = default)
        {
            Progress = 0f;
            await UniTask.WaitForSeconds(0.1f);
            Progress = 1f;
            TaskSequencer.Default.Run(nameof(PatchStep));
            return true;
        }
    }

    public class VersionStep : TaskSequencer.StapBase
    {
        public override async UniTask<bool> Run(CancellationToken cancellationToken = default)
        {
            Progress = 0f;
            await UniTask.WaitForSeconds(0.1f);
            Progress = 1f;
            return true;
        }
    }

    public class PatchStep : TaskSequencer.StapBase
    {
        public override float Weight => 60f;

        public override async UniTask<bool> Run(CancellationToken cancellationToken = default)
        {
            Progress = 0f;
            for (int i = 0; i < 10; ++i)
            {
                await UniTask.NextFrame();
                Progress = i / 10f;
            }
            Progress = 1f;

            if (UIManager.Instance != null)
                Debug.Log("UIManager Init");
            return true;
        }
    }

    public class BootScene : MonoScene
    {
        void Start()
        {
            TaskSequencer.Default.AddStep(new InitStep())
                                .AddStep(new VersionStep())
                                .AddStep(new PatchStep())
                                .Start();
        }

        void Update()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.enterKey.wasReleasedThisFrame)
            {
                SceneTransition.TransitionAsync("01_Title").Forget();
            }
        }
    }
}