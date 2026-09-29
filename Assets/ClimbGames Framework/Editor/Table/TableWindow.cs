using UnityEngine;
using UnityEditor;

namespace ClimbGames.Editor.Table
{
    public class TableWindow : EditorWindow
    {
        private EditorResizer contentResizer;

        [MenuItem("Tools/ClimbGames/Table Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<TableWindow>("Tables Converter");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        void OnGUI()
        {
            //if (contentResizer == null)
            //    contentResizer = new EditorResizer(EditorResizer.Direction.Horizontal, position.width * 0.5f);
            //
            //DrawConvert();
            //EditorGUILayout.Space(10);
            //
            ////Debug.Log($"OnGUI({position})");
            //
            //
            //EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            //var contentY = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(0)).y;
            //Rect rect = new Rect(0, contentY, position.width, position.height - contentY);
            //
            //
            //DrawContent(rect);
            //EditorGUILayout.EndVertical();
        }

        void DrawConvert()
        {
            GUILayout.Label("Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Excel Path", TableEditorSettings.ExcelPath);
                if (GUILayout.Button("Browse", GUILayout.Width(70)))
                {
                    // 폴더 선택 창 오픈
                    string selectedPath = EditorUtility.OpenFolderPanel("Select Directory", TableEditorSettings.ExcelPath, "");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        BuildSettings.RootPath = selectedPath;
                        GUI.FocusControl(null); // 입력 포커스 해제
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            if (GUILayout.Button($"Convert", GUILayout.Height(35)))
            {

            }
        }

        void DrawContent(Rect rect)
        {
            contentResizer.Resize(rect, out var firstRect, out var secondRect);

            //Debug.Log($"rect({rect})");

            GUI.Box(firstRect, "First");
            GUI.Box(secondRect, "Second");
        }
    }
}