using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ClimbGames
{
    public partial class TaskSequencer : IDisposable
    {
        public static TaskSequencer Default = new TaskSequencer();

        private List<StepBase> steps = new List<StepBase>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public int CurrentIndex { get; private set; }
        public int NextIndex { get; private set; }
        public int TotalCount => steps.Count;
        public bool IsRunning { get; private set; }
        public float TotalProgress
        {
            get
            {
                if (steps.Count == 0)
                    return 1f;

                float totalWeight = 0f;
                float currentWeight = 0f;

                for (int i = 0; i < steps.Count; ++i)
                {
                    StepBase step = steps[i];
                    totalWeight += step.Weight;

                    float progress = i < CurrentIndex ? 1f : step.Progress;
                    currentWeight += progress * step.Weight;
                }

                return totalWeight > 0f ? currentWeight / totalWeight : 0f;
            }
        }

        public TaskSequencer AddStep(StepBase step)
        {
            steps.Add(step);
            return this;
        }

        public void RemoveSteps(string name)
        {
            steps.RemoveAll(step => step.Name == name);
        }

        public void RemoveStep(StepBase step)
        {
            steps.Remove(step);
        }

        async UniTask<bool> RunAsync(int index, Action<bool> onComplete = default)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                IsRunning = true;
                CurrentIndex = Mathf.Clamp(index, 0, steps.Count);

                for (; CurrentIndex < steps.Count; CurrentIndex = NextIndex)
                {
                    StepBase step = steps[CurrentIndex];
                    Debug.Log($"[TaskSequencer] <color=green>Run</color> {step.Name}");

                    NextIndex = CurrentIndex + 1;
                    bool result = await step.Run(cancellationTokenSource.Token);
                    if (result == false)
                    {
                        Debug.LogError($"[TaskSequencer] Fail {step.Name}");
                        onComplete?.Invoke(false);
                        return false;
                    }
                }

                await UniTask.NextFrame();
                onComplete?.Invoke(true);
                Debug.Log("[TaskSequencer] Complete");
                return true;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("[TaskSequencer] Sequence Canceled");
                onComplete?.Invoke(true);
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[TaskSequencer] Unexpected Error: {e}");
                onComplete?.Invoke(false);
                return false;
            }
            finally
            {
                IsRunning = false;
            }
        }

        public void Run(string stepName)
        {
            int index = steps.FindIndex(step => step.Name == stepName);
            if (index > -1)
            {
                if (IsRunning)
                {
                    NextIndex = index;
                }
                else
                {
                    RunAsync(index).Forget();
                }
            }
        }

        public void Start(Action<bool> onComplete = default)
        {
            if (IsRunning == false)
            {
                RunAsync(0, onComplete).Forget();
            }
            else
            {
                onComplete?.Invoke(false);
            }
        }

        public async UniTask<bool> StartAsync()
        {
            if (IsRunning == false)
                return await RunAsync(0);

            return false;
        }

        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;

            IsRunning = false;
            steps.Clear();
            steps = null;
        }
    }
}