using UnityEngine;
using UnityEditor;
using System;

namespace ClimbGames.Editor
{
    [Serializable]
    public class FrameworkEditorSettings : ClimbGamesSettings<FrameworkEditorSettings>
    {
        public static string EditorKey => $"{Application.dataPath.GetHashCode()}";

        [SerializeField] private bool showSceneName = true;
        [SerializeField] private string projectNamesapce = "ClimbGames";

        public string ProjectNamesapce => projectNamesapce;

        public void UpdateRuntimeSettings()
        {
            FrameworkSettings.Instance.ShowSceneName = showSceneName;
        }

        public override SettingsProvider CreateProvider()
        {
            return new FrameworkSettingsProvider("Project/ClimbGames/Framework Settings", SettingsScope.Project);
        }
    }
}