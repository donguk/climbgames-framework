using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ClimbGames.Editor
{
    [InitializeOnLoad]
    public static class FrameworkEditor
    {
        private const string AssetPath = "Assets/ClimbGames/Resources/FrameworkSettings.asset";
        private const string EmptySceneGUID = "1d61250f766252d459de01ed701a82ed";

        static FrameworkEditor()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode == false)
                CheckAndCreateSettings();
        }

        private static void CheckAndCreateSettings()
        {
            if (File.Exists(Path.Combine(Application.dataPath, "..", AssetPath)))
                return;

            string folderPath = Path.GetDirectoryName(AssetPath);
            if (AssetDatabase.IsValidFolder(folderPath) == false)
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "..", folderPath));
                AssetDatabase.Refresh();
            }

            FrameworkSettings newSettings = ScriptableObject.CreateInstance<FrameworkSettings>();
            AssetDatabase.CreateAsset(newSettings, AssetPath);

            UseEmptyScene(newSettings.UseEmptyScene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void UseEmptyScene(bool value)
        {
            var scenes = EditorBuildSettings.scenes.ToList();

            GUID guid = new GUID(EmptySceneGUID);
            var index = scenes.FindIndex(scene => scene.guid == guid);

            if (value && index <= -1)
            {
                scenes.Insert(0, new EditorBuildSettingsScene(guid, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
            else if (value == false && index > -1)
            {
                scenes.RemoveAt(index);
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
    }
}