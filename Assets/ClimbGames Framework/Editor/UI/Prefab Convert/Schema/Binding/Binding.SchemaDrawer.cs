using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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

                bool isReadonly = property.FindPropertyRelative("isReadonly").boolValue;
                bool originEnabled = GUI.enabled;
                GUI.enabled = isReadonly == false;

                // prefix
                Rect rect = new Rect(position.x, position.y, position.width, lineHeight);
                SerializedProperty serializedProperty = property.FindPropertyRelative($"prefix");
                if (serializedProperty != null)
                {
                    title.text = $"Prefix{(GUI.enabled == false ? " (readonly)" : string.Empty)}";
                    EditorGUI.PropertyField(rect, serializedProperty, title);
                }

                // options
                SchemaOption options = SchemaOption.None;
                rect = new Rect(position.x, rect.y + lineHeight + spacing, position.width, lineHeight);
                serializedProperty = property.FindPropertyRelative("options");
                if (serializedProperty != null)
                {
                    options = (SchemaOption)serializedProperty.intValue;
                    EditorGUI.PropertyField(rect, serializedProperty);
                }

                // typeName
                rect = new Rect(position.x, rect.y + lineHeight + spacing, position.width, lineHeight);
                serializedProperty = property.FindPropertyRelative("typeName");
                if (serializedProperty != null)
                {
                    string typeName = serializedProperty.stringValue;
                    if (string.IsNullOrEmpty(typeName))
                        EditorGUI.LabelField(rect, "Type", "Auto Generated Type", EditorStyles.boldLabel);
                    else
                        EditorGUI.PropertyField(rect, serializedProperty);
                }

                if ((options & SchemaOption.Fallback) != 0)
                {
                    // fallback
                    serializedProperty = property.FindPropertyRelative("fallback");
                    if (serializedProperty != null)
                    {
                        rect = new Rect(position.x, rect.y + lineHeight + spacing, position.width, lineHeight);
                        EditorGUI.PropertyField(rect, serializedProperty);
                    }
                }

                GUI.enabled = originEnabled;
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                float lineHeight = EditorGUIUtility.singleLineHeight;
                float spacing = EditorGUIUtility.standardVerticalSpacing;
                float propertyHeight = 0f;

                propertyHeight += lineHeight + spacing;
                propertyHeight += lineHeight + spacing;
                propertyHeight += lineHeight + spacing;

                SerializedProperty serializedProperty = property.FindPropertyRelative("options");
                SchemaOption options = serializedProperty != null ? (SchemaOption)serializedProperty.intValue : SchemaOption.None;

                if ((options & SchemaOption.Fallback) != 0)
                {
                    serializedProperty = property.FindPropertyRelative("fallback");
                    propertyHeight += EditorGUI.GetPropertyHeight(serializedProperty, true) + spacing;
                }

                return propertyHeight;
            }
        }
    }
}