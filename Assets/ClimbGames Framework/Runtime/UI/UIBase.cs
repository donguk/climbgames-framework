using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ClimbGames.UI
{
    public abstract class UIBase : MonoBehaviour
    {
        private UILayer _layer;
        internal IUIData _data;

        public UILayer Layer => _layer;

        internal void Initialize(UILayer layer, IUIData data = default)
        {
            _layer = layer;
            _data = data;

            OnInitialize();
        }

        protected virtual UniTask OnInitialize()
        {
            return UniTask.CompletedTask;
        }

        public void Hide()
        {
            UIManager.Instance.Hide(this);
        }
    }

    public abstract class UIBase<T> : UIBase where T : IUIData
    {
        protected sealed override UniTask OnInitialize()
        {
            if (_data is T data)
                return OnInitialize(data);

            Debug.LogError($"[{GetType().Name}] Type Dismatched: Expected {typeof(T).Name}, but data is {_data?.GetType().Name ?? "null"}");
            return OnInitialize(default);
        }

        protected virtual UniTask OnInitialize(T data)
        {
            return UniTask.CompletedTask;
        }
    }
}