using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ClimbGames.Editor.UI;

namespace ClimbGames.Editor
{
    [CustomPropertyDrawer(typeof(PrefabConvertSettings))]
    public class PrefabConvertSettingsDrawer : PropertyDrawer
    {
        private ReorderableList bindingList;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.BeginChangeCheck();

            // 제목
            Rect rect = new Rect(position.x, position.y + 10f, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(rect, "Prefab Convert Settings", EditorStyles.boldLabel);
            rect.y += rect.height + spacing;

            if (EditorGUIs.SelectPathField(rect, "Origin Prefab Path", PrefabConvertSettings.OriginPath, out var selectedPath))
                PrefabConvertSettings.OriginPath = selectedPath;
            rect.y += lineHeight + spacing;

            if (EditorGUIs.SelectPathField(rect, "Convert Prefab Path", PrefabConvertSettings.OutputPath, out selectedPath))
                PrefabConvertSettings.OutputPath = selectedPath;
            rect.y += lineHeight + spacing;

            if (EditorGUIs.SelectPathField(rect, "CodeGen Script Path", PrefabConvertSettings.ScriptPath, out selectedPath))
                PrefabConvertSettings.ScriptPath = selectedPath;
            rect.y += lineHeight + spacing;

            if (bindingList == null)
                InitList(property);

            var schemasProperty = property.FindPropertyRelative("bindingSchemas");
            Rect headerRect = new Rect(rect.x, rect.yMax, position.width, lineHeight);
            schemasProperty.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(headerRect, schemasProperty.isExpanded, "Binding Schemas");
            EditorGUI.EndFoldoutHeaderGroup();

            if (schemasProperty.isExpanded)
            {
                rect = new Rect(rect.x, headerRect.yMax + spacing * 2, position.width, bindingList.GetHeight());
                bindingList.DoList(rect);

                rect.y += rect.height + spacing;
                rect.height = lineHeight;

                if (GUI.Button(rect, "Reset Schemas"))
                {
                    var target = (PrefabConvertSettings)property.GetTargetObjectOfProperty();
                    target.ResetSchemas();
                }
            }


            if (EditorGUI.EndChangeCheck())
                UISettings.Instance.SetDirty();
        }

        private void InitList(SerializedProperty property)
        {
            var schemasProperty = property.FindPropertyRelative("bindingSchemas");

            bindingList = new ReorderableList(
                property.serializedObject,
                schemasProperty,
                draggable: true,
                displayHeader: false,
                displayAddButton: true,
                displayRemoveButton: true)
            {
                //drawHeaderCallback = rect =>
                //{
                //    EditorGUI.LabelField(rect, "Binding Schemas");
                //},

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var element = schemasProperty.GetArrayElementAtIndex(index);

                    rect.y += 2f;
                    rect.height = EditorGUI.GetPropertyHeight(element, true);

                    EditorGUI.PropertyField(rect, element, GUIContent.none, true);
                },

                elementHeightCallback = index =>
                {
                    var element = schemasProperty.GetArrayElementAtIndex(index);

                    return EditorGUI.GetPropertyHeight(element, true);
                },

                onAddCallback = list =>
                {
                    int index = schemasProperty.arraySize;
                    schemasProperty.InsertArrayElementAtIndex(index);

                    var element = schemasProperty.GetArrayElementAtIndex(index);
                    element.managedReferenceValue = new Binding.ComponentSchema("trs_", typeof(Transform), Binding.SchemaOption.Fallback | Binding.SchemaOption.IncludeChildren);
                    schemasProperty.serializedObject.ApplyModifiedProperties();
                },

                onRemoveCallback = list =>
                {
                    int index = list.index;

                    if (index < 0 || index >= schemasProperty.arraySize)
                        return;

                    var element = schemasProperty.GetArrayElementAtIndex(index);
                    if (element.FindPropertyRelative("isReadonly").boolValue)
                        return;

                    schemasProperty.DeleteArrayElementAtIndex(index);
                    schemasProperty.serializedObject.ApplyModifiedProperties();
                },
            };
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            float height =
                lineHeight + spacing + 10f + // 제목
                lineHeight + spacing + // Origin Path
                lineHeight + spacing + // Output Path
                lineHeight + spacing; // Script Path

            if (bindingList == null)
                InitList(property);

            lineHeight += (lineHeight + spacing) * 2;

            var schemasProperty = property.FindPropertyRelative("bindingSchemas");
            if (schemasProperty.isExpanded)
            {
                height += bindingList.GetHeight() + spacing;
                height += lineHeight;
            }

            height += spacing;
            return height;
        }
    }
}