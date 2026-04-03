using System.Threading;
using Cysharp.Threading.Tasks;

namespace ClimbGames
{
    public abstract class TaskStep
    {
        public string Name => GetType().Name;
        public float Progress { get; protected set; }
        public virtual float Weight => 1f;

        public abstract UniTask<bool> Run(CancellationToken cancellationToken = default);
    }
}