using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ClimbGames.Editor
{
    public class FrameworkSettingsProvider : ClimbGamesSettingsProvider<FrameworkEditorSettings>
    {
        private UnityEditor.Editor runtimeSettings;

        public FrameworkSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
            label = "Settings";
            this.keywords = new[] { "ClimbGames", "Framework", "Settings", "Transition", "Namespace" };
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Editor Environment", EditorStyles.boldLabel);
            base.OnGUI(searchContext);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);
            ScriptableObject settings = FrameworkSettings.Instance;
            if (settings != null)
            {
                UnityEditor.Editor.CreateCachedEditor(settings, null, ref runtimeSettings);
                if (runtimeSettings is FrameworkSettingsInspector inspector)
                    inspector.DrawSettingsInspector("m_Script");
            }
        }
    }
}