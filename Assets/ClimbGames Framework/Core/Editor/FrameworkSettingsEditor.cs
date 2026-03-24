using UnityEditor;

namespace ClimbGames.Editor
{
    [CustomEditor(typeof(FrameworkSettings))]
    public class FrameworkSettingsEditor : UnityEditor.Editor
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
                    FrameworkEditor.UseEmptyScene(settings.UseEmptyScene);

                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
        }
    }
}