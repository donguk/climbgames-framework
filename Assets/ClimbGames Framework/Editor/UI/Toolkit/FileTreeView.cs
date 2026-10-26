using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace ClimbGames.Editor.UI
{
    [Flags]
    public enum FileMenuFlag
    {
        Folder = 1 << 0,
        File = 1 << 1,
        Common = Folder | File,
    }

    public class FileMenuItem
    {
        public string name;
        public FileMenuFlag flag;

        public FileMenuItem(string name, FileMenuFlag flag = FileMenuFlag.Common)
        {
            this.name = name;
            this.flag = flag;
        }
    }

    [UxmlElement]
    public partial class FileTreeView : VisualElement
    {
        private const string UxmlGUID = "ec1e011fbf3561248b5588d92c341ec1";
        private const string FileCellGUID = "2664e5480ad83a946a3821d056d272e5";
        private const string ModifiedCellGUID = "0d59160c1123d394e897525224932da1";
        private const string SizeCellGUID = "12ec519605605d442979642645a5e27e";

        private Label title;
        private MultiColumnTreeView treeView;
        private VisualElement empty;

        private int treeItemId = 0;
        private string rootPath;
        private string[] searchPatterns;
        private Func<int, string, FileEntry> entryProvider;
        private bool excludeRootItem;
        private FileMenuItem[] menuItems;
        private Dictionary<string, FileEntry> directoryEnties;
        private FileExpansionState expansionState;

        public event Action<string> onContextMenuClicked;
        public event Action<MouseDownEvent, int> onFileMouseDownEvent;
        public event Action<TreeViewExpansionChangedArgs> onItemExpandedChanged;

        [UxmlAttribute]
        public string Title
        {
            get => title.text;
            set => title.text = value;
        }

        public Columns Columns => treeView.columns;
        public IEnumerable<int> SelectedIds => treeView.selectedIds;
        public FileExpansionState ExpansionState => expansionState;

        public FileTreeView()
        {
            directoryEnties = new Dictionary<string, FileEntry>();
            expansionState = new FileExpansionState(this);

            string uxmlPath = AssetDatabase.GUIDToAssetPath(UxmlGUID);
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            visualTree.CloneTree(this);

            title = this.Q<Label>("title");
            treeView = this.Q<MultiColumnTreeView>("tree_view");
            treeView.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
            treeView.itemExpandedChanged += OnItemExpandedChanged;
            empty = this.Q<VisualElement>("empty");

            VisualElement clickedElement = null;
            bool IsTreeViewItem(VisualElement element)
            {
                while (element != null && element != treeView)
                {
                    if (element.ClassListContains("unity-collection-view__item"))
                        return true; // 아이템 클릭
                    element = element.parent;
                }
                return false;
            }
            treeView.RegisterCallback<MouseDownEvent>(evt =>
            {
                clickedElement = evt.target as VisualElement;
                if (evt.button != 0)
                    return;

                if (IsTreeViewItem(clickedElement))
                    return;

                // 아이템 외부의 빈 영역 클릭
                treeView.selectedIndex = -1;
            }, TrickleDown.TrickleDown);
            treeView.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                if (IsTreeViewItem(clickedElement))
                    return;

                evt.menu.AppendAction("Refresh", _ =>
                {
                    SetRootPath(rootPath, searchPatterns, entryProvider, excludeRootItem);
                    expansionState.ExpandAll();
                });
            }));

            menuItems = new FileMenuItem[]
            {
                new FileMenuItem("Refresh", FileMenuFlag.Common)
            };

            var fileCell = treeView.columns["file"];
            fileCell.makeCell = () =>
            {
                var template = AssetDatabase.LoadAssetByGUID<VisualTreeAsset>(new GUID(FileCellGUID));
                var cell = template.Instantiate();
                var root = cell.Q<VisualElement>("root");

                root.AddManipulator(new ContextualMenuManipulator((evt) => OnContextMenu(evt)));
                cell.RegisterCallback<MouseDownEvent>(evt =>
                {
                    if (cell.userData is FileEntry entry)
                        onFileMouseDownEvent?.Invoke(evt, entry.id);
                });
                return cell;
            };
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
                element.userData = entry;
            };

            var modifiedCell = treeView.columns["modified"];
            modifiedCell.makeCell = () =>
            {
                var template = AssetDatabase.LoadAssetByGUID<VisualTreeAsset>(new GUID(ModifiedCellGUID));
                var cell = template.Instantiate();

                return cell;
            };
            modifiedCell.bindCell = (element, index) =>
            {
                var entry = treeView.GetItemDataForIndex<FileEntry>(index);
                element.style.height = Length.Percent(100);

                var label = element.Q<Label>("modified");
                label.text = entry.modified.ToString("yyyy-MM-dd HH:mm");
            };

            var sizeCell = treeView.columns["size"];
            sizeCell.makeCell = () =>
            {
                var template = AssetDatabase.LoadAssetByGUID<VisualTreeAsset>(new GUID(SizeCellGUID));
                var cell = template.Instantiate();

                return cell;
            };
            sizeCell.bindCell = (element, index) =>
            {
                var entry = treeView.GetItemDataForIndex<FileEntry>(index);
                element.style.height = Length.Percent(100);

                var label = element.Q<Label>("size");
                label.text = entry.isDirectory ? "" : FormatFileSize(entry.size);
            };
        }

        FileEntry CreateFileEntry(int id, string path)
        {
            FileEntry entry = null;
            if (entryProvider != null)
                entry = entryProvider?.Invoke(id, path);

            if (entry == null)
                entry = new FileEntry(id, path);

            if (entry.isDirectory)
                directoryEnties[path] = entry;

            return entry;
        }

        public void SetRootPath(string path, string[] searchPatterns, Func<int, string, FileEntry> entryProvider, bool excludeRootItem = false)
        {
            rootPath = path;
            this.searchPatterns = searchPatterns;
            this.entryProvider = entryProvider;
            this.excludeRootItem = excludeRootItem;

            if (Directory.Exists(path))
            {
                var rootItem = BuildRootItem(path, searchPatterns, ref treeItemId, CreateFileEntry);

                if (excludeRootItem)
                    SetRootItems(rootItem.children.ToList());
                else
                    SetRootItems(new List<TreeViewItemData<FileEntry>>() { rootItem });
            }
        }

        public void SetRootPath(string path, string[] searchPatterns)
        {
            SetRootPath(path, searchPatterns, (id, path) => new FileEntry(id, path));
        }

        void SetRootItems<T>(List<TreeViewItemData<T>> rootItems) where T : FileEntry
        {
            if (empty != null)
                empty.style.display = rootItems.Count > 0 ? DisplayStyle.None : DisplayStyle.Flex;

            treeView.SetRootItems(rootItems);
            treeView.Rebuild();
            treeView.ExpandRootItems();
        }

        public FileEntry GetItemDataForIndex(int index)
        {
            return treeView.GetItemDataForIndex<FileEntry>(index);
        }

        public T GetItemDataForIndex<T>(int index) where T : FileEntry
        {
            return treeView.GetItemDataForIndex<T>(index);
        }

        public T GetItemDataForId<T>(int id) where T : FileEntry
        {
            return treeView.GetItemDataForId<T>(id);
        }

        public void RefreshItems()
        {
            treeView.RefreshItems();
        }

        public void RefreshItem(int id)
        {
            int index = treeView.viewController.GetIndexForId(id);
            treeView.RefreshItem(index);
        }

        public void SetSelectionById(int id)
        {
            treeView.SetSelectionById(id);
        }

        public void ExpandAll(FileExpansionState state)
        {
            var expandedPath = state.ExpandedPath;
            foreach (var path in expandedPath)
            {
                if (directoryEnties.TryGetValue(path, out var entry))
                    treeView.ExpandItem(entry.id);
            }
        }

        void RefreshDirectory(FileEntry entry)
        {
            if (entry != null && entry.isDirectory)
            {
                int parentId = treeView.viewController.GetParentId(entry.id);
                if (parentId > -1)
                {
                    var parentData = treeView.GetItemDataForId<FileEntry>(parentId);
                    IEnumerable<int> childrenIds = treeView.viewController.GetChildrenIds(parentData.id).ToArray();

                    foreach (var childId in childrenIds)
                        treeView.viewController.TryRemoveItem(childId, false);

                    var tempRoot = BuildRootItem(parentData.path, searchPatterns, ref treeItemId, CreateFileEntry);

                    int childIndex = 0;
                    foreach (var newChild in tempRoot.children)
                        treeView.AddItem(newChild, parentId, childIndex++, false);

                    treeView.Rebuild();
                }
                else
                {
                    SetRootPath(rootPath, searchPatterns, entryProvider, excludeRootItem);
                }
            }
        }

        public void AddFileMenuItem(FileMenuItem[] items)
        {
            menuItems = menuItems.Concat(items).ToArray();
        }

        public void OnContextMenu(ContextualMenuPopulateEvent evt)
        {
            FileMenuFlag flag = 0;
            foreach (var id in treeView.selectedIds)
            {
                var entry = treeView.GetItemDataForId<FileEntry>(id);
                flag |= entry.isDirectory ? FileMenuFlag.Folder : FileMenuFlag.File;
            }

            foreach (var item in menuItems)
            {
                if ((item.flag & flag) != 0)
                {
                    var actionName = item.name;
                    evt.menu.AppendAction(actionName, _ => OnContextMenuClicked(actionName));
                }
            }
        }

        void OnContextMenuClicked(string actionName)
        {
            switch (actionName)
            {
                case "Refresh":
                    {
                        foreach (var selectedId in treeView.selectedIds)
                        {
                            var data = treeView.GetItemDataForId<FileEntry>(selectedId);
                            if (data.isDirectory)
                            {
                                expansionState.SetState(data.path, true);
                                RefreshDirectory(data);
                            }
                        }
                        expansionState.ExpandAll();
                        break;
                    }
            }

            onContextMenuClicked?.Invoke(actionName);
        }

        void OnItemExpandedChanged(TreeViewExpansionChangedArgs args)
        {
            onItemExpandedChanged?.Invoke(args);
        }

        public static TreeViewItemData<FileEntry> BuildRootItem<T>(string path, string[] searchPatterns, ref int id, Func<int, string, T> entryProvider) where T : FileEntry
        {
            var entry = entryProvider.Invoke(id++, path);
            var rootItem = CreateTreeItem(entry, ref id, searchPatterns, entryProvider);

            return rootItem;
        }

        private static TreeViewItemData<FileEntry> CreateTreeItem<T>(T entry, ref int id, string[] searchPatterns, Func<int, string, T> entryProvider) where T : FileEntry
        {
            var children = new List<TreeViewItemData<FileEntry>>();

            if (entry.isDirectory)
            {
                string[] directories;
                string[] files;

                try
                {
                    directories = Directory.GetDirectories(entry.path);
                    files = Paths.GetFiles(entry.path, searchPatterns, SearchOption.TopDirectoryOnly);
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
                    var child = entryProvider.Invoke(id++, directory);
                    children.Add(CreateTreeItem(child, ref id, searchPatterns, entryProvider));
                }

                // ------------------------------
                // Files
                // ------------------------------
                foreach (var file in files)
                {
                    var child = entryProvider.Invoke(id++, file);
                    children.Add(new TreeViewItemData<FileEntry>(child.id, child));
                }
            }

            return new TreeViewItemData<FileEntry>(entry.id, entry, children);
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