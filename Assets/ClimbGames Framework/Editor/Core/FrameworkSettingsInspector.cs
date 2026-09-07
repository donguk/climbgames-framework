using UnityEditor;

namespace ClimbGames.Editor
{
    [CustomEditor(typeof(FrameworkSettings))]
    public class FrameworkSettingsInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var settings = (FrameworkSettings)target;
            bool useEmptyScene = settings.UseEmptyScene;

            EditorGUI.BeginChangeCheck();

            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                bool isChanged;

                if (isChanged = useEmptyScene != settings.UseEmptyScene)
                    FrameworkInitializer.UpdateEmptySceneBuildSettings(FrameworkSettings.Instance.UseEmptyScene);

                if (isChanged)
                {
                    EditorUtility.SetDirty(settings);
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
}