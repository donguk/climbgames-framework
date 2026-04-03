using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ClimbGames.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        private ReorderableList _reorderableList;
        private string _propertyHash;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.serializedObject.Update();

            SerializedProperty listProp = property.FindPropertyRelative("list");
            if (listProp == null)
            {
                EditorGUI.LabelField(position, "Error: 'list' field not found.");
                return;
            }

            if (_reorderableList == null || _propertyHash != property.propertyPath)
            {
                _propertyHash = property.propertyPath;
                _reorderableList = new ReorderableList(property.serializedObject, listProp, true, false, true, true);

                //_reorderableList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, label.text);

                // 요소 높이 계산: Value가 List<T>여도 정밀하게 계산됨
                _reorderableList.elementHeightCallback = index =>
                {
                    if (index >= listProp.arraySize) return 22f;
                    var pair = listProp.GetArrayElementAtIndex(index);
                    var valueProp = pair.FindPropertyRelative("Value");
                    return EditorGUI.GetPropertyHeight(valueProp, true);
                };

                _reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var pair = listProp.GetArrayElementAtIndex(index);
                    var keyProp = pair.FindPropertyRelative("Key");
                    var valueProp = pair.FindPropertyRelative("Value");

                    float originLabelWidth = EditorGUIUtility.labelWidth;
                    float spaceWidth = 10f;
                    if (valueProp.isArray || valueProp.propertyType == SerializedPropertyType.Generic)
                        spaceWidth = Mathf.Max(spaceWidth, 16f);

                    float keyWidth = rect.width * 0.45f;
                    float valWidth = rect.width * 0.55f;
                    rect.y += 1f;

                    // Key 그리기
                    var keyConetent = new GUIContent("Key");
                    EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(keyConetent).x;
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, keyWidth, EditorGUIUtility.singleLineHeight), keyProp, keyConetent);

                    // Value 그리기 (List<T>나 클래스여도 includeChildren: true로 인해 자동 Fold 대응)
                    var valueConetent = new GUIContent("Value");
                    EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(valueConetent).x;
                    Rect valRect = new Rect(rect.x + keyWidth + spaceWidth, rect.y, valWidth - spaceWidth, EditorGUI.GetPropertyHeight(valueProp, true));
                    EditorGUI.PropertyField(valRect, valueProp, valueConetent, true);

                    EditorGUIUtility.labelWidth = originLabelWidth;
                };
            }

            // Foldout 처리
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, 20), property.isExpanded, label, true);
            if (property.isExpanded)
                _reorderableList.DoList(new Rect(position.x, position.y + 22, position.width, position.height));

            if (property.serializedObject.ApplyModifiedProperties())
                EditorUtility.SetDirty(property.serializedObject.targetObject);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            return (_reorderableList != null ? _reorderableList.GetHeight() : EditorGUIUtility.singleLineHeight) + 22f;
        }
    }
}