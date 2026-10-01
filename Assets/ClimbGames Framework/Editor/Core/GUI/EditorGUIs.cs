


using UnityEditor;
using UnityEngine;

namespace ClimbGames.Editor
{
    public static class EditorGUIs
    {
        public static bool SelectPathField(string title, string path, out string selectedPath)
        {
            selectedPath = null;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(title, path);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                selectedPath = EditorUtility.OpenFolderPanel("Select Directory", path, "");
                if (string.IsNullOrEmpty(selectedPath) == false)
                    GUI.FocusControl(null);
            }
            GUILayout.EndHorizontal();
            return string.IsNullOrEmpty(selectedPath) == false;
        }

        public static bool SelectFileField(string title, string path, string exstension, out string selectedPath)
        {
            selectedPath = null;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(title, path);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                selectedPath = EditorUtility.OpenFilePanel("Select File", path, exstension);
                if (string.IsNullOrEmpty(selectedPath) == false)
                    GUI.FocusControl(null);
            }
            GUILayout.EndHorizontal();
            return string.IsNullOrEmpty(selectedPath) == false;
        }
    }
}