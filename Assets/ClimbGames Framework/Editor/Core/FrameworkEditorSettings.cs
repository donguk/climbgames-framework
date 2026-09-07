using UnityEngine;
using UnityEditor;

namespace ClimbGames.Editor
{
    [FilePath("ProjectSettings/ClimbGamesSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class FrameworkEditorSettings : UnityEditor.ScriptableSingleton<FrameworkEditorSettings>
    {
        [SerializeField] private bool showSceneName = true;
        [SerializeField] private string projectNamesapce;

        public string ProjectNamesapce => projectNamesapce;

        public void Save()
        {
            Save(true);
        }

        public void UpdateRuntimeSettings()
        {
            FrameworkSettings.Instance.ShowSceneName = showSceneName;
            EditorUtility.SetDirty(FrameworkSettings.Instance);
        }
    }
}