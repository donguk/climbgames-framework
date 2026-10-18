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

                EditorGUI.BeginChangeCheck();

                SerializedProperty serializedProperty = property.FindPropertyRelative("options");
                SchemaOption options = serializedProperty != null ? (SchemaOption)serializedProperty.intValue : SchemaOption.None;

                bool originEnabled = GUI.enabled;
                GUI.enabled = (options & SchemaOption.Readonly) == 0;

                // prefix
                Rect rect = new Rect(position.x, position.y, position.width, lineHeight);
                serializedProperty = property.FindPropertyRelative($"prefix");
                if (serializedProperty != null)
                {
                    title.text = $"Prefix{(GUI.enabled == false ? " (readonly)" : string.Empty)}";
                    EditorGUI.PropertyField(rect, serializedProperty, title);
                }

                // options
                //rect = new Rect(position.x, rect.y + lineHeight + spacing, position.width, lineHeight);
                //SchemaOption selectedOptions = (SchemaOption)EditorGUI.EnumPopup(rect, "Options", options);
                //if (selectedOptions != options)
                //{
                //    options = selectedOptions;
                //}

                // typeName
                rect = new Rect(position.x, rect.y + lineHeight + spacing, position.width, lineHeight);
                serializedProperty = property.FindPropertyRelative("typeName");
                if (serializedProperty != null)
                    EditorGUI.PropertyField(rect, serializedProperty);

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

                if (EditorGUI.EndChangeCheck())
                    UISettings.Instance.SetDirty();

                GUI.enabled = originEnabled;
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                float lineHeight = EditorGUIUtility.singleLineHeight;
                float spacing = EditorGUIUtility.standardVerticalSpacing;
                float propertyHeight = (lineHeight + spacing) * 2f;

                SerializedProperty serializedProperty = property.FindPropertyRelative("options");
                SchemaOption options = serializedProperty != null ? (SchemaOption)serializedProperty.intValue : SchemaOption.None;

                if ((options & SchemaOption.Fallback) != 0)
                {
                    serializedProperty = property.FindPropertyRelative("fallback");
                    propertyHeight += EditorGUI.GetPropertyHeight(serializedProperty, true);
                }

                return propertyHeight;
            }
        }
    }
}