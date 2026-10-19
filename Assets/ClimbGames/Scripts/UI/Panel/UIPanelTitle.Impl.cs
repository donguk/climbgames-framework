using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using ClimbGames.UI;
using R3;

namespace ClimbGames
{
    public partial class UIPanelTitle : UIBase<UIPanelTitleData>
    {

        protected override UniTask OnInitialize(UIPanelTitleData data)
        {
            // implement ui

            return base.OnInitialize(data);
        }

        private GameObject uiVIew;

        void Start()
        {
            btn_check_catalog.OnClickAsObservable().Subscribe(_ => OnCheckCatalogUpdates()).AddTo(this);
            btn_load_view.OnClickAsObservable().Subscribe(_ => OnLoadView()).AddTo(this);

            ValidateTextShader(gameObject);
        }

        async void OnCheckCatalogUpdates()
        {
            if (int.TryParse(inf_patch.text, out var number))
            {
                string catalogPath = $"http://192.168.0.10/climbgames-root/framework/Android/{inf_version.text}/catalog_{inf_version.text}_{number}.json";

                var patchInfo = await AssetManager.CheckForCatalogUpdates(catalogPath);
                text_check_result.text = patchInfo.ToString();
            }
        }

        async void OnLoadView()
        {
            if (uiVIew != null)
                GameObject.DestroyImmediate(uiVIew);

            await UniTask.NextFrame();

            uiVIew = AssetManager.Instantiate("UIPatchView", trs_root);
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