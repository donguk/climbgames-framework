using UnityEngine;
using Cysharp.Threading.Tasks;

namespace ClimbGames.Core
{
    public class MonoScene : MonoBehaviour
    {
        public virtual UniTask InitializeAsync()
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask ActivateAsync()
        {
            return UniTask.CompletedTask;
        }

        public virtual void Deactivate()
        {

        }
    }
}