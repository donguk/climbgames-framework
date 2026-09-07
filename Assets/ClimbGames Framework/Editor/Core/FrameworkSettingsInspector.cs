using UnityEditor;

namespace ClimbGames.Editor
{
    [CustomEditor(typeof(FrameworkSettings))]
    public class FrameworkSettingsInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawSettingsInspector();
        }

        public void DrawSettingsInspector(params string[] propertyToExclude)
        {
            var settings = (FrameworkSettings)target;
            bool useEmptyScene = settings.UseEmptyScene;

            EditorGUI.BeginChangeCheck();

            serializedObject.Update();
            if (propertyToExclude != null && propertyToExclude.Length > 0)
                DrawPropertiesExcluding(serializedObject, propertyToExclude);
            else
                DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(settings);

                bool isUseEmptySceneChanged = useEmptyScene != settings.UseEmptyScene;
                if (isUseEmptySceneChanged)
                {
                    FrameworkInitializer.UpdateEmptySceneBuildSettings(FrameworkSettings.Instance.UseEmptyScene);
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
}