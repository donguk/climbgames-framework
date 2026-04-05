using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

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

            SetupUICamera();
            CreateEventSystemIfNotExist();

            SceneTransition.sceneLoaded += OnSceneLoaded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SceneTransition.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(MonoScene monoScene)
        {
            SetupUICamera();
            CreateEventSystemIfNotExist();
        }

        void SetupUICamera()
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
                {
                    data.renderType = CameraRenderType.Base;
                    uiCamera.clearFlags = CameraClearFlags.SolidColor;
                    uiCamera.backgroundColor = Color.black;
                }
            }
        }

        void CreateEventSystemIfNotExist()
        {
            EventSystem eventSystem = GameObject.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject go = new GameObject("EventSystem");
                go.AddComponent<EventSystem>();
                go.AddComponent<InputSystemUIInputModule>();

                Debug.Log("[ClimbGames] AutoCreate EventSytem");
            }
        }

        async UniTask<T> Get<T>(string key, UILayer layer) where T : UIBase
        {
            if (layers.TryGetValue(layer, out var parent) == false)
                parent = transform as RectTransform;

            T ui = await AssetManager.InstantiateAsync<T>(key, parent);
            uiList.Add(ui);

            return ui;
        }

        public async UniTask<T> Show<T>(string key, IUIData data, UILayer layer) where T : UIBase
        {
            T ui = await Get<T>(key, layer);
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

        public void Hide<T>() where T : UIBase
        {
            for (int i = 0; i < uiList.Count;)
            {
                UIBase ui = uiList[i];
                if (ui.GetType() == typeof(T))
                {
                    Hide(ui);
                }
                else
                {
                    ++i;
                }
            }
        }
    }
}