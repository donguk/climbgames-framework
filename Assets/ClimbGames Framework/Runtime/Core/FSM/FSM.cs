using System;
using System.Collections.Generic;
using System.Threading;
using R3;

namespace ClimbGames
{
    public partial class FSM<T> : IDisposable
    {
        public interface IStateParam
        {
        }

        private Dictionary<T, StateBase> states = new Dictionary<T, StateBase>(EqualityComparer<T>.Default);
        private StateBase currentState = null, beforeState = null;
        private T currentType, beforeType;
        private CompositeDisposable disposables;
        private bool isDisposed = false;
        private bool isChanging = false;
        private bool showDebug = false;

        public string Name { get; private set; }
        public StateBase CurrentState => currentState;
        public StateBase BeforeState => beforeState;
        public T CurrentType => currentType;
        public T BeforeType => beforeType;

        public FSM(string name = default, bool showDebug = false)
        {
            Name = string.IsNullOrEmpty(name) ? $"FSM<{typeof(T).Name}>" : name;
            this.showDebug = showDebug;
        }

        public void Initialize(params (T, StateBase)[] states)
        {
            Clear();

            foreach (var (type, state) in states)
                AddState(type, state);
        }

        public FSM<T> AddState(T type, StateBase state)
        {
            if (isDisposed)
                return this;

            if (states.ContainsKey(type) == false)
                states[type] = state;

            return this;
        }

        public void Start(T type, CancellationToken cancellationToken = default)
        {
            ChangeState(type);
            Resume(cancellationToken);
        }

        public void Resume(CancellationToken cancellationToken = default)
        {
            if (disposables == null || disposables.IsDisposed)
                disposables = new CompositeDisposable();

            Observable.EveryUpdate(UnityFrameProvider.Update, cancellationToken).Subscribe(_ => currentState?.Update()).AddTo(disposables);
            Observable.EveryUpdate(UnityFrameProvider.FixedUpdate, cancellationToken).Subscribe(_ => currentState?.FixedUpdate()).AddTo(disposables);
        }

        public void Pause()
        {
            disposables?.Clear();
            disposables?.Dispose();
            disposables = null;
        }

        public void ChangeState(T type, IStateParam param = default)
        {
            if (isDisposed)
                return;

            if (isChanging)
            {
                Debug.LogWarning($"[{Name}] State{type.ToString()} change ignored: Currently transitioning to {currentState.Name}");
                return;
            }

            if (states.TryGetValue(type, out var state))
            {
                if (currentState == state)
                    return;

                isChanging = true;
                try
                {
                    StateBase stateBase = currentState;
                    T stateType = currentType;

                    currentState = state;
                    currentType = type;

                    if (showDebug)
                        Debug.Log($"[{Name}] ChangeState: {stateBase?.Name ?? "None"} > {currentState.Name}");

                    stateBase?.Exit();
                    beforeState = stateBase;
                    beforeType = stateType;
                    currentState.Enter(param);
                }
                finally
                {
                    isChanging = false;
                }
            }
        }

        void Clear()
        {
            Pause();

            currentState?.Exit();
            currentState = null;
            beforeState = null;
            states.Clear();
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            Clear();
            states = null;
            isDisposed = true;
        }
    }
}