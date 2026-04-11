using System.IO;
using System.Linq;
using System.Reflection;
using ClimbGames.UI;
using UnityEditor;
using UnityEngine;

namespace ClimbGames.Editor
{
    [InitializeOnLoad]
    public static class FrameworkEditor
    {
        private const string EmptySceneGUID = "1d61250f766252d459de01ed701a82ed";

        static FrameworkEditor()
        {
            EditorApplication.delayCall += Initialize;
        }

        static void Initialize()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            CreateSettingsIfNotExist();
            UpdateEmptySceneBuildSettings(FrameworkSettings.Instance.UseEmptyScene);
            AddLayerToTagManager(UIManager.WORLD_UI_LAYER);
        }

        private static void CreateSettingsIfNotExist()
        {
            var assetPath = typeof(FrameworkSettings).GetCustomAttribute<AssetPathAttribute>();
            if (assetPath != null)
            {
                if (File.Exists(Path.Combine(Application.dataPath, "..", assetPath.Value)))
                    return;

                string folderPath = Path.GetDirectoryName(assetPath.Value);
                if (AssetDatabase.IsValidFolder(folderPath) == false)
                {
                    Directory.CreateDirectory(Path.Combine(Application.dataPath, "..", folderPath));
                    AssetDatabase.Refresh();
                }

                FrameworkSettings newSettings = ScriptableObject.CreateInstance<FrameworkSettings>();
                AssetDatabase.CreateAsset(newSettings, assetPath.Value);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        public static void UpdateEmptySceneBuildSettings(bool useEmptyScene)
        {
            var scenes = EditorBuildSettings.scenes.ToList();

            GUID guid = new GUID(EmptySceneGUID);
            var index = scenes.FindIndex(scene => scene.guid == guid);
            if (useEmptyScene && index <= -1)
            {
                scenes.Insert(0, new EditorBuildSettingsScene(guid, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
            else if (useEmptyScene == false && index > -1)
            {
                scenes.RemoveAt(index);
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }

        public static void AddLayerToTagManager(string layerName)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            SerializedObject tagManager = new SerializedObject(assets[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            // 이미 레이어가 존재하는지 확인
            for (int i = 6; i < layers.arraySize; i++)
            {
                SerializedProperty layerProp = layers.GetArrayElementAtIndex(i);

                // 동일한 이름이 있으면 중단
                if (layerProp.stringValue == layerName)
                    return;

                // 비어있는 슬롯을 찾으면 레이어 이름 할당
                if (string.IsNullOrEmpty(layerProp.stringValue))
                {
                    layerProp.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    return;
                }
            }
        }
    }
}