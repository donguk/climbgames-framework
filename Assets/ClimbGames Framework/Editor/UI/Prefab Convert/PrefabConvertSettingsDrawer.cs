using System.Linq;
using ClimbGames.Editor.UI;
using Codice.Client.Common.WebApi.Responses;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

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

            if (EditorGUIs.SelectPathField(rect, "Origin Path", PrefabConvertSettings.OriginPath, out var selectedPath))
                PrefabConvertSettings.OriginPath = selectedPath;
            rect.y += lineHeight + spacing;

            if (EditorGUIs.SelectPathField(rect, "Output Path", PrefabConvertSettings.OutputPath, out selectedPath))
                PrefabConvertSettings.OutputPath = selectedPath;
            rect.y += lineHeight + spacing;

            if (EditorGUIs.SelectPathField(rect, "Script Path", PrefabConvertSettings.ScriptPath, out selectedPath))
                PrefabConvertSettings.ScriptPath = selectedPath;
            rect.y += lineHeight + spacing;


            if (bindingList == null)
                InitList(property);

            rect = new Rect(position.x, rect.y + 10f, position.width, bindingList.GetHeight());
            bindingList.DoList(rect);

            rect.y += rect.height + spacing;
            rect.height = lineHeight;

            if (GUI.Button(rect, "Reset Schemas"))
            {
                var target = (PrefabConvertSettings)property.GetTargetObjectOfProperty();
                target.ResetSchemas();
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
                displayHeader: true,
                displayAddButton: true,
                displayRemoveButton: true)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "Binding Schemas");
                },

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

                    return EditorGUI.GetPropertyHeight(element, true) + 4f;
                },

                onAddCallback = list =>
                {
                    int index = schemasProperty.arraySize;
                    schemasProperty.InsertArrayElementAtIndex(index);

                    var element = schemasProperty.GetArrayElementAtIndex(index);

                    element.FindPropertyRelative("prefix").stringValue = "";
                    element.FindPropertyRelative("typeName").stringValue = "";
                    element.FindPropertyRelative("bindingType").enumValueIndex = (int)BindingType.Component;
                    element.FindPropertyRelative("isReadOnly").boolValue = false;

                    schemasProperty.serializedObject.ApplyModifiedProperties();
                },

                onRemoveCallback = list =>
                {
                    int index = list.index;

                    if (index < 0 || index >= schemasProperty.arraySize)
                        return;

                    var element = schemasProperty.GetArrayElementAtIndex(index);
                    if (element.FindPropertyRelative("isReadOnly").boolValue)
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
                lineHeight + spacing + 10f; // Script Path

            if (bindingList == null)
                InitList(property);

            height += bindingList.GetHeight() + spacing;
            height += lineHeight;

            return height;
        }
    }
}