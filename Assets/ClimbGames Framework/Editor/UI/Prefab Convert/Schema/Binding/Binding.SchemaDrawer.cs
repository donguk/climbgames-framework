using System;
using UnityEditor;
using UnityEngine;


public class TestSchema
{

}

namespace ClimbGames.Editor.UI
{
    public partial class Binding
    {
        [CustomPropertyDrawer(typeof(Schema))]
        public class SchemaDrawer : PropertyDrawer
        {
            private GUIContent title = new GUIContent();

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                float lineHeight = EditorGUIUtility.singleLineHeight;
                float spacing = EditorGUIUtility.standardVerticalSpacing;

                SerializedProperty readOnlyProperty = property.FindPropertyRelative("isReadOnly");
                bool originEnabled = GUI.enabled;
                GUI.enabled = readOnlyProperty != null && readOnlyProperty.boolValue == false;

                // prefix
                Rect prefixRect = new Rect(position.x, position.y, position.width, lineHeight);
                SerializedProperty serializedProperty = property.FindPropertyRelative($"prefix");
                if (serializedProperty != null)
                {
                    title.text = $"Prefix{(GUI.enabled == false ? " (readonly)" : string.Empty)}";
                    EditorGUI.PropertyField(prefixRect, serializedProperty, title, true);
                }

                // bindingType
                BindingType bindingType = BindingType.None;
                Rect bindingRect = new Rect(position.x, prefixRect.y + lineHeight + spacing, position.width, lineHeight);
                serializedProperty = property.FindPropertyRelative("bindingType");
                if (serializedProperty != null)
                {
                    bindingType = (BindingType)serializedProperty.intValue;
                    BindingType selectedType = (BindingType)EditorGUI.EnumPopup(bindingRect, "Binding", bindingType);
                    if (selectedType != bindingType)
                    {
                        bindingType = selectedType;
                        OnBindingSelect(property, selectedType);
                    }
                }

                // typeName
                serializedProperty = property.FindPropertyRelative("typeName");
                if (serializedProperty != null)
                {
                    float buttonWidth = position.width * 0.12f;
                    Rect typeRect = new Rect(position.x, bindingRect.y + lineHeight + spacing, position.width - buttonWidth, lineHeight);

                    string typeName = serializedProperty.stringValue;
                    typeName = Schema.IsNoneType(bindingType) || string.IsNullOrEmpty(typeName) ? "None" : Schema.GetDisplayName(typeName);
                    EditorGUI.LabelField(typeRect, "Type", typeName, EditorStyles.boldLabel);

                    bool originEnabled_2 = GUI.enabled;
                    GUI.enabled = Schema.IsNoneType(bindingType) == false && GUI.enabled;
                    Rect buttonRect = new Rect(position.x + (position.width - buttonWidth), typeRect.y, buttonWidth, lineHeight);
                    if (GUI.Button(buttonRect, "Select Types"))
                    {
                        SelectTypesWindow.Open(typeof(Component), result => OnTypeSelected(property, result));
                    }
                    GUI.enabled = originEnabled_2;
                }

                GUI.enabled = originEnabled;
            }

            void OnBindingSelect(SerializedProperty property, BindingType selectedType)
            {
                var target = (Schema)property.GetTargetObjectOfProperty();
                if (target != null)
                {
                    if (target.bindingType != selectedType)
                    {
                        target.bindingType = selectedType;
                        UISettings.Instance.SetDirty();
                    }
                }
            }

            void OnTypeSelected(SerializedProperty property, Type selectedType)
            {
                var target = (Schema)property.GetTargetObjectOfProperty();
                if (target != null)
                {
                    if (target.SetType(selectedType))
                        UISettings.Instance.SetDirty();
                }
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 2;
            }
        }
    }
}