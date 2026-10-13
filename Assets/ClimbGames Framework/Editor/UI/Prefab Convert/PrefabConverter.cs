using System.IO;
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
                CreatePrefab(prefabPath);
            }
        }

        static bool GeneratePrefabCode(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var schema = new PrefabSchema(prefab.name);
            schema.Read(prefab.transform);

            var generator = new PrefabCodeGenerator(schema);
            if (generator.Write(PrefabConvertSettings.CodeGenPath))
            {
                AssetDatabase.Refresh();
                return true;
            }

            return false;
        }

        static void CreatePrefab(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

            var schema = new PrefabSchema(instance.name);
            schema.Read(instance.transform);
            schema.AssignElements();

            Directory.CreateDirectory(PrefabConvertSettings.OutputPath);
            string newPath = Path.Combine(PrefabConvertSettings.OutputPath, Path.GetFileName(prefabPath));

            PrefabUtility.SaveAsPrefabAsset(instance, newPath, out bool success);
            GameObject.DestroyImmediate(instance);

            AssetDatabase.Refresh();
            Debug.Log($"[PrefabConvet] convert success({success}): {newPath}");
            if (EditorWindow.HasOpenInstances<PrefabWindow>())
            {
                var window = EditorWindow.GetWindow<PrefabWindow>();
                window?.OnConvertFinished(prefabPath, success);
            }
        }
    }
}