using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;

namespace ClimbGames.Editor.Table
{
    public partial class TableWindow : EditorWindow
    {
        private EditorResizer settingsResizer = new EditorResizer(EditorResizer.Direction.Vertical, 0.5f, 24f, 24f);
        private EditorResizer viewResizer = new EditorResizer(EditorResizer.Direction.Horizontal, 0.3f, 100f, 100f);
        private Rect settingsLastRect;

        private FileTreeView excelViewer;
        private FileTreeView tableViewer;
        private SearchableTextArea jsonViewer;

        private ClimbGames.Table selectedTable;
        private string rawJson;
        private int jsonHashCode;

        public bool IsJsonChanged
        {
            get
            {
                var json = jsonViewer.Text;
                return string.IsNullOrEmpty(json) == false && string.IsNullOrEmpty(rawJson) == false &&
                        (json.Length != rawJson.Length || json.GetHashCode() != jsonHashCode);
            }
        }

        [MenuItem("Tools/ClimbGames/Table Convert")]
        public static void ShowWindow()
        {
            var window = GetWindow<TableWindow>("Table Convert");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        void OnEnable()
        {
            if (excelViewer == null)
                excelViewer = new FileTreeView();

            excelViewer.Title = "Excel Files";
            excelViewer.MultiSelect = true;
            excelViewer.SetPath(TableEditorSettings.ExcelPath, "*.xlsx", "*.xls");
            excelViewer.SetCustomMenuItem(new FileTreeMenuItem[]
            {
                new FileTreeMenuItem("Convert", FileTreeMenuFlag.File, "convert"),
            });
            excelViewer.onContextClicked += OnExcelContextClicked;

            if (tableViewer == null)
                tableViewer = new FileTreeView();

            tableViewer.Title = "Asset Files";
            tableViewer.SetPath(TableEditorSettings.DataPath, "*.asset");

            tableViewer.onSelected -= OnTableSelected;
            tableViewer.onSelected += OnTableSelected;

            if (jsonViewer == null)
                jsonViewer = new SearchableTextArea(this);
        }

        void OnGUI()
        {
            Rect rect = position;
            rect.x = rect.y = 0f;

            settingsResizer.Resize(rect, out var settingsRect, out var viewRect);
            DrawSettings(settingsRect);

            float settingsHeight = settingsRect.y + settingsLastRect.height;
            var excelViewRect = new Rect(settingsRect.x, settingsHeight, settingsRect.width, settingsRect.height - settingsHeight);
            excelViewer.OnGUI(excelViewRect);

            viewResizer.Resize(viewRect, out var tableViewRect, out var jsonViewRect);
            tableViewer.OnGUI(tableViewRect);

            jsonViewer.Draw(jsonViewRect);
        }

        void DrawSettings(Rect rect)
        {
            GUILayout.BeginArea(rect);
            EditorGUILayout.BeginVertical();
            {
                GUILayout.Space(10);

                EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
                EditorGUILayout.Space();

                if (EditorGUIs.SelectPathField("Excel Path", TableEditorSettings.ExcelPath, out var selectedPath)) //
                    TableEditorSettings.ExcelPath = selectedPath;

                if (EditorGUIs.SelectPathField("Data Path", TableEditorSettings.DataPath, out selectedPath))
                {
                    TableEditorSettings.DataPath = selectedPath.ToUnityRelativePath();
                    tableViewer.SetPath(selectedPath, "*.asset");
                }

                if (EditorGUIs.SelectPathField("CodeGen Path", TableEditorSettings.CodeGenPath, out selectedPath))
                    TableEditorSettings.CodeGenPath = selectedPath.ToUnityRelativePath();

                TableEditorSettings.SaveToBytes = EditorGUILayout.Toggle("Save To Bytes", TableEditorSettings.SaveToBytes);

                EditorGUILayout.Space(5);
                if (GUILayout.Button($"Clear And Convert All", GUILayout.Height(35)))
                {
                    string[] filePath = Paths.GetFiles(TableEditorSettings.ExcelPath, "*.xlsx", "*.xls");
                    ConvertFiles(filePath, true);
                }

                EditorGUILayout.Space(20);
            }
            EditorGUILayout.EndVertical();

            // Repaint 시점에 정확한 lastRect 저장
            if (Event.current.type == EventType.Repaint)
                settingsLastRect = GUILayoutUtility.GetLastRect();

            GUILayout.EndArea();
        }

        void OnExcelContextClicked(FileTreeMenuItem selectedMenuItem, string[] selections)
        {
            switch (selectedMenuItem.action)
            {
                case "convert":
                    {
                        var filePath = selections.Where(x => Path.HasExtension(x)).ToArray();
                        ConvertFiles(filePath);
                        break;
                    }
            }
        }

        void HandleSaveTable(Rect rect)
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
                    SaveChangedTable();
                    e.Use();
                }
            }
        }

        [Serializable]
        class ArrayWrapper
        {
            public List<TableRecord> datas;
        }

        void OnTableSelected(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            ClimbGames.Table table = AssetDatabase.LoadAssetAtPath<ClimbGames.Table>(filePath.ToUnityRelativePath());
            if (table != null && selectedTable != table)
            {
                selectedTable = table;

                int maxCount = 40;
                var datas = table.GetDatas();
                var list = datas.Take(maxCount).ToList();

                rawJson = JsonConvert.SerializeObject(new ArrayWrapper() { datas = list }, Formatting.Indented);
                if (datas.Count > maxCount)
                {
                    int lastIndex = rawJson.LastIndexOf(']');
                    if (lastIndex != -1)
                    {
                        string warningText = $"\n\n    ... \n    <...etc...> \n\n  ";
                        rawJson = rawJson.Insert(lastIndex, warningText);
                    }
                }

                jsonViewer.Title = $"{selectedTable.name} Table Preview";
                jsonViewer.SetText(rawJson);

                ProjectWindowUtil.ShowCreatedAsset(table);
            }
        }

        void SaveChangedTable()
        {
            JsonUtility.FromJsonOverwrite(jsonViewer.Text, selectedTable);
            EditorUtility.SetDirty(selectedTable);
            AssetDatabase.SaveAssetIfDirty(selectedTable);
            AssetDatabase.Refresh();

            rawJson = JsonUtility.ToJson(selectedTable, true);
            jsonHashCode = rawJson.GetHashCode();

            jsonViewer.Title = selectedTable.name;
            jsonViewer.SetText(rawJson);
        }

        void ConvertFiles(string[] files, bool clearUnused = false)
        {
            try
            {
                if (clearUnused)
                    TableConverter.DeleteUnusedFiles();

                TableConverter.StartProcess(files);
            }
            catch (IOException ex)
            {
                EditorUtility.DisplayDialog("Table Convert", "Please close the open Excel file before converting.", "Confirm");
                Debug.LogError(ex);
            }
        }

        public void OnConvertFinished(List<ClimbGames.Table> tables)
        {
            tableViewer.Refresh();

            if (TableEditorSettings.SaveToBytes)
                TableConverter.SaveToBytes(tables, TableEditorSettings.DataPath);
        }
    }
}