using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ClimbGames.Editor
{
    public class FileTreeElement : TreeElement
    {
        public string path, extension;
        public bool isDirectory;

        public FileTreeElement(string path, int depth, int id)
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
                    extension = Path.GetExtension(path);
                }
            }
        }
    }

    [Flags]
    public enum FileTreeMenuFlag
    {
        Folder = 1 << 0,
        File = 1 << 1,
        Common = Folder | File,
    }

    public class FileTreeMenuItem
    {
        public GUIContent content;
        public FileTreeMenuFlag flag;
        public string name, action;

        public FileTreeMenuItem(string name, FileTreeMenuFlag flag = FileTreeMenuFlag.Common, string action = default)
        {
            this.name = name;
            content = new GUIContent(name);
            this.flag = flag;
            this.action = action;
        }
    }

    public class FileTreeView : TreeView<FileTreeElement>
    {
        private string rootPath;
        private string[] searchPatterns;

        private int treeItemId = 0;
        private List<FileTreeElement> datas;
        private FileTreeMenuItem[] defaultMenuItems, customMenuItems;
        private List<FileTreeMenuItem> contextMenuItems;

        public bool MultiSelect { get; set; }
        public string Title { get; set; }
        public event Action<string> onSelected;
        public event Action<FileTreeMenuItem, string[]> onContextClicked;

        public FileTreeView() : base(new TreeViewState<int>())
        {
            rowHeight = 20f;
            showAlternatingRowBackgrounds = true;

            datas = new List<FileTreeElement>() { new FileTreeElement("root", -1, -1) };
            var treeModel = new TreeModel<FileTreeElement>(datas);
            Init(treeModel);
            Reload();

            defaultMenuItems = new FileTreeMenuItem[]
            {
                new FileTreeMenuItem("Refresh", FileTreeMenuFlag.Folder, "refresh_folder")
            };
            customMenuItems = new FileTreeMenuItem[] { };
            contextMenuItems = new List<FileTreeMenuItem>();
        }

        public void SetCustomMenuItem(FileTreeMenuItem[] menuItems)
        {
            customMenuItems = menuItems;
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.BeginArea(rect);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(Title, EditorStyles.boldLabel);

            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();

            var lastRect = GUILayoutUtility.GetLastRect();
            float headerHeight = lastRect.y + lastRect.height;
            var treeViewRect = new Rect(0f, headerHeight, rect.width, rect.height - headerHeight);

            base.OnGUI(treeViewRect);
            GUILayout.EndArea();
        }

        public void SetPath(string path, params string[] searchPatterns)
        {
            if (Directory.Exists(path) == false)
                return;

            rootPath = path;
            this.searchPatterns = searchPatterns;
            treeItemId = 0;

            RefreshFolder(rootPath);
            ExpandAll();
        }

        public void Refresh()
        {
            SetPath(rootPath, searchPatterns);
        }

        void BuildTreeData(string path, ref List<FileTreeElement> list)
        {
            list.Add(new FileTreeElement("root", -1, -1));

            if (Directory.Exists(path) == false)
                return;

            string[] files = Paths.GetFiles(path, searchPatterns)
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
                                list.Add(new FileTreeElement(directoryPath, depth, treeItemId++));
                        }
                        else
                        {
                            list.Add(new FileTreeElement(files[i], depth, treeItemId++));
                        }
                    }
                }
            }
        }

        void RefreshFolder(string path)
        {
            TreeElement parent = datas[0]; // root
            TreeElement element = FindItemByPath(treeModel.root, path);
            if (element != null)
                parent = element.parent;

            if (parent.children == null)
                parent.children = new List<TreeElement>();

            int index = parent.children.FindIndex(x => (x as FileTreeElement).path == path);
            index = Mathf.Max(index, 0);

            using (new ListPoolScope<FileTreeElement>(out var list))
            {
                BuildTreeData(path, ref list);

                var newRoot = TreeElementUtility.ListToTree(list);
                treeModel.ReplaceElement(parent, index, newRoot.hasChildren ? newRoot.children[0] : null);
            }
        }

        private FileTreeElement FindItemByPath(FileTreeElement parent, string path)
        {
            if (parent.path == path)
                return parent;

            if (parent.hasChildren)
            {
                foreach (var child in parent.children)
                {
                    var found = FindItemByPath(child as FileTreeElement, path);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        protected override void OnRowGUI(Rect rect, TreeViewItem item, FileTreeElement element, ref RowGUIArgs args)
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
                    switch (element.extension)
                    {
                        case ".asset": GUI.DrawTexture(iconRect, GUIContents.ScriptableObject_Icon?.image, ScaleMode.ScaleToFit); break;
                        case ".xlsx":
                        case ".xls": GUI.DrawTexture(iconRect, GUIContents.UxmlScript_Icon?.image, ScaleMode.ScaleToFit); break;
                        default: GUI.DrawTexture(iconRect, GUIContents.DefaultAsset_Icon?.image, ScaleMode.ScaleToFit); break;
                    }
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
                onSelected?.Invoke(element.path);
        }

        protected override bool CanMultiSelect(TreeViewItem<int> item)
        {
            return MultiSelect;
        }

        protected override void ContextClicked()
        {
            IList<int> ids = GetSelection();
            var elements = ids.Select(x => treeModel.Find(x))
                            .Where(x => x != null)
                            .ToArray();

            if (elements.Length > 0)
            {
                FileTreeMenuFlag flag = 0;
                foreach (var element in elements)
                {
                    if (element.isDirectory) flag |= FileTreeMenuFlag.Folder;
                    else flag |= FileTreeMenuFlag.File;
                }

                contextMenuItems.Clear();
                contextMenuItems.AddRange(defaultMenuItems.Where(x => (x.flag & flag) != 0));
                contextMenuItems.AddRange(customMenuItems.Where(x => (x.flag & flag) != 0));

                if (contextMenuItems.Count > 0)
                {
                    Vector2 position = Event.current.mousePosition;
                    EditorUtility.DisplayCustomMenu(new Rect(position.x, position.y, 0, 0), contextMenuItems.Select(x => x.content).ToArray(), -1, OnContextMenuSelected, elements);
                }
            }
        }

        void OnContextMenuSelected(object userData, string[] options, int selected)
        {
            var elements = userData as FileTreeElement[];
            var selectedMenuItem = contextMenuItems[selected];

            switch (selectedMenuItem.action)
            {
                case "refresh_folder":
                    {
                        foreach (var element in elements)
                        {
                            if (element.isDirectory)
                                RefreshFolder(element.path);
                        }
                        break;
                    }
            }

            onContextClicked?.Invoke(selectedMenuItem, elements.Select(x => x.path).ToArray());
        }
    }
}