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

        [SerializeReference] private Binding.Schema[] bindingSchemas;

        public static string OriginPath
        {
            get => UISettings.Instance.PrefabConvertSettings.originPath;
            set
            {
                if (UISettings.Instance.PrefabConvertSettings.originPath != value)
                {
                    UISettings.Instance.PrefabConvertSettings.originPath = value;
                    UISettings.Instance.Save();
                }
            }
        }
        public static string OutputPath
        {
            get => UISettings.Instance.PrefabConvertSettings.outputPath;
            set
            {
                if (UISettings.Instance.PrefabConvertSettings.outputPath != value)
                {
                    UISettings.Instance.PrefabConvertSettings.outputPath = value;
                    UISettings.Instance.Save();
                }

            }
        }
        public static string ScriptPath
        {
            get => UISettings.Instance.PrefabConvertSettings.scriptPath;
            set
            {
                if (UISettings.Instance.PrefabConvertSettings.scriptPath != value)
                {
                    UISettings.Instance.PrefabConvertSettings.scriptPath = value;
                    UISettings.Instance.Save();
                }
            }
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
                new Binding.ListSchema("list_", typeof(List<>), Binding.SchemaOption.IncludeChildren, true),
                new Binding.GroupSchema("grp_", Binding.SchemaOption.IncludeChildren, true),
                new Binding.ComponentSchema("ui_", typeof(UIBase),  Binding.SchemaOption.None, true),

                new Binding.ComponentSchema("trs_", typeof(RectTransform), new Type[] { typeof(Transform) }, Binding.SchemaOption.IncludeChildren),
                new Binding.ComponentSchema("txt_;text_", typeof(UIText), new Type[] { typeof(TMP_Text) }, Binding.SchemaOption.IncludeChildren),
                new Binding.ComponentSchema("img_;image_", typeof(UIImage), new Type[] { typeof(Image) }, Binding.SchemaOption.IncludeChildren),
                new Binding.ComponentSchema("raw_;rawimg_", typeof(UIRawImage), new Type[] { typeof(RawImage) }, Binding.SchemaOption.IncludeChildren),

                new Binding.ComponentSchema("tgl_", typeof(UIToggle), new Type[] { typeof(Toggle) }, Binding.SchemaOption.IncludeChildren),
                new Binding.ComponentSchema("sld_", typeof(UISlider), new Type[] { typeof(Slider) }, Binding.SchemaOption.IncludeChildren),
                new Binding.ComponentSchema("scb_", typeof(UIScrollbar), new Type[] { typeof(Scrollbar) }, Binding.SchemaOption.IncludeChildren),
                new Binding.ComponentSchema("scr_", typeof(UIScrollRect), new Type[] { typeof(ScrollRect) }, Binding.SchemaOption.IncludeChildren),
                new Binding.ComponentSchema("btn_", typeof(UIButton), new Type[] { typeof(Button) }, Binding.SchemaOption.IncludeChildren),
                new Binding.ComponentSchema("drd_", typeof(UIDropdown), new Type[] { typeof(TMP_Dropdown) }, Binding.SchemaOption.IncludeChildren),
                new Binding.ComponentSchema("inf_", typeof(UIInputField), new Type[] { typeof(TMP_InputField) }, Binding.SchemaOption.IncludeChildren),
            };
        }
    }
}