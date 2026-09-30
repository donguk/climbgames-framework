using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System;

namespace ClimbGames.Editor.Table
{
    public partial class TableWindow
    {
        private ClimbGames.Table tableAsset;
        private string tableAssetPath, tableName;
        private string rawText, tableJsonText;
        private bool isTableChanged;

        private TextField textField;
        private Scroller textFieldScroller;
        private float savedScrollOffset;

        private string searchWord;
        private List<int> matchIndices = new List<int>();
        private bool searchMatchWord = false;
        private int totalLineCount, matchIndexCursor;

        void InitTableContent()
        {
            textField = new TextField()
            {
                multiline = true,
                selectAllOnFocus = false,
                selectAllOnMouseUp = false,
                verticalScrollerVisibility = ScrollerVisibility.Auto
            };

            textField.style.position = Position.Absolute;
            textField.style.unityTextAlign = TextAnchor.UpperLeft;
            textField.style.whiteSpace = WhiteSpace.Normal;

            textField.RegisterCallback<AttachToPanelEvent>(evt => textFieldScroller = textField.Q<Scroller>());
            textField.RegisterValueChangedCallback(evt =>
            {
                tableJsonText = evt.newValue;
                searchMatchWord = false;
                isTableChanged = true;
            });

            textField.RegisterCallback<MouseDownEvent>(OnTextFieldMouseDown, TrickleDown.TrickleDown);
            textField.RegisterCallback<FocusInEvent>(OnTextFieldFocusIn);

            rootVisualElement.Add(textField);
        }

        void DrawAssetContent(Rect rect)
        {
            rect.x += 4f;
            rect.width -= 8f;

            GUILayout.BeginArea(rect);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(tableName, EditorStyles.boldLabel);

            GUI.enabled = isTableChanged;
            if (GUILayout.Button(GUIContentUtility.SaveActive, GUILayout.Width(26f)))
                SaveTableAsset();

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            DrawSearchField();
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();

            float headerHeight = 44f;
            textField.style.left = rect.x;
            textField.style.top = rect.y + headerHeight;
            textField.style.width = rect.width - 6f;
            textField.style.height = rect.height - headerHeight - 6f;
        }

        void DrawSearchField()
        {
            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName("SearchTextField");

            EditorGUI.BeginChangeCheck();
            searchWord = EditorGUILayout.TextField(searchWord, EditorStyles.toolbarSearchField, GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck())
                searchMatchWord = false;

            if (GUILayout.Button(GUIContentUtility.HoverBar_Down, GUILayout.Width(32f), GUILayout.Height(18f)))
                FocusMatchWord();
            if (GUILayout.Button(GUIContentUtility.HoverBar_Up, GUILayout.Width(32f), GUILayout.Height(18f)))
                FocusMatchWord(true);

            EditorGUILayout.EndHorizontal();
        }

        void SelectTableAsset(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            if (tableAssetPath == filePath)
                return;

            ClimbGames.Table asset = AssetDatabase.LoadAssetAtPath<ClimbGames.Table>(filePath);
            if (asset != null)
            {
                tableAsset = asset;
                tableAssetPath = filePath;
                tableName = Path.GetFileNameWithoutExtension(filePath);

                isTableChanged = false;
                tableJsonText = JsonUtility.ToJson(asset, true);
                textField?.SetValueWithoutNotify(tableJsonText);

                totalLineCount = tableJsonText.Split('\n').Length;
                matchIndexCursor = 0;
                searchMatchWord = false;
            }
        }

        void SaveTableAsset()
        {
            try
            {
                JsonUtility.FromJsonOverwrite(tableJsonText, tableAsset);
                EditorUtility.SetDirty(tableAsset);
                AssetDatabase.SaveAssetIfDirty(tableAsset);
                isTableChanged = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Tables] {ex.Message}");
            }
        }

        void SearchAllMatchIndex()
        {
            matchIndices.Clear();

            if (string.IsNullOrEmpty(searchWord) || string.IsNullOrEmpty(tableJsonText))
                return;

            // 검색어에 정규식 특수문자($, *, +, ? 등)가 포함되어 있어도 안전하게 일반 문자열로 처리하도록 이스케이프 합니다.
            string escapedWord = Regex.Escape(searchWord);

            // RegexOptions.IgnoreCase를 주어 대소문자 구분 없이 매칭합니다.
            MatchCollection matches = Regex.Matches(tableJsonText, escapedWord, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
                matchIndices.Add(match.Index);
        }

        private void FocusMatchWord(bool previousMatch = false)
        {
            if (searchMatchWord == false)
            {
                SearchAllMatchIndex();
                searchMatchWord = true;
            }

            if (matchIndices.Count <= 0)
                return;

            matchIndexCursor = Mathf.Clamp(matchIndexCursor, 0, matchIndices.Count - 1);
            int index = matchIndices[matchIndexCursor];

            textField.Focus();
            textField.SelectRange(index, index + searchWord.Length);

            // 시작지점부터 해당 단어 인덱스까지 문자열을 잘라 줄바꿈(\n)이 몇 개 포함되었는지 카운트합니다.
            string textUpToWord = tableJsonText.Substring(0, index);
            int lineIndex = textUpToWord.Split('\n').Length - 1; // 0부터 시작하는 라인 번호

            if (totalLineCount > 1)
            {
                float maxScrollValue = textFieldScroller.highValue;

                // (해당 라인 위치 / 전체 라인 수) 비율로 정밀한 스크롤 타겟 값을 계산합니다.
                float targetScrollValue = ((float)lineIndex / (totalLineCount - 1)) * maxScrollValue;

                // 계산된 픽셀 좌표값으로 스크롤바 강제 가동 및 백업 변수 갱신
                textFieldScroller.value = targetScrollValue;
                savedScrollOffset = targetScrollValue;
            }

            if (previousMatch)
            {
                if (--matchIndexCursor < 0)
                    matchIndexCursor = matchIndices.Count - 1;
            }
            else
            {
                if (++matchIndexCursor >= matchIndices.Count)
                    matchIndexCursor = 0;
            }
        }

        private void OnTextFieldMouseDown(MouseDownEvent evt)
        {
            savedScrollOffset = textFieldScroller.value;
        }

        private void OnTextFieldFocusIn(FocusInEvent evt)
        {
            textFieldScroller.value = savedScrollOffset;
        }
    }
}