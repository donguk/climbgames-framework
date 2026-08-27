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
        private const string UIEnvironmentSceneGUID = "86d684a88f3f0184da6516468018b5ff";

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
            RegisterUIEnvironmentScene();
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
            if (useEmptyScene)
            {
                if (index <= -1)
                {
                    scenes.Add(new EditorBuildSettingsScene(guid, true));
                }
                else if (index == 0)
                {
                    scenes.RemoveAt(index);
                    scenes.Add(new EditorBuildSettingsScene(guid, true));
                }
                EditorBuildSettings.scenes = scenes.ToArray();
            }
            else if (useEmptyScene == false && index > -1)
            {
                scenes.RemoveAt(index);
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }

        //

        public static void RegisterUIEnvironmentScene()
        {
            // 씬 로드
            GUID guid = new GUID(UIEnvironmentSceneGUID);
            SceneAsset sceneAsset = AssetDatabase.LoadAssetByGUID<SceneAsset>(guid);

            if (sceneAsset == null)
            {
                Debug.LogError($"can not find UIEnvironment Scene..");
                return;
            }

            // ProjectSettings/EditorSettings.asset 에셋 로드
            Object[] settingsAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/EditorSettings.asset");
            if (settingsAssets == null || settingsAssets.Length == 0)
                return;

            // SerializedObject를 통해 UIEnvironment 씬 프로퍼티 변경
            SerializedObject serializedSettings = new SerializedObject(settingsAssets[0]);
            SerializedProperty uiEnvProp = serializedSettings.FindProperty("m_PrefabUIEnvironment");

            if (uiEnvProp != null && uiEnvProp.objectReferenceValue == null)
            {
                uiEnvProp.objectReferenceValue = sceneAsset;
                serializedSettings.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }
        }

        public static void AddLayerToTagManager(string layerName)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            SerializedObject serializedSettings = new SerializedObject(assets[0]);
            SerializedProperty layerProp = serializedSettings.FindProperty("layers");

            // 이미 레이어가 존재하는지 확인
            for (int i = 6; i < layerProp.arraySize; i++)
            {
                SerializedProperty elementProp = layerProp.GetArrayElementAtIndex(i);

                // 동일한 이름이 있으면 중단
                if (elementProp.stringValue == layerName)
                    return;

                // 비어있는 슬롯을 찾으면 레이어 이름 할당
                if (string.IsNullOrEmpty(elementProp.stringValue))
                {
                    elementProp.stringValue = layerName;
                    serializedSettings.ApplyModifiedProperties();
                    return;
                }
            }
        }
    }
}