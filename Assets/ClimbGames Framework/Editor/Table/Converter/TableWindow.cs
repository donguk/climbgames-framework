using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace ClimbGames.Editor.Table
{
    public partial class TableWindow : EditorWindow
    {
        private EditorResizer settingsResizer = new EditorResizer(EditorResizer.Direction.Vertical, 0.3f, 100f, 20f);
        private EditorResizer viewResizer = new EditorResizer(EditorResizer.Direction.Horizontal);
        private SearchableTextArea jsonViewer;
        private Vector2 _settingsPosition;

        private ClimbGames.Table selectedTable;
        private string tableJsonText;
        private int tableHashCode;

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

            if (jsonViewer == null)
                jsonViewer = new SearchableTextArea(this);
        }

        void OnGUI()
        {
            Rect rect = position;
            rect.x = rect.y = 0f;

            settingsResizer.Resize(rect, out var settingsRect, out var viewRect);
            DrawSettings(settingsRect);

            viewResizer.Resize(viewRect, out var fileViewRect, out var jsonViewRect);
            DrawFileView(fileViewRect);

            // save
            var jsonText = jsonViewer.Text;
            bool isTableChanged = string.IsNullOrEmpty(jsonText) == false && string.IsNullOrEmpty(tableJsonText) == false &&
                (jsonText.Length != tableJsonText.Length || jsonText.GetHashCode() != tableHashCode);

            Rect saveRect = new Rect(rect.width - 60f, rect.height - 50f, 32f, 32f);
            if (isTableChanged)
                HandleSave(saveRect);

            // 
            jsonViewer.Draw(jsonViewRect);

            if (isTableChanged)
                GUI.Button(saveRect, GUIContentUtility.SaveAs_2x);
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

        void HandleSave(Rect rect)
        {
            Event e = Event.current;
            if (e != null && e.button == 0 && rect.Contains(e.mousePosition))
            {
                if (e.type == EventType.MouseDown)
                {
                    e.Use();
                }
                else if (e.type == EventType.MouseUp)
                {
                    SaveTable();
                    e.Use();
                }
            }
        }

        void OnTableSelected(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            ClimbGames.Table asset = AssetDatabase.LoadAssetAtPath<ClimbGames.Table>(filePath);
            if (asset != null && selectedTable != asset)
            {
                selectedTable = asset;
                tableJsonText = JsonUtility.ToJson(asset, true);
                tableHashCode = tableJsonText.GetHashCode();

                jsonViewer.Title = selectedTable.name;
                jsonViewer.SetText(tableJsonText);
            }
        }

        void SaveTable()
        {
            try
            {
                JsonUtility.FromJsonOverwrite(jsonViewer.Text, selectedTable);
                EditorUtility.SetDirty(selectedTable);
                AssetDatabase.SaveAssetIfDirty(selectedTable);

                tableJsonText = JsonUtility.ToJson(selectedTable, true);
                tableHashCode = tableJsonText.GetHashCode();

                jsonViewer.Title = selectedTable.name;
                jsonViewer.SetText(tableJsonText);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Tables] {ex.Message}");
            }
        }

        public void OnConvertFinished()
        {
            Debug.Log("OnConvertFinished");
        }
    }
}