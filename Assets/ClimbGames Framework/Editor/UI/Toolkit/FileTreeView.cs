using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ClimbGames.Editor.UI
{
    [Serializable]
    public class FileEntry
    {
        public int id;
        public Texture icon;
        public string name;
        public string extension;
        public string path;
        public bool isDirectory;
        public long size;
        public DateTime modified;
    }

    [UxmlElement]
    public partial class FileTreeView : VisualElement
    {
        private const string UxmlGUID = "ec1e011fbf3561248b5588d92c341ec1";

        private Label title;
        private MultiColumnTreeView treeView;

        private List<TreeViewItemData<FileEntry>> datas;

        [UxmlAttribute]
        public string Title
        {
            get => title?.text;
            set { if (title != null) title.text = value; }
        }

        public Columns Columns => treeView.columns;

        public FileTreeView()
        {
            string uxmlPath = AssetDatabase.GUIDToAssetPath(UxmlGUID);

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            visualTree.CloneTree(this);

            title = this.Q<Label>("title");
            treeView = this.Q<MultiColumnTreeView>("tree_view");
            treeView.showAlternatingRowBackgrounds = AlternatingRowBackground.All;


            var fileCell = treeView.columns["file"];
            fileCell.bindCell = (element, index) =>
            {
                var entry = treeView.GetItemDataForIndex<FileEntry>(index);
                element.dataSource = entry;
                element.style.height = Length.Percent(100);

                var icon = element.Q<Image>("icon");
                if (entry.isDirectory && treeView.IsExpanded(entry.id))
                {
                    icon.image = GUIContents.FolderOpened_Icon?.image;
                }
                else
                {
                    icon.image = entry.icon;
                }
            };

            var modifiedCell = treeView.columns["modified"];
            modifiedCell.bindCell = (element, index) =>
            {
                var entry = treeView.GetItemDataForIndex<FileEntry>(index);
                element.style.height = Length.Percent(100);

                var label = element.Q<Label>("modified");
                label.text = entry.modified.ToString("yyyy-MM-dd HH:mm");
            };

            var sizeCell = treeView.columns["size"];
            sizeCell.bindCell = (element, index) =>
            {
                var entry = treeView.GetItemDataForIndex<FileEntry>(index);
                element.style.height = Length.Percent(100);

                var label = element.Q<Label>("size");
                label.text = entry.isDirectory ? "" : FormatFileSize(entry.size);
            };
        }

        public void SetRootPath(string path, params string[] searchPatterns)
        {
            datas = BuildEntry(path, searchPatterns);

            treeView.SetRootItems(datas);
            treeView.ExpandItem(datas[0].id);
        }

        public FileEntry GetItemDataForIndex(int index) => treeView.GetItemDataForIndex<FileEntry>(index);

        public static List<TreeViewItemData<FileEntry>> BuildEntry(string path, params string[] searchPatterns)
        {
            var roots = new List<TreeViewItemData<FileEntry>>();

            int id = 0;

            var rootEntry = CreateEntry(id++, path);
            var rootItem = CreateTreeItem(rootEntry, ref id, searchPatterns);

            roots.Add(rootItem);
            return roots;
        }

        private static TreeViewItemData<FileEntry> CreateTreeItem(FileEntry entry, ref int id, params string[] searchPatterns)
        {
            var children = new List<TreeViewItemData<FileEntry>>();

            if (entry.isDirectory)
            {
                string[] directories;
                string[] files;

                try
                {
                    directories = Directory.GetDirectories(entry.path);
                    files = Paths.GetFiles(entry.path, searchPatterns);
                }
                catch
                {
                    directories = Array.Empty<string>();
                    files = Array.Empty<string>();
                }

                // ------------------------------
                // Directories
                // ------------------------------
                foreach (var directory in directories)
                {
                    var child = CreateEntry(id++, directory);
                    children.Add(CreateTreeItem(child, ref id, searchPatterns));
                }

                // ------------------------------
                // Files
                // ------------------------------
                foreach (var file in files)
                {
                    var child = CreateEntry(id++, file);
                    children.Add(new TreeViewItemData<FileEntry>(child.id, child));
                }
            }

            return new TreeViewItemData<FileEntry>(entry.id, entry, children);
        }

        private static FileEntry CreateEntry(int id, string path)
        {
            bool isDirectory = Directory.Exists(path);

            Texture icon = null;
            string name = string.Empty;
            string extension = string.Empty;
            long size = 0;
            DateTime modified = DateTime.MinValue;

            try
            {
                if (isDirectory)
                {
                    icon = GUIContents.Folder_Icon.image;
                    name = Path.GetFileName(path);
                    modified = Directory.GetLastWriteTime(path);

                }
                else
                {
                    var info = new FileInfo(path);

                    name = info.Name;
                    extension = info.Extension;
                    size = info.Length;
                    modified = info.LastWriteTime;

                    switch (extension)
                    {
                        case ".prefab": icon = GUIContents.d_Prefab_Icon?.image; break;
                        case ".asset": icon = GUIContents.ScriptableObject_Icon?.image; break;
                        case ".xlsx":
                        case ".xls": icon = GUIContents.UxmlScript_Icon?.image; break;
                        default: icon = GUIContents.DefaultAsset_Icon?.image; ; break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FileTree] CreateEntry: {ex}");
            }

            return new FileEntry
            {
                id = id,
                icon = icon,
                name = name,
                extension = extension,
                path = path,
                isDirectory = isDirectory,
                size = size,
                modified = modified
            };
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";

            if (bytes < 1024 * 1024)
                return $"{bytes / 1024f:0.0} KB";

            if (bytes < 1024 * 1024 * 1024)
                return $"{bytes / (1024f * 1024f):0.0} MB";

            return $"{bytes / (1024f * 1024f * 1024f):0.0} GB";
        }
    }
}