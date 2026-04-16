using UnityEngine;

namespace ClimbGames
{
    public enum PlayerState
    {
        Idle,
        Move,
        Jump,
    }

    public class IdleState : FSM<PlayerState>.StateBase
    {
        public IdleState(FSM<PlayerState> fsm) : base(fsm)
        {
        }
    }

    public class MoveState : FSM<PlayerState>.StateBase
    {
        public MoveState(FSM<PlayerState> fsm) : base(fsm)
        {
        }
    }

    public class PlayerFSM : MonoBehaviour
    {
        private FSM<PlayerState> playerFSM = new FSM<PlayerState>("Player", true);

        void Start()
        {
            playerFSM.AddState(PlayerState.Idle, new IdleState(playerFSM))
                    .AddState(PlayerState.Move, new MoveState(playerFSM));

            playerFSM.Start(PlayerState.Idle);
        }
    }
}