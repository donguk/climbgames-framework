using ClimbGames.UI;
using UnityEngine;
using UnityEngine.UI;
using R3;
using TMPro;

namespace ClimbGames
{
    public class UIPanelTitle : UIBase
    {
        [SerializeField] private Transform root;
        [SerializeField] private Button btn_check_patch;
        [SerializeField] private Button btn_load_view;
        [SerializeField] private TMP_Text txt_check_result;

        private GameObject uiVIew;

        void Start()
        {
            btn_check_patch.OnClickAsObservable().Subscribe(_ => OnCheckPatch()).AddTo(this);
            btn_load_view.OnClickAsObservable().Subscribe(_ => OnLoadView()).AddTo(this);

            ValidateTextShader(gameObject);


        }

        async void OnCheckPatch()
        {
            var patchInfo = await AssetManager.CheckForCatalogUpdates("");
            txt_check_result.text = patchInfo.ToString();
        }

        void OnLoadView()
        {
            if (uiVIew != null)
                GameObject.DestroyImmediate(uiVIew);

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