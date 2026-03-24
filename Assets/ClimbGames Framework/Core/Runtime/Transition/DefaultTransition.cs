using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ClimbGames
{
    [SingletonConfig("Resources/DefaultTransition")]
    public class DefaultTransition : MonoSingleton<DefaultTransition>, ITransitionHandler
    {
        [SerializeField] private CanvasGroup root;
        [Range(0f, 1f)][SerializeField] private float progress = 0f;

        public bool IsRunning { get; private set; }

        async UniTaskVoid Start()
        {
            await UniTask.NextFrame();
            gameObject.SetActive(false);
        }

        async UniTask Do(float startValue, float endValue, float duration, CancellationToken cancellationToken = default)
        {
            // 중복 실행 방지 (필요 시 기존 작업 취소 로직 추가 가능)
            if (IsRunning)
                return;
            IsRunning = true;

            gameObject.SetActive(true);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // 0 ~ 1 사이의 진행률 계산
                float normalizedTime = Mathf.Clamp01(elapsed / duration);

                // Ease 적용 (EaseManager가 있다면 사용, 없다면 Mathf.SmoothStep 등 활용)
                // progressRatio = EaseManager.Evaluate(Ease.OutQuad, null, elapsed, duration, 1f, 1f);
                progress = Mathf.SmoothStep(0f, 1f, normalizedTime);

                // progress에 따른 실제 알파값 적용
                root.alpha = Mathf.Lerp(startValue, endValue, progress);

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            // 최종 값 보정
            root.alpha = endValue;
            progress = 1f;
            IsRunning = false;

            // 페이드 인(사라지기) 완료 시 오브젝트 비활성화
            if (endValue <= 0f)
                gameObject.SetActive(false);
        }

        UniTask ITransitionHandler.BeginAsync(ISceneParameter sceneParameter)
        {
            return Do(0f, 1f, FrameworkSettings.Instance.DefaultTransitionTime, this.GetCancellationTokenOnDestroy());
        }

        void ITransitionHandler.Complete()
        {
            Do(1f, 0f, FrameworkSettings.Instance.DefaultTransitionTime, this.GetCancellationTokenOnDestroy()).Forget();
        }

        void IDisposable.Dispose()
        {

        }

        void ITransitionHandler.Finally()
        {

        }

        void ITransitionHandler.Transition(AsyncOperation asyncOperation)
        {
            // AsyncOperation 처리 (로딩 진행률)
        }
    }
}