using UnityEngine;
using UnityEditor;

namespace ClimbGames.Editor.Table
{
    public partial class TableWindow : EditorWindow
    {
        private EditorResizer settingsResizer = new EditorResizer(EditorResizer.Direction.Vertical, 0.3f, 100f, 20f);
        private EditorResizer viewResizer = new EditorResizer(EditorResizer.Direction.Horizontal);

        private Vector2 _settingsPosition;

        [MenuItem("Tools/ClimbGames/Table Converter")]
        public static void ShowWindow()
        {
            var window = GetWindow<TableWindow>("Tables Converter");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        void OnEnable()
        {
            InitTableFileTree(TableEditorSettings.DataPath);
            InitTableContent();
        }

        void OnGUI()
        {
            Rect rect = position;
            rect.x = rect.y = 0f;

            settingsResizer.Resize(rect, out var settingsRect, out var viewRect);
            DrawSettings(settingsRect);

            viewResizer.Resize(viewRect, out var fileViewRect, out var contentViewRect);
            DrawFileView(fileViewRect);
            DrawAssetContent(contentViewRect);
        }

        bool SelectPathField(string title, string path, out string selectedPath)
        {
            selectedPath = string.Empty;
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(title, path);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                // 폴더 선택 창 오픈
                selectedPath = EditorUtility.OpenFolderPanel("Select Directory", TableEditorSettings.ExcelPath, "");
                if (string.IsNullOrEmpty(selectedPath) == false)
                {
                    GUI.FocusControl(null); // 입력 포커스 해제
                }
            }
            GUILayout.EndHorizontal();
            return string.IsNullOrEmpty(selectedPath) == false;
        }

        void DrawSettings(Rect rect)
        {
            GUILayout.BeginArea(rect);
            _settingsPosition = EditorGUILayout.BeginScrollView(_settingsPosition);
            GUILayout.Space(10);

            GUILayout.Label("Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (SelectPathField("Excel Path", TableEditorSettings.ExcelPath, out var selectedPath))
                TableEditorSettings.ExcelPath = selectedPath;

            if (SelectPathField("Data Path", TableEditorSettings.DataPath, out selectedPath))
            {
                TableEditorSettings.DataPath = selectedPath;
                RefreshFileView(selectedPath);
            }

            if (SelectPathField("CodeGen Path", TableEditorSettings.CodeGenPath, out selectedPath))
                TableEditorSettings.CodeGenPath = selectedPath;

            EditorGUILayout.Space(5);
            if (GUILayout.Button($"Convert", GUILayout.Height(35)))
            {
                TableConverter.StartConvert();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        public void OnConvertFinished()
        {
            Debug.Log("OnConvertFinished");
        }
    }
}