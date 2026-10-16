using System;
using ClimbGames.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
                new Binding.Schema("list_", BindingType.List, true),
                new Binding.Schema("grp_", BindingType.Group, true),
                new Binding.Schema("ui_", typeof(UIBase), BindingType.UIBase, true),

                new Binding.Schema("txt_", typeof(TMP_Text), BindingType.Component),
                new Binding.Schema("img_", typeof(Image), BindingType.Component),
                //
                //
                //
                //
            };
        }
    }
}