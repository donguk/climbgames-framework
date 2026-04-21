namespace ClimbGames
{
    public partial class FSM<T>
    {
        public abstract class StateBase
        {
            private FSM<T> _fsm;
            internal IStateParam _param;

            protected FSM<T> fsm => _fsm;
            protected IStateParam param => _param;
            public virtual string Name => GetType().Name;

            public StateBase(FSM<T> fsm)
            {
                _fsm = fsm;
            }

            internal void Enter(IStateParam param)
            {
                _param = param;
                OnEnter();
            }
            internal void Update() => OnUpdate();
            internal void Exit() => OnExit();

            protected virtual void OnEnter() { }
            protected virtual void OnUpdate() { }
            protected virtual void OnExit() { }
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
}