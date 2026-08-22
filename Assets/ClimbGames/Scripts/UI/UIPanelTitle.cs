using ClimbGames.UI;
using UnityEngine;
using UnityEngine.UI;
using R3;
using TMPro;
using Cysharp.Threading.Tasks;

namespace ClimbGames
{
    public class UIPanelTitle : UIBase
    {
        [SerializeField] private Transform root;
        [SerializeField] private TMP_InputField tif_version;
        [SerializeField] private TMP_InputField tif_patch;
        [SerializeField] private Button btn_check_catalog;
        [SerializeField] private Button btn_load_view;
        [SerializeField] private TMP_Text txt_catalog_result;

        private GameObject uiVIew;

        void Start()
        {
            btn_check_catalog.OnClickAsObservable().Subscribe(_ => OnCheckCatalogUpdates()).AddTo(this);
            btn_load_view.OnClickAsObservable().Subscribe(_ => OnLoadView()).AddTo(this);

            ValidateTextShader(gameObject);
        }

        async void OnCheckCatalogUpdates()
        {
            if (int.TryParse(tif_patch.text, out var number))
            {
                string catalogPath = $"http://localhost/climbgames-root/framework/Android/{tif_version.text}/catalog_{tif_version.text}_{number}.json";

                var patchInfo = await AssetManager.CheckForCatalogUpdates(catalogPath);
                txt_catalog_result.text = patchInfo.ToString();
            }
        }

        async void OnLoadView()
        {
            if (uiVIew != null)
                GameObject.DestroyImmediate(uiVIew);

            await UniTask.NextFrame();

            uiVIew = AssetManager.Instantiate("UIPatchView", root);
            ValidateTextShader(uiVIew);
        }

        void ValidateTextShader(GameObject go)
        {
            if (go == null)
                return;

            var texts = go.GetComponentsInChildren<TMP_Text>();
            foreach (var text in texts)
            {
                if (text.fontMaterial != null)
                {
                    string name = text.fontMaterial.shader.name;
                    text.fontMaterial.shader = Shader.Find(name);
                }
            }
        }
    }
}