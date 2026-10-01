using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ClimbGames.Editor
{
    public class FileTreeView
    {
        private string _title;
        private string rootPath;
        private string searchPattern;

        private int treeItemId = 0;
        private TreeModel<Element> treeModel;
        private TreeView treeView;
        private List<Element> datas, cachedBuildDatas;
        private TreeViewState<int> treeViewState = new TreeViewState<int>();

        public event Action<string> onSelected;
        public string Title { get => _title; set => _title = value; }

        public FileTreeView()
        {
            datas = new List<Element>() { new Element("root", -1, -1) };
            cachedBuildDatas = new List<Element>();

            treeModel = new TreeModel<Element>(datas);
            treeView = new TreeView(treeViewState, treeModel);

            treeView.onSelected += (element) => onSelected?.Invoke(element.path);
            treeView.onContextClicked += ContextClicked;
        }

        public void Draw(Rect rect)
        {
            GUILayout.BeginArea(rect);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(_title, EditorStyles.boldLabel);

            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();

            var lastRect = GUILayoutUtility.GetLastRect();
            float headerHeight = lastRect.y + lastRect.height;
            var treeViewRect = new Rect(0f, headerHeight, rect.width, rect.height - headerHeight);

            treeView.OnGUI(treeViewRect);
            GUILayout.EndArea();
        }

        public void SetPath(string path, string searchPattern = default)
        {
            if (Directory.Exists(path) == false)
                return;

            rootPath = path;
            this.searchPattern = searchPattern;
            treeItemId = 0;
            datas.Clear();

            using (new ListPoolScope<Element>(out var list))
            {
                BuildTreeData(path, ref list);

                datas.AddRange(list);
                treeModel.SetData(datas);
            }

            treeView.Reload();
            treeView.ExpandAll();
        }

        void BuildTreeData(string path, ref List<Element> list)
        {
            list.Add(new Element("root", -1, -1));

            if (Directory.Exists(path) == false)
                return;

            string[] files = Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories)
                                        .Select(x => x.Replace("\\", "/"))
                                        .OrderByDescending(x => x.Split('/').Length)
                                        .ThenBy(x => x)
                                        .ToArray();

            string rootPath = Path.GetDirectoryName(path).Replace("\\", "/");
            using (new ListPoolScope<HashSet<string>>(out var folders))
            {
                for (int i = 0; i < files.Length; ++i)
                {
                    string filePath = files[i];

                    string directoryPath = filePath.Substring(0, rootPath.Length);
                    string relativePath = filePath.Remove(0, rootPath.Length);

                    // 타켓 경로에서부터 파싱
                    string[] values = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    for (int depth = 0; depth < values.Length; ++depth)
                    {
                        if (depth < values.Length - 1)
                        {
                            if (depth >= folders.Count)
                                folders.Add(new HashSet<string>());

                            string directoryName = values[depth];

                            if (directoryPath.Length > 0)
                                directoryPath += "/";
                            directoryPath += directoryName;

                            if (folders[depth].Add(directoryName))
                                list.Add(new Element(directoryPath, depth, treeItemId++));
                        }
                        else
                        {
                            list.Add(new Element(files[i], depth, treeItemId++));
                        }
                    }
                }
            }
        }

        void RefreshFolder(string path)
        {
            Element element = FindItemByPath(treeModel.root, path);
            if (element == null)
                return;

            int index = element.parent.children.FindIndex(x => (x as Element).path == path);
            if (index > -1)
            {
                using (new ListPoolScope<Element>(out var list))
                {
                    BuildTreeData(path, ref list);

                    var root = TreeElementUtility.ListToTree(list);
                    treeModel.ReplaceElement(element.parent, index, root.hasChildren ? root.children[0] : null);
                }
            }
        }

        private Element FindItemByPath(Element parent, string path)
        {
            if (parent.path == path)
                return parent;

            if (parent.hasChildren)
            {
                foreach (var child in parent.children)
                {
                    var found = FindItemByPath(child as Element, path);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        void ContextClicked()
        {
            IList<int> ids = treeView.GetSelection();
            if (ids.Count <= 0)
                return;

            var element = treeModel.Find(ids[0]);
            if (element == null)
                return;

            if (element.isDirectory)
            {
                Vector2 position = Event.current.mousePosition;
                EditorUtility.DisplayCustomMenu(new Rect(position.x, position.y, 0, 0), new GUIContent[]
                {
                    new GUIContent("Refresh"),
                }, -1, OnSelectContextMenu, element);
            }
        }

        void OnSelectContextMenu(object userData, string[] options, int selected)
        {
            switch (selected)
            {
                case 0:
                    {
                        Element element = userData as Element;
                        RefreshFolder(element.path);
                        break;
                    }
            }
        }












        class Element : TreeElement
        {
            public string path;
            public bool isDirectory;

            public Element(string path, int depth, int id)
            {
                this.path = path;
                this.id = id;
                this.depth = depth;

                if (string.IsNullOrEmpty(path) == false)
                {
                    isDirectory = Path.HasExtension(path) == false;
                    if (isDirectory)
                    {
                        name = Path.GetFileName(path);
                    }
                    else
                    {
                        name = Path.GetFileNameWithoutExtension(path);
                    }
                }
            }
        }

        class TreeView : TreeView<Element>
        {
            public event Action<Element> onSelected;
            public event Action onContextClicked;

            public TreeView(TreeViewState<int> state, TreeModel<Element> model) : base(state, model)
            {
                rowHeight = 20f;
                showAlternatingRowBackgrounds = true;
            }

            protected override void OnRowGUI(Rect rect, TreeViewItem item, Element element, ref RowGUIArgs args)
            {
                if (element != null)
                {
                    Rect iconRect = rect;
                    iconRect.x += GetContentIndent(item);
                    iconRect.width = 20f;

                    if (element.isDirectory)
                    {
                        GUI.DrawTexture(iconRect, IsExpanded(element.id) ? GUIContents.FolderOpened_Icon?.image : GUIContents.Folder_Icon?.image, ScaleMode.ScaleToFit);
                    }
                    else
                    {
                        GUI.DrawTexture(iconRect, GUIContents.ScriptableObject_Icon?.image, ScaleMode.ScaleToFit);
                    }

                    rect.x += iconRect.width + 2f;
                }

                float lineHeight = EditorGUIUtility.singleLineHeight;

                Rect labelRect = rect;
                labelRect.height = lineHeight;
                labelRect.y += (rect.height - lineHeight) * 0.5f;

                args.rowRect = labelRect;
                base.OnRowGUI(rect, item, element, ref args);
            }

            protected override void SingleClickedItem(int id)
            {
                var element = treeModel.Find(id);
                if (element != null)
                    onSelected?.Invoke(element);
            }

            protected override bool CanMultiSelect(TreeViewItem<int> item)
            {
                return false;
            }

            protected override void ContextClicked()
            {
                onContextClicked?.Invoke();
            }
        }
    }
}