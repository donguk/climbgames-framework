using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;

namespace ClimbGames.Editor
{
    public class ClimbGamesSettingsProvider<T> : SettingsProvider where T : class, IClimbGamesSettings, new()
    {
        private SerializedObject m_SettingsSerializedObject;

        public ClimbGamesSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            m_SettingsSerializedObject = new SerializedObject(ClimbGamesSettings.instance);
        }

        public override void OnGUI(string searchContext)
        {
            if (m_SettingsSerializedObject == null || m_SettingsSerializedObject.targetObject == null)
                m_SettingsSerializedObject = new SerializedObject(ClimbGamesSettings.instance);

            m_SettingsSerializedObject.Update();

            var serializedProperty = GetSerializedProperty(m_SettingsSerializedObject);
            if (serializedProperty != null)
            {
                SerializedProperty childProp = serializedProperty.Copy();
                SerializedProperty endProp = serializedProperty.GetEndProperty();

                if (childProp.NextVisible(true))
                {
                    do
                    {
                        if (SerializedProperty.EqualContents(childProp, endProp))
                            break;

                        EditorGUILayout.PropertyField(childProp, true);
                    }
                    while (childProp.NextVisible(false));
                }
            }

            if (m_SettingsSerializedObject.ApplyModifiedProperties())
                ClimbGamesSettings.instance.Save();
        }

        public static SerializedProperty GetSerializedProperty(SerializedObject serializedObject)
        {
            ClimbGamesSettings<T>.GetOrCreateIfNotExist();
            SerializedProperty listProp = serializedObject.FindProperty("settings");

            if (listProp == null || listProp.isArray == false)
                throw new System.Exception("[ClimbGamesSettings] settings is invalid...");

            for (int i = 0; i < listProp.arraySize; i++)
            {
                var elementProp = listProp.GetArrayElementAtIndex(i);

                // [SerializeReference]로 선언된 리스트인 경우 실제 인스턴스는 managedReferenceValue에 담깁니다.
                if (elementProp.propertyType == SerializedPropertyType.ManagedReference)
                {
                    if (elementProp.managedReferenceValue is T targetItem)
                        return elementProp;
                }
                // 일반 [SerializeField] 리스트 객체인 경우 (만약 구체 클래스 리스트라면)
                else if (elementProp.boxedValue is T boxedItem)
                {
                    return elementProp;
                }
            }

            throw new System.Exception($"[ClimbGamesSettings] can not find {typeof(T).Name}");
        }
    }

    public static class ClimbGamesSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider SettingsProvider()
        {
            var provider = new SettingsProvider("Project/ClimbGames", SettingsScope.Project)
            {
                label = "ClimbGames",
                guiHandler = (searchContext) =>
                {
                    EditorGUILayout.Space(10);
                    EditorGUILayout.HelpBox("ClimbGames Framework Settings Pages", MessageType.Info);
                },
                keywords = new[] { "ClimbGames", "Framework" }
            };

            return provider;
        }
    }
}