using System.Linq;
using ClimbGames.Core;
using ClimbGames.Core.Editor;
using UnityEditor;

namespace ClimbGames
{
    [CustomEditor(typeof(FrameworkSettings))]
    public class FrameworkSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var settings = (FrameworkSettings)target;
            bool useEmptyScene = settings.UseEmptyScene;

            EditorGUI.BeginChangeCheck();

            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                if (useEmptyScene != settings.UseEmptyScene)
                    FrameworkInitializer.SetupEmptySceneToBuildSettings(settings.UseEmptyScene);

                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
        }
    }
}