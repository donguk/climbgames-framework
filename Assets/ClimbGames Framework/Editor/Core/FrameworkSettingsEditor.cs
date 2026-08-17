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
                bool isChanged;

                if (isChanged = useEmptyScene != settings.UseEmptyScene)
                    FrameworkEditor.UpdateEmptySceneBuildSettings(FrameworkSettings.Instance.UseEmptyScene);

                if (isChanged)
                {
                    EditorUtility.SetDirty(settings);
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
}