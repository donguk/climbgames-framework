using UnityEditor;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    [InitializeOnLoad]
    public static class PrefabConverter
    {
        static PrefabConverter()
        {
            if (CodeCompilation<PrefabCodeGenerator>.IsFinished())
            {
                var prefabPath = CodeCompilation<PrefabCodeGenerator>.GetData();
                EditorApplication.delayCall += () =>
                {
                    CreatePrefab(prefabPath);
                };
            }
            CodeCompilation<PrefabCodeGenerator>.Clear();
        }

        public static void StartProcess(string prefabPath)
        {
            // generate code
            if (GeneratePrefabCode(prefabPath))
            {
                CodeCompilation<PrefabCodeGenerator>.SetData(prefabPath);
                CodeCompilation<PrefabCodeGenerator>.Start();
            }
            else
            {

            }
        }

        static bool GeneratePrefabCode(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var root = prefab.transform;








            return true;
        }

        static void CreatePrefab(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);








            AssetDatabase.Refresh();
            Debug.Log($"[PrefabConvet] convert success: {prefabPath}");

            if (EditorWindow.HasOpenInstances<PrefabWindow>())
            {
                var window = EditorWindow.GetWindow<PrefabWindow>();
                //window?.OnConvertFinished(tables);
            }
        }
    }
}