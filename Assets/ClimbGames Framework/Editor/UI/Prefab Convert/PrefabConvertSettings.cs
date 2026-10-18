using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ClimbGames.UI;

namespace ClimbGames.Editor.UI
{
    [Serializable]
    public class PrefabConvertSettings
    {
        [SerializeField] private string originPath = "Assets/Prefabs/UI";
        [SerializeField] private string outputPath = "Assets/Sources/Prefabs/UI";
        [SerializeField] private string scriptPath = "Assets/Scripts/UI";

        [SerializeField] private Binding.Schema[] bindingSchemas;

        public static string OriginPath
        {
            get => UISettings.Instance.PrefabConvertSettings.originPath;
            set => UISettings.Instance.PrefabConvertSettings.originPath = value;
        }
        public static string OutputPath
        {
            get => UISettings.Instance.PrefabConvertSettings.outputPath;
            set => UISettings.Instance.PrefabConvertSettings.outputPath = value;
        }
        public static string ScriptPath
        {
            get => UISettings.Instance.PrefabConvertSettings.scriptPath;
            set => UISettings.Instance.PrefabConvertSettings.scriptPath = value;
        }
        public static Binding.Schema[] BindingSchemas => UISettings.Instance.PrefabConvertSettings.bindingSchemas;

        public void Reset()
        {
            originPath = "Assets/Prefabs/UI";
            outputPath = "Assets/Sources/Prefabs/UI";
            scriptPath = "Assets/Scripts/UI";

            ResetSchemas();
        }

        public void ResetSchemas()
        {
            bindingSchemas = new Binding.Schema[]
            {
                new Binding.Schema("list_", typeof(List<>), Binding.SchemaOption.List | Binding.SchemaOption.Readonly),
                new Binding.Schema("grp_", Binding.SchemaOption.Group | Binding.SchemaOption.Readonly),
                new Binding.Schema("ui_", typeof(UIBase), Binding.SchemaOption.UIBase | Binding.SchemaOption.Readonly),

                new Binding.Schema("trs_", typeof(RectTransform), new Type[] { typeof(Transform) }),
                new Binding.Schema("txt_;text_", typeof(UIText), new Type[] { typeof(TMP_Text) }),
                new Binding.Schema("img_;image_", typeof(UIImage), new Type[] { typeof(Image) }),
                new Binding.Schema("raw_;rawimg_", typeof(UIRawImage), new Type[] { typeof(RawImage) }),

                new Binding.Schema("tgl_", typeof(UIToggle), new Type[] { typeof(Toggle) }),
                new Binding.Schema("sld_", typeof(UISlider), new Type[] { typeof(Slider) }),
                new Binding.Schema("scb_", typeof(UIScrollbar), new Type[] { typeof(Scrollbar) }),
                new Binding.Schema("scr_", typeof(UIScrollRect), new Type[] { typeof(ScrollRect) }),
                new Binding.Schema("btn_", typeof(UIButton), new Type[] { typeof(Button) }),
                new Binding.Schema("drd_", typeof(UIDropdown), new Type[] { typeof(TMP_Dropdown) }),
                new Binding.Schema("inf_", typeof(UIInputField), new Type[] { typeof(TMP_InputField) }),
            };
        }
    }
}