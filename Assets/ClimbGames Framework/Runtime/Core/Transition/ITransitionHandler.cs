using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace ClimbGames
{
    public interface ITransitionHandler : IDisposable
    {
        UniTask BeginAsync(ISceneParameter sceneParameter);
        void Transition(AsyncOperation asyncOperation);
        void Complete();
        void Finally();
    }
}