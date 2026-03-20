using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ClimbGames.Core.Tset
{
    public class BootScene : MonoScene
    {



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