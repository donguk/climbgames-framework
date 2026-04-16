namespace ClimbGames
{
    public partial class FSM<T>
    {
        public abstract class StateBase
        {
            protected FSM<T> fsm;
            public virtual string Name => GetType().Name;

            public StateBase(FSM<T> fsm)
            {
                this.fsm = fsm;
            }

            internal void Enter() => OnEnter();
            internal void Update() => OnUpdate();
            internal void Exit() => OnExit();

            protected virtual void OnEnter() { }
            protected virtual void OnUpdate() { }
            protected virtual void OnExit() { }
        }
    }
}