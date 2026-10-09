using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    [InitializeOnLoad]
    public static class UIPrefabConverter
    {
        static UIPrefabConverter()
        {
            if (CodeCompilation<UIPrefabCodeGenerator>.IsFinished())
            {
                var prefabPath = CodeCompilation<UIPrefabCodeGenerator>.GetData();
                EditorApplication.delayCall += () =>
                {
                    CreatePrefab(prefabPath);
                };
            }
            CodeCompilation<UIPrefabCodeGenerator>.Clear();
        }

        public static void StartProcess(string prefabPath)
        {
            // generate code
            if (GeneratePrefabCode(prefabPath))
            {
                CodeCompilation<UIPrefabCodeGenerator>.SetData(prefabPath);
                CodeCompilation<UIPrefabCodeGenerator>.Start();
            }
            else
            {

            }
        }

        static bool GeneratePrefabCode(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            string scriptText = CodeGenerator.CreateScript("3f97c32d3d31ede44b44bb53e15dfb6d", "ClimbGames", "UIPrefab");
            scriptText = scriptText.Replace("#FIELDS#", string.Empty);

            string filePath = Path.Combine(UIPrefabConvertSettings.CodeGenPath, $"UIPrefab.cs");
            CodeGenerator.Write(scriptText, filePath);

            AssetDatabase.ImportAsset(filePath);
            AssetDatabase.Refresh();
            return true;
        }

        static void CreatePrefab(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);








            AssetDatabase.Refresh();
            Debug.Log($"[UIPrefabConvet] convert success: {prefabPath}");

            if (EditorWindow.HasOpenInstances<UIPrefabWindow>())
            {
                var window = EditorWindow.GetWindow<UIPrefabWindow>();
                //window?.OnConvertFinished(tables);
            }
        }
    }
}