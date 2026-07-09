using System;
using System.Collections.Generic;

namespace ClimbGames
{
    public abstract partial class FSM : IDisposable
    {
        public interface IStateParam
        {
        }

        protected bool isDisposed = false;

        public string Name { get; protected set; }
        public StateBase CurrentState { get; protected set; }
        public StateBase BeforeState { get; protected set; }

        public virtual void Resume()
        {
            if (FSMUpdater.IsValid)
                FSMUpdater.Instance.Add(this);
        }

        public virtual void Pause()
        {
            if (FSMUpdater.IsValid)
                FSMUpdater.Instance.Remove(this);
        }

        protected virtual void Clear()
        {
            Pause();

            CurrentState?.Exit();
            CurrentState = null;
            BeforeState = null;
        }

        public virtual void Dispose()
        {
            if (isDisposed)
                return;

            Clear();
            isDisposed = true;
        }
    }

    public partial class FSM<T> : FSM
    {
        private Dictionary<T, StateBase> states = new Dictionary<T, StateBase>(EqualityComparer<T>.Default);
        private bool isChanging = false;
        private bool showDebug = false;

        public event Action<T> onChange;

        public new StateBase CurrentState => base.CurrentState as StateBase;
        public new StateBase BeforeState => base.BeforeState as StateBase;
        public T CurrentType { get; private set; }
        public T BeforeType { get; private set; }

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

        public void Start(T type)
        {
            ChangeState(type);
            Resume();
        }

        public void ChangeState(T type, IStateParam param = default)
        {
            if (isDisposed)
                return;

            if (isChanging)
            {
                Debug.LogWarning($"[{Name}] State{type.ToString()} change ignored: Currently transitioning to {CurrentState.Name}");
                return;
            }

            if (states.TryGetValue(type, out var state))
            {
                if (base.CurrentState == state)
                    return;

                isChanging = true;
                try
                {
                    StateBase stateBase = CurrentState;
                    T stateType = CurrentType;

                    base.CurrentState = state;
                    CurrentType = type;

                    if (showDebug)
                        Debug.Log($"[{Name}] ChangeState: {stateBase?.Name ?? "None"} > {CurrentState.Name}");

                    stateBase?.Exit();
                    base.BeforeState = stateBase;
                    BeforeType = stateType;
                    CurrentState.Enter(param);

                    onChange?.Invoke(CurrentType);
                }
                finally
                {
                    isChanging = false;
                }
            }
            else
            {
                Debug.LogWarning($"[{Name}] State{type.ToString()} is not exist.");
            }
        }

        protected override void Clear()
        {
            base.Clear();
            states.Clear();
        }

        public override void Dispose()
        {
            base.Dispose();
            states = null;
        }
    }
}