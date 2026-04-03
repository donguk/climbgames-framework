using UnityEngine;

namespace ClimbGames.UI
{
    public abstract class UIBase : MonoBehaviour
    {
        private UILayer _layer;

        public UILayer Layer => _layer;

        internal void Initialize(UILayer layer, IUIData data = default)
        {
            _layer = layer;

            OnInitialize(data);
        }

        protected abstract void OnInitialize(IUIData data);

        public void Hide()
        {
            UIManager.Instance.Hide(this);
        }
    }
}