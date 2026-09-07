using UnityEditor;

namespace ClimbGames.Editor
{
    [CustomEditor(typeof(FrameworkEditorSettings))]
    public class FrameworkEditorSettingsInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                var editorSettings = (FrameworkEditorSettings)target;
                editorSettings.Save();
                editorSettings.UpdateRuntimeSettings();
            }
        }
    }
}