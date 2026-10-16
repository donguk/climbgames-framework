using System;
using System.Collections.Generic;
using System.Linq;
using ClimbGames.Editor;
using UnityEditor;
using UnityEngine;

namespace ClimbGames
{
    public class SelectTypesWindow : EditorWindow
    {
        private List<Type> types;

        private string searchText = "";
        private Vector2 scrollPosition;
        private Action<Type> onComplete;
        private Type selectedType;
        IEnumerable<Type> filteredTypes;

        public static void Open(Type baseType, Action<Type> onComplete)
        {
            var window = CreateInstance<SelectTypesWindow>();

            window.types = TypeCache.GetTypesDerivedFrom(baseType)
                .Where(type => type.IsGenericType == false)
                .OrderBy(type => type.FullName)
                .ToList();

            window.filteredTypes = window.types;
            window.onComplete = onComplete;
            window.titleContent = new GUIContent("Select Types");
            window.minSize = new Vector2(320, 400);
            window.ShowUtility();
        }

        private void OnLostFocus()
        {
            Close();
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            searchText = EditorGUILayout.TextField(searchText, EditorStyles.toolbarSearchField, GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck())
                filteredTypes = GetFilteredTypes();

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (var type in filteredTypes)
            {
                Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
                DrawItem(rect, type);
            }
            EditorGUILayout.EndScrollView();
        }

        private IEnumerable<Type> GetFilteredTypes()
        {
            return types.Where(type => type.FullName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void DrawItem(Rect rect, Type type)
        {
            // 선택된 항목의 배경
            if (selectedType == type)
                GUI.Box(rect, GUIContent.none, EditorStyles.selectionRect);

            // 검색 아이콘 + 타입 이름
            GUIContent content = new GUIContent(type.FullName, GUIContents.Search_Icon.image);
            GUI.Label(rect, content, EditorStyles.label);

            // 더블 클릭 처리
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
            {
                selectedType = type;

                if (e.clickCount == 2)
                {
                    onComplete?.Invoke(type);
                    Close();
                }

                e.Use();
                Repaint();
            }
        }
    }
}