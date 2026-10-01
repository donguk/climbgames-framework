using UnityEditor;
using UnityEngine;

public class JsonSearchEditorWindow22 : EditorWindow
{
    private string jsonText = "{\n  \"name\": \"Unity\",\n  \"type\": \"Engine\",\n  \"version\": \"2023.2\",\n  \"features\": [\n    \"UI Toolkit\",\n    \"IMGUI\",\n    \"Graphics\"\n  ],\n  \"description\": \"Search and scroll feature in IMGUI\"\n}";

    private string searchWord = "";
    private Vector2 scrollPosition;

    // Splitter 상태 변수
    private float splitterPos = 200f;
    private bool isResizing = false;

    // 텍스트 필드 제어용
    private const string TextAreaControlName = "JsonTextArea";

    [MenuItem("Tools/JSON Search Window (IMGUI)22")]
    public static void ShowWindow()
    {
        GetWindow<JsonSearchEditorWindow22>("JSON Search");
    }

    bool needsSelectUpdate, isTextChanged;
    private void OnGUI()
    {
        // 1. Splitter 영역 계산
        Rect leftRect = new Rect(0, 0, splitterPos, position.height);
        Rect splitterRect = new Rect(splitterPos, 0, 5f, position.height);
        Rect rightRect = new Rect(splitterPos + 5f, 0, position.width - splitterPos - 5f, position.height);

        // --------------------------------------------------
        // [좌측 영역] - 검색 제어 패널
        // --------------------------------------------------
        GUILayout.BeginArea(leftRect);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Search Panel", EditorStyles.boldLabel);

        searchWord = EditorGUILayout.TextField("Search Word", searchWord);

        if (GUILayout.Button("Find & Scroll", GUILayout.Height(30)))
        {
            SearchAndScrollToLine();
        }
        GUILayout.EndArea();

        // --------------------------------------------------
        // [중앙 Splitter 바]
        // --------------------------------------------------
        EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);
        HandleSplitter(splitterRect);
        EditorGUI.DrawRect(splitterRect, Color.gray);

        // --------------------------------------------------
        // [우측 영역] - JSON Text & ScrollView
        // --------------------------------------------------
        GUILayout.BeginArea(rightRect);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUI.SetNextControlName(TextAreaControlName);
        EditorGUI.BeginChangeCheck();
        jsonText = GUILayout.TextArea(jsonText, EditorStyles.textArea, GUILayout.ExpandHeight(true));
        if (EditorGUI.EndChangeCheck())
        {
            isTextChanged = true;
        }
        Event e = Event.current;
        if (e != null && e.isKey && e.type == EventType.KeyUp && e.keyCode == KeyCode.LeftAlt)
        {
            GUI.FocusControl(TextAreaControlName);
            needsSelectUpdate = true;
            Repaint();

        }

        if (needsSelectUpdate && Event.current.type == EventType.Repaint)
        {
            SearchAndScrollToLine();
            needsSelectUpdate = false;
        }


        EditorGUILayout.EndScrollView();
        // 3. 사용자가 타이핑 중(현재 TextArea에 포커스가 가 있을 때) 실시간 커서 추적
        if (isTextChanged && GUI.GetNameOfFocusedControl() == TextAreaControlName && Event.current.type == EventType.Repaint)
        {
            // 현재 활성화된 텍스트 필드의 상태 객체(TextEditor)를 획득
            TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

            if (te != null)
            {
                // 유니티가 그리는 실시간 커서의 Y 좌표 (엔터로 라인이 생성되어도 실시간 반영됨)
                float cursorY = te.graphicalCursorPos.y;

                // 스크롤 뷰가 보여주는 뷰포트 영역의 높이 (BeginScrollView에서 지정한 Height)
                float viewportHeight = rightRect.height;

                // 폰트 크기 및 줄 바꿈 여유 공간 패딩 (한 줄 정도 미리 스크롤되도록 보정)
                float linePadding = 0f;

                // [중요] 커서의 Y 위치가 현재 스크롤 가시 영역의 아래쪽 끝을 벗어났다면?
                if (cursorY > scrollPosition.y + viewportHeight - linePadding)
                {
                    // 커서가 화면 최하단에 걸치도록 스크롤 아래로 이동
                    scrollPosition.y = cursorY - viewportHeight + linePadding;

                    // 스크롤 변경 사양을 윈도우에 즉각 반영
                    Repaint();
                }
                // (선택 사항) 방향키로 위로 올렸을 때 위쪽으로 스크롤이 안 올라간다면 아래 조건도 추가 가능
                else if (cursorY < scrollPosition.y + 40f)
                {
                    scrollPosition.y = cursorY - 40f;
                    if (scrollPosition.y < 0) scrollPosition.y = 0;
                    Repaint();
                }
            }
            isTextChanged = false;
        }


        GUILayout.EndArea();
    }

    /// <summary>
    /// 검색어 탐색, 스크롤 이동 및 TextEditor Selection 지정
    /// </summary>
    private void SearchAndScrollToLine()
    {
        if (string.IsNullOrEmpty(searchWord) || string.IsNullOrEmpty(jsonText))
            return;

        string[] lines = jsonText.Split('\n');
        int targetLineIndex = -1;
        int targetCharStart = 0;
        int currentLength = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            int foundInLineIndex = lines[i].IndexOf(searchWord, System.StringComparison.OrdinalIgnoreCase);
            if (foundInLineIndex >= 0)
            {
                targetLineIndex = i;
                targetCharStart = currentLength + foundInLineIndex;
                break;
            }
            // 줄바꿈 문자를 고려해 +1
            currentLength += lines[i].Length + 1;
        }

        if (targetLineIndex >= 0)
        {
            // 1. 스크롤 이동 위치 계산 (대략적인 1줄 높이: 13~15px 기준)
            float lineHeight = EditorStyles.textArea.lineHeight > 0
                ? EditorStyles.textArea.lineHeight
                : 14f;

            scrollPosition.y = targetLineIndex * lineHeight;

            // 2. 텍스트 커서 포커스 및 선택 영역 지정
            GUI.FocusControl(TextAreaControlName);
            TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

            if (textEditor != null)
            {
                textEditor.cursorIndex = targetCharStart;
                textEditor.selectIndex = targetCharStart + searchWord.Length;
            }

            Repaint();
        }
        else
        {
            Debug.LogWarning($"'${searchWord}' 단어를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 좌우 분할 스플리터 드래그 처리
    /// </summary>
    private void HandleSplitter(Rect splitterRect)
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && splitterRect.Contains(e.mousePosition))
        {
            isResizing = true;
        }

        if (isResizing)
        {
            splitterPos = e.mousePosition.x;
            splitterPos = Mathf.Clamp(splitterPos, 100f, position.width - 100f);
            Repaint();
        }

        if (e.type == EventType.MouseUp)
        {
            isResizing = false;
        }
    }
}