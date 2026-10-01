using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine.UIElements;

namespace ClimbGames.Editor.Table
{
    public partial class TableWindow
    {
        private TreeModel<TableAssetTreeElement> fileTreeModel;
        private TableAssetTreeView fileTreeView;
        private List<TableAssetTreeElement> treeDataList;
        private TreeViewState<int> treeViewState = new TreeViewState<int>();

        void InitTableFileTree(string path)
        {
            treeDataList = new List<TableAssetTreeElement>
            {
                new TableAssetTreeElement("root", -1, -1)
            };

            if (fileTreeModel == null)
                fileTreeModel = new TreeModel<TableAssetTreeElement>(treeDataList);

            if (fileTreeView == null)
            {
                fileTreeView = new TableAssetTreeView(treeViewState, fileTreeModel);
                fileTreeView.onSelected += OnTableSelected;
            }

            RefreshFileView(path);
        }

        void DrawFileView(Rect rect)
        {
            GUILayout.BeginArea(rect);

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Asset Files", EditorStyles.boldLabel);

            if (GUILayout.Button(GUIContentUtility.Refresh, GUILayout.Width(26f)))
                RefreshFileView(TableEditorSettings.DataPath);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();

            var lastRect = GUILayoutUtility.GetLastRect();
            var treeViewRect = new Rect(0f, lastRect.height, rect.width, rect.height - lastRect.height);

            fileTreeView.OnGUI(treeViewRect);
            GUILayout.EndArea();
        }

        void RefreshFileView(string path)
        {
            if (Directory.Exists(path) == false)
                return;

            string[] filePath = Directory.GetFiles(path, "*.asset")
                                        .Select(x => x.Replace("\\", "/"))
                                        .ToArray();

            BuildTreeData(filePath);
            fileTreeModel.SetData(treeDataList);
            fileTreeView.Reload();
            fileTreeView.ExpandAll();
        }

        void BuildTreeData(string[] filePath)
        {
            int id = 0;

            treeDataList.RemoveRange(1, treeDataList.Count - 1);
            using (new ListPoolScope<HashSet<string>>(out var folders))
            {
                for (int i = 0; i < filePath.Length; ++i)
                {
                    if (filePath[i].StartsWith("Assets/"))
                    {
                        string[] values = filePath[i].Split('/');
                        for (int depth = 0; depth < values.Length; ++depth)
                        {
                            if (depth < values.Length - 1) // folder
                            {
                                if (depth >= folders.Count)
                                    folders.Add(new HashSet<string>());

                                string directoryName = values[depth];
                                if (folders[depth].Add(directoryName))
                                    treeDataList.Add(new TableAssetTreeElement(directoryName, depth, id++));
                            }
                            else // file
                            {
                                treeDataList.Add(new TableAssetTreeElement(Path.GetFileNameWithoutExtension(filePath[i]), depth, id++) { assetPath = filePath[i] });
                            }
                        }
                    }
                }
            }
        }
    }
}