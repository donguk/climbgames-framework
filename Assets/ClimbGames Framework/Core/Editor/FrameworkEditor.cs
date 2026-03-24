using System.IO;
using System.Linq;
using System.Reflection;
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
    }
}