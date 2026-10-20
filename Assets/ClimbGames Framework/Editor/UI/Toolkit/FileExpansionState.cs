using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine.UIElements;

namespace ClimbGames.Editor.UI
{
    public class FileExpansionState
    {
        private FileTreeView treeView;
        private HashSet<string> expandedPath = new HashSet<string>();

        public IReadOnlyList<string> ExpandedPath => expandedPath.ToList();

        public FileExpansionState(FileTreeView treeView)
        {
            this.treeView = treeView;
            treeView.onItemExpandedChanged += OnItemExpandedChanged;
        }

        void OnItemExpandedChanged(TreeViewExpansionChangedArgs args)
        {
            var entry = treeView.GetItemDataForId<FileEntry>(args.id);
            SetState(entry.path, args.isExpanded);
        }

        public void SetState(string path, bool isExpanded)
        {
            if (isExpanded)
                expandedPath.Add(path);
            else
                expandedPath.Remove(path);
        }

        public void Clear()
        {
            expandedPath.Clear();
        }

        public void Clear(string key)
        {
            expandedPath.Clear();
            Save(key);
        }

        public void ExpandAll()
        {
            treeView.ExpandAll(this);
        }

        public void ExpandAll(string key)
        {
            Load(key);
            treeView.ExpandAll(this);
        }

        public void Save(string key)
        {
            var ids = string.Join(';', expandedPath.Select(x => x.ToString()));
            EditorPrefs.SetString($"FileExpansionState_{key}", ids);
        }

        public void Load(string key)
        {
            string ids = EditorPrefs.GetString($"FileExpansionState_{key}", string.Empty);
            string[] values = ids.Split(';');

            expandedPath.Clear();
            foreach (var value in values)
                expandedPath.Add(value);
        }
    }
}