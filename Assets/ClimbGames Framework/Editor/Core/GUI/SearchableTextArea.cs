using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ClimbGames.Editor
{
    public class SearchableTextArea
    {
        private EditorWindow window;
        private string _title;
        private string _text;
        private bool isTextChanged;
        private Vector2 scrollPosition;
        private TextSearchBar searchBar;
        private bool isSearchClicked;
        private string controlName;

        public string Title { get => _title; set => _title = value; }
        public string Text => _text;
        public bool IsChanged { get; private set; }

        public SearchableTextArea(EditorWindow window)
        {
            controlName = $"TextArea_{GetHashCode()}";

            this.window = window;
            searchBar = new TextSearchBar();

            searchBar.onSearchClicked += (match) => isSearchClicked = true;

            GetHashCode();
        }

        public void Draw(Rect rect)
        {
            GUILayout.BeginArea(rect);
            EditorGUILayout.LabelField(_title, EditorStyles.boldLabel);
            searchBar.Draw();

            Rect lastRect = GUILayoutUtility.GetLastRect();
            float headerHeight = lastRect.y + lastRect.height;
            Rect scrollRect = new Rect(0f, headerHeight, rect.width, rect.height - headerHeight);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            GUI.SetNextControlName(controlName);

            EditorGUI.BeginChangeCheck();
            _text = GUILayout.TextArea(_text, EditorStyles.textArea, GUILayout.ExpandHeight(true));
            if (EditorGUI.EndChangeCheck())
            {
                isTextChanged = true;
                searchBar.SetText(_text);
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();

            HandleSearch();
            HandleScrollBar(scrollRect);
        }

        public void SetText(string text)
        {
            _text = text;
            searchBar.SetText(text);
            IsChanged = false;
        }

        private void SelectMatchText(Match match)
        {
            if (match == null)
                return;

            TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            if (textEditor != null)
            {
                textEditor.OnFocus(); // 포커스 활성화 처리
                textEditor.selectIndex = match.Index;
                textEditor.cursorIndex = match.Index + match.Value.Length;

                Vector2 cursorPos = textEditor.graphicalCursorPos;
                // 스크롤 뷰의 y축을 커서 위치로 바로 매칭
                // 화면 맨 위에 딱 붙는 게 싫다면 화면 높이의 절반 정도를 빼주면 중앙에 위치합니다.
                scrollPosition.y = cursorPos.y - 40f;
                window.Repaint();
            }
        }

        void HandleSearch()
        {
            Event e = Event.current;
            if (e != null && e.type == EventType.Repaint && isSearchClicked)
            {
                var match = searchBar.Match;
                if (match != null)
                {
                    GUI.FocusControl(controlName);
                    SelectMatchText(match);
                }

                isSearchClicked = false;
            }
        }

        void HandleScrollBar(Rect rect)
        {
            Event e = Event.current;
            if (e != null && Event.current.type == EventType.Repaint)
            {
                if (isTextChanged && GUI.GetNameOfFocusedControl() == controlName)
                {
                    TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                    if (textEditor != null)
                    {
                        float cursorY = textEditor.graphicalCursorPos.y;
                        float viewportHeight = rect.height;

                        // 폰트 크기 및 줄 바꿈 여유 공간 패딩
                        float realLineHeight = EditorStyles.textArea.lineHeight > 0 ? EditorStyles.textArea.lineHeight : 14f;
                        float linePadding = realLineHeight;

                        if (cursorY > scrollPosition.y + viewportHeight - linePadding)
                        {
                            // 커서가 화면 최하단에 걸치도록 스크롤 아래로 이동
                            scrollPosition.y = cursorY - viewportHeight + linePadding;
                            //window.Repaint();
                        }
                        else if (cursorY < scrollPosition.y + realLineHeight)
                        {
                            scrollPosition.y = cursorY - realLineHeight;
                            if (scrollPosition.y < 0)
                                scrollPosition.y = 0;

                            window.Repaint();
                        }
                    }

                    isTextChanged = false;
                }
            }
        }
    }
}