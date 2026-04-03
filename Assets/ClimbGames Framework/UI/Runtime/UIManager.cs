using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

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

    [SingletonConfig("Resources/UIManager")]
    public class UIManager : MonoSingleton<UIManager>
    {
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private Camera uiCamera;
        [SerializeField] private SerializableDictionary<UILayer, RectTransform> layers = new SerializableDictionary<UILayer, RectTransform>();

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
    }
}