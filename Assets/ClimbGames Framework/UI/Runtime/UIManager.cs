using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace ClimbGames.UI
{
    public enum UILayer
    {
        World,
        HUD,
        View,
        Popup,
        Top,
        System,
        Transition
    }

    public interface IUIData
    {

    }

    [SingletonConfig("Resources/UIManager")]
    public partial class UIManager : MonoSingleton<UIManager>
    {
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private Camera uiCamera;
        [SerializeField] private SerializableDictionary<UILayer, RectTransform> layers = new SerializableDictionary<UILayer, RectTransform>();

        private List<UIBase> uiList = new List<UIBase>();

        protected override void Awake()
        {
            base.Awake();

            SceneManager.sceneLoaded += OnSceneLoaded;
            SetupCameraSettings();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            SetupCameraSettings();
        }

        void SetupCameraSettings()
        {
            var mainCamera = Framework.MainCamera;

            UniversalAdditionalCameraData data;
            if (mainCamera != null)
            {
                if (uiCamera.TryGetComponent(out data))
                    data.renderType = CameraRenderType.Overlay;

                if (mainCamera.TryGetComponent(out data))
                {
                    var cameraStack = data.cameraStack;
                    if (cameraStack.Contains(uiCamera) == false)
                        cameraStack.Add(uiCamera);
                }
            }
            else
            {
                if (uiCamera.TryGetComponent(out data))
                    data.renderType = CameraRenderType.Base;
            }
        }

        public async UniTask<T> Show<T>(string key, UILayer layer = UILayer.View, IUIData data = default) where T : UIBase
        {
            if (layers.TryGetValue(layer, out var parent) == false)
                parent = transform as RectTransform;

            T ui = await AssetManager.InstantiateAsync<T>(key, parent);
            uiList.Add(ui);

            ui.Initialize(layer, data);
            return ui;
        }

        public void Hide(UIBase ui)
        {
            if (ui.Layer == UILayer.Popup)
            {
                // dimm 처리
            }

            uiList.Remove(ui);
            GameObject.Destroy(ui.gameObject);
        }
    }
}