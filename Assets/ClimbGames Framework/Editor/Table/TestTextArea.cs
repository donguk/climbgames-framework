using UnityEditor;
using UnityEngine;

public class JsonSearchEditorWindow : EditorWindow
{
    private string jsonText = "{\n  \"name\": \"Unity\",\n  \"type\": \"Engine\",\n  \"version\": \"2023.2\",\n  \"features\": [\n    \"UI Toolkit\",\n    \"IMGUI\",\n    \"Graphics\"\n  ],\n  \"description\": \"Search and scroll feature in IMGUI ControlID\"\n}";

    private string searchWord = "";
    private Vector2 scrollPosition;

    private float splitterPos = 200f;
    private bool isResizing = false;

    // ControlID 및 선택 제어 변수
    private int textAreaControlID;
    private const string TextAreaName = "CustomJsonTextArea";

    private bool needsSelectUpdate = false;
    private int targetStart = 0;
    private int targetEnd = 0;

    [MenuItem("Tools/JSON Search Window (ControlID)")]
    public static void ShowWindow()
    {
        GetWindow<JsonSearchEditorWindow>("JSON Search (ControlID)");
    }

    private void OnGUI()
    {
        Rect leftRect = new Rect(0, 0, splitterPos, position.height);
        Rect splitterRect = new Rect(splitterPos, 0, 5f, position.height);
        Rect rightRect = new Rect(splitterPos + 5f, 0, position.width - splitterPos - 5f, position.height);

        // --------------------------------------------------
        // [좌측 검색 패널]
        // --------------------------------------------------
        GUILayout.BeginArea(leftRect);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Search Panel", EditorStyles.boldLabel);

        searchWord = EditorGUILayout.TextField("Search Word", searchWord);

        if (GUILayout.Button("Find & Scroll", GUILayout.Height(30)))
        {
            SearchAndSelectText();
        }
        GUILayout.EndArea();

        // --------------------------------------------------
        // [중앙 Splitter]
        // --------------------------------------------------
        EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);
        HandleSplitter(splitterRect);
        EditorGUI.DrawRect(splitterRect, Color.gray);

        // --------------------------------------------------
        // [우측 TextArea 영역]
        // --------------------------------------------------
        GUILayout.BeginArea(rightRect);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // 1. 키보드 포커스용 ControlID 획득
        // (TextArea가 배치될 영역의 컨트롤 ID를 미리 가져옵니다)
        textAreaControlID = GUIUtility.GetControlID(FocusType.Keyboard);

        // 2. ControlID 이름을 매핑하여 FocusControl 대응
        GUI.SetNextControlName(TextAreaName);

        // 3. 텍스트 영역 높이 계산 (긴 텍스트 대응)
        GUIStyle textAreaStyle = EditorStyles.textArea;
        float textHeight = textAreaStyle.CalcHeight(new GUIContent(jsonText), rightRect.width - 20f);
        textHeight = Mathf.Max(textHeight, rightRect.height - 20f);

        Rect textAreaRect = GUILayoutUtility.GetRect(
            rightRect.width - 20f,
            textHeight,
            GUILayout.ExpandWidth(true),
            GUILayout.ExpandHeight(true)
        );

        // 4. GUI.TextArea 사용 (ControlID를 명시적으로 전달)
        jsonText = GUI.TextArea(textAreaRect, jsonText, textAreaStyle);

        Event e = Event.current;
        if (e != null && e.isKey && e.type == EventType.KeyUp && e.keyCode == KeyCode.LeftAlt)
        {

            //TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            //
            //if (textEditor != null)
            //{
            //    Debug.Log($"SelectedText:   {textEditor.SelectedText}");
            //}

            //SearchAndSelectText();
            GUI.FocusControl(TextAreaName);
            needsSelectUpdate = true;
            Repaint();

        }

        if (needsSelectUpdate && Event.current.type == EventType.Repaint)
        {
            SearchAndSelectText();
        }


        EditorGUILayout.EndScrollView();




        GUILayout.EndArea();
    }

    private void SearchAndSelectText()
    {
        if (string.IsNullOrEmpty(searchWord) || string.IsNullOrEmpty(jsonText))
            return;

        int foundIndex = jsonText.IndexOf(searchWord, System.StringComparison.OrdinalIgnoreCase);

        if (foundIndex >= 0)
        {
            // 1. 포커스 지정
            //GUI.FocusControl(TextAreaName);
            //GUIUtility.keyboardControl = textAreaControlID;

            // 2. 스크롤 위치 계산
            //string textBeforeFound = jsonText.Substring(0, foundIndex);
            //int lineBreakCount = textBeforeFound.Split('\n').Length - 1;
            //float lineHeight = EditorStyles.textArea.lineHeight > 0 ? EditorStyles.textArea.lineHeight : 13f;
            //
            //scrollPosition.y = lineBreakCount * lineHeight;

            // 3. 선택 영역 타겟 설정
            targetStart = foundIndex;
            targetEnd = foundIndex + searchWord.Length;
            needsSelectUpdate = true;


            TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

            if (textEditor != null)
            {
                textEditor.OnFocus(); // 포커스 활성화 처리
                textEditor.cursorIndex = targetEnd;
                textEditor.selectIndex = targetStart;

                needsSelectUpdate = false;
                Debug.Log($"SelectedText:   {textEditor.SelectedText}");


                Vector2 cursorPos = textEditor.graphicalCursorPos;
                // 스크롤 뷰의 y축을 커서 위치로 바로 매칭
                // 화면 맨 위에 딱 붙는 게 싫다면 화면 높이의 절반 정도를 빼주면 중앙에 위치합니다.
                scrollPosition.y = cursorPos.y - 40f;
            }

            Repaint();
        }
        else
        {
            Debug.LogWarning($"'{searchWord}' 단어를 찾을 수 없습니다.");
        }
    }

    private void ApplyTextSelection()
    {
        // keyboardControl ID를 사용해 정확한 TextEditor 상태 객체를 추출
        int activeID = GUIUtility.keyboardControl;
        if (activeID != 0)
        {
            TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), activeID);

            if (textEditor != null)
            {
                textEditor.OnFocus(); // 포커스 활성화 처리
                textEditor.cursorIndex = targetEnd;
                textEditor.selectIndex = targetStart;

                needsSelectUpdate = false;
            }
        }
    }

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