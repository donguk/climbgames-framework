using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ClimbGames.Editor.Table
{
    public class TableAssetTreeElement : TreeElement
    {
        public string assetPath;

        public TableAssetTreeElement(string name, int depth, int id) : base(name, depth, id)
        {

        }
    }

    public class TableAssetTreeView : TreeView<TableAssetTreeElement>
    {
        public event Action<string> onSelected;

        public TableAssetTreeView(TreeViewState<int> state, TreeModel<TableAssetTreeElement> model) : base(state, model)
        {
            rowHeight = 20f;
            showAlternatingRowBackgrounds = true;
        }

        protected override void OnRowGUI(Rect rect, TreeViewItem item, TableAssetTreeElement element, ref RowGUIArgs args)
        {
            if (element != null)
            {
                Rect iconRect = rect;
                iconRect.x += GetContentIndent(item);
                iconRect.width = 20f;

                if (string.IsNullOrEmpty(element.assetPath))
                {
                    GUI.DrawTexture(iconRect, IsExpanded(element.id) ? GUIContentUtility.FolderOpened_Icon?.image : GUIContentUtility.Folder_Icon?.image, ScaleMode.ScaleToFit);
                }
                else
                {
                    GUI.DrawTexture(iconRect, GUIContentUtility.ScriptableObject_Icon?.image, ScaleMode.ScaleToFit);
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
                onSelected?.Invoke(element.assetPath);
        }

        protected override bool CanMultiSelect(TreeViewItem<int> item)
        {
            return false;
        }
    }
}