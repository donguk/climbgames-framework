using System.Collections.Generic;

namespace ClimbGames
{
    [SingletonConfig(true)]
    public class FSMUpdater : MonoSingleton<FSMUpdater>
    {
        private List<FSM> machines = new List<FSM>();
        private HashSet<FSM> lookup = new HashSet<FSM>();

        void Update()
        {
            for (int i = machines.Count - 1; i >= 0; --i)
                machines[i]?.CurrentState?.Update();
        }

        void FixedUpdate()
        {
            for (int i = machines.Count - 1; i >= 0; --i)
                machines[i]?.CurrentState?.FixedUpdate();
        }

        public void Add(FSM fsm)
        {
            if (lookup.Add(fsm))
                machines.Add(fsm);
        }

        public void Remove(FSM fsm)
        {
            if (lookup.Remove(fsm))
                machines.Remove(fsm);
        }
    }
}