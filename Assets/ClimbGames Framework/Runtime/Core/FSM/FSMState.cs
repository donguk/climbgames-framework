using System;
using System.Collections.Generic;

namespace ClimbGames
{
    public abstract partial class FSM
    {
        public abstract class StateBase
        {
            protected FSM _fsm;
            internal IStateParam _param;
            protected List<IDisposable> disposables;

            public virtual string Name => GetType().Name;
            protected IStateParam param => _param;

            public StateBase(FSM fsm)
            {
                _fsm = fsm;
            }

            internal void Enter(IStateParam param)
            {
                _param = param;

                Dispose();
                OnEnter();
            }

            internal void Exit()
            {
                OnExit();
                Dispose();
            }

            protected virtual void OnEnter() { }
            protected virtual void OnExit() { }

            internal void Update() => OnUpdate();
            internal void FixedUpdate() => OnFixedUpdate();
            protected virtual void OnUpdate() { }
            protected virtual void OnFixedUpdate() { }

            internal void RegisterTo(IDisposable disposable)
            {
                if (disposables == null)
                    disposables = new List<IDisposable>();

                disposables.Add(disposable);
            }

            void Dispose()
            {
                if (disposables == null)
                    return;

                for (int i = disposables.Count - 1; i >= 0; i--)
                    disposables[i]?.Dispose();

                disposables.Clear();
            }
        }
    }

    public partial class FSM<T>
    {
        public abstract new class StateBase : FSM.StateBase
        {
            protected FSM<T> fsm => _fsm as FSM<T>;

            public StateBase(FSM<T> fsm) : base(fsm)
            {
                _fsm = fsm;
            }
        }

        public abstract class StateBase<TParam> : StateBase where TParam : IStateParam
        {
            protected StateBase(FSM<T> fsm) : base(fsm)
            {
            }

            protected sealed override void OnEnter()
            {
                if (_param is TParam param)
                {
                    OnEnter(param);
                    return;
                }

                Debug.LogError($"[{Name}] Type Dismatched: Expected {typeof(TParam).Name}, but param is {_param?.GetType().Name ?? "null"}");
                OnEnter(default);
            }

            protected virtual void OnEnter(TParam param = default)
            {

            }
        }
    }

    public static class FSMExtensions
    {
        public static IDisposable AddTo(this IDisposable disposable, FSM.StateBase stateBase)
        {
            if (stateBase == null)
            {
                disposable.Dispose();
                return disposable;
            }

            stateBase.RegisterTo(disposable);
            return disposable;
        }
    }
}