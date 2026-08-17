using UnityEngine;
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

    [DefaultExecutionOrder(9999)]
    [SingletonConfig("Resources/UIManager")]
    public partial class UIManager : MonoSingleton<UIManager>
    {
        public const string UI_LAYER = "UI";
        public const string WORLD_UI_LAYER = "World UI";

        [SerializeField] private Camera uiCamera;
        [SerializeField] private Camera worldUICamera;
        [SerializeField] private SerializableDictionary<UILayer, RectTransform> layers = new SerializableDictionary<UILayer, RectTransform>();

        private List<UIBase> uiList = new List<UIBase>();

        public Camera UICamera => uiCamera;
        public Camera WorldUICamera => worldUICamera;

        protected override void Awake()
        {
            base.Awake();

            SetupUICamera(Framework.MainCamera);
            CreateEventSystemIfNotExist();

            SceneTransition.transitionStarted += OnTransitionStarted;
            SceneTransition.sceneLoaded += OnSceneLoaded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SceneTransition.transitionStarted -= OnTransitionStarted;
            SceneTransition.sceneLoaded -= OnSceneLoaded;
        }

        void LateUpdate()
        {
            var mainCamra = Framework.MainCamera;
            if (mainCamra == null)
                return;

            // sync worldCamera;
            worldUICamera.transform.SetPositionAndRotation(mainCamra.transform.position, mainCamra.transform.rotation);
            worldUICamera.fieldOfView = mainCamra.fieldOfView;
        }

        void OnTransitionStarted()
        {
            Camera mainCamera = Framework.MainCamera;
            if (mainCamera != null)
            {
                if (mainCamera.gameObject.IsInDontDestroyOnLoad())
                    return;

                // remove statcking cameras
                if (mainCamera.TryGetComponent<UniversalAdditionalCameraData>(out var data))
                {
                    var cameraStack = data.cameraStack;
                    cameraStack.Clear();
                }
            }

            // for transition layer
            SetupUICamera();
        }

        void OnSceneLoaded(MonoScene monoScene)
        {
            SetupUICamera(Framework.MainCamera);
            CreateEventSystemIfNotExist();
        }

        void SetupUICamera(Camera mainCamera = default)
        {
            if (uiCamera.TryGetComponent<UniversalAdditionalCameraData>(out var data))
                data.renderType = CameraRenderType.Overlay;

            uiCamera.cullingMask = LayerMask.GetMask(UI_LAYER);
            worldUICamera.cullingMask = LayerMask.GetMask(WORLD_UI_LAYER);

            if (mainCamera != null)
            {
                if (worldUICamera.TryGetComponent(out data))
                    data.renderType = CameraRenderType.Overlay;

                if (mainCamera.TryGetComponent(out data))
                {
                    var cameraStack = data.cameraStack;
                    if (cameraStack.Contains(worldUICamera) == false)
                        cameraStack.Add(worldUICamera);

                    if (cameraStack.Contains(uiCamera) == false)
                        cameraStack.Add(uiCamera);

                }

                mainCamera.cullingMask &= ~LayerMask.GetMask(UI_LAYER, WORLD_UI_LAYER);
            }
            else
            {
                if (worldUICamera.TryGetComponent(out data))
                {
                    data.renderType = CameraRenderType.Base;
                    worldUICamera.clearFlags = CameraClearFlags.SolidColor;
                    worldUICamera.backgroundColor = Color.black;

                    var cameraStack = data.cameraStack;
                    if (cameraStack.Contains(uiCamera) == false)
                        cameraStack.Add(uiCamera);
                }
            }
        }

        void CreateEventSystemIfNotExist()
        {
            EventSystem eventSystem = GameObject.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject go = new GameObject("EventSystem (Created)");
                go.AddComponent<EventSystem>();
                go.AddComponent<InputSystemUIInputModule>();
            }
        }

        public RectTransform GetLayer(UILayer layer)
        {
            return layers.TryGetValue(layer, out var parent) ? parent : transform as RectTransform;
        }

        public T CreateUI<T>(string key, UILayer layer, IUIData data = default) where T : UIBase
        {
            T ui = AssetManager.Instantiate<T>(key, GetLayer(layer));
            ui.Initialize(layer, data);
            uiList.Add(ui);
            return ui;
        }

        public async UniTask<T> ShowUI<T>(string key, UILayer layer, IUIData data = default) where T : UIBase
        {
            T ui = await AssetManager.InstantiateAsync<T>(key, GetLayer(layer));
            ui.Initialize(layer, data);
            uiList.Add(ui);
            return ui;
        }

        public UniTask<T> OpenPopup<T>(string key) where T : UIBase
        {


            return default;
        }

        public void Hide(UIBase ui)
        {
            if (ui == null)
                return;

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
                if (ui == null)
                    continue;

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

        public static void HideSafely(UIBase ui)
        {
            if (IsValid == false)
                return;

            Instance.Hide(ui);
        }
    }
}