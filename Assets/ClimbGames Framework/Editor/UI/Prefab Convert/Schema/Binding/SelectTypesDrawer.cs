using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SelectTypesAttribute : PropertyAttribute
    {
        public string label;

        public SelectTypesAttribute(string label = default)
        {
            this.label = label;
        }
    }

    [CustomPropertyDrawer(typeof(SelectTypesAttribute))]
    public class SelectTypesDrawer : PropertyDrawer
    {
        private StringBuilder builder;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attribute = this.attribute as SelectTypesAttribute;

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.BeginProperty(position, label, property);

            float buttonWidth = 100f;
            Rect rect = new Rect(position.x, position.y, position.width - buttonWidth, lineHeight);

            string typeName = GetDisplayName(property.stringValue);
            string _label = attribute.label != null ? attribute.label : label.text;
            EditorGUI.LabelField(rect, _label, typeName, EditorStyles.boldLabel);

            Rect buttonRect = new Rect(position.x + (position.width - buttonWidth), rect.y, buttonWidth, lineHeight);
            if (GUI.Button(buttonRect, "Select Types"))
                SelectTypesWindow.Open(typeof(Component), result => OnTypeSelected(property, result));

            EditorGUI.EndProperty();
        }

        void OnTypeSelected(SerializedProperty property, Type selectedType)
        {
            if (selectedType == null)
                return;

            property.stringValue = selectedType.AssemblyQualifiedName;
            property.serializedObject.ApplyModifiedProperties();
        }

        public string GetDisplayName(string typeName)
        {
            if (builder == null)
                builder = new StringBuilder();

            builder.Clear();
            string[] value = typeName.Split(',');

            int count = Math.Min(2, value.Length);
            for (int i = 0; i < count; ++i)
            {
                if (builder.Length > 0)
                    builder.Append(",");

                builder.Append(value[i]);
            }

            return builder.ToString();
        }
    }
}