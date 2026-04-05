using System;
using UnityEngine;

namespace ClimbGames.UI
{
    public abstract class UIBase : MonoBehaviour
    {
        private UILayer _layer;
        protected IUIData _data;

        public UILayer Layer => _layer;

        internal void Initialize(UILayer layer, IUIData data = default)
        {
            _layer = layer;
            _data = data;

            OnInitialize(data);
        }

        protected virtual void OnInitialize(IUIData data)
        {

        }

        public void Hide()
        {
            UIManager.Instance.Hide(this);
        }
    }

    public abstract class UIBase<T> : UIBase where T : IUIData
    {
        protected override void OnInitialize(IUIData data)
        {
            OnInitialize((T)data);
        }

        protected virtual void OnInitialize(T data)
        {

        }
    }
}