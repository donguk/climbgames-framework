using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ClimbGames.Editor
{
    public class TextSearchBar
    {
        private string patternText;
        private string searchText;
        private bool isMatchSearched;
        private List<Match> matches = new List<Match>();
        private int matchCursor;

        public Match Match => matchCursor >= 0 && matchCursor < matches.Count ? matches[matchCursor] : null;
        public event Action<Match> onSearchClicked;
        public string PatternText => patternText;

        public void Draw()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUI.BeginChangeCheck();
            patternText = EditorGUILayout.TextField(patternText, EditorStyles.toolbarSearchField, GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck())
                ResetState();

            if (GUILayout.Button(GUIContents.HoverBar_Down, EditorStyles.toolbarButton, GUILayout.Width(28f)))
                MoveMatchCursor(true);

            if (GUILayout.Button(GUIContents.HoverBar_Up, EditorStyles.toolbarButton, GUILayout.Width(28f)))
                MoveMatchCursor(false);

            EditorGUILayout.EndHorizontal();
        }

        public void SetText(string text)
        {
            searchText = text;
            ResetState();
        }

        void ResetState()
        {
            matchCursor = -1;
            matches.Clear();
            isMatchSearched = false;
        }

        public void SearchAllMatches()
        {
            matches.Clear();

            if (string.IsNullOrEmpty(patternText) || string.IsNullOrEmpty(searchText))
                return;

            // 검색어에 정규식 특수문자($, *, +, ? 등)가 포함되어 있어도 안전하게 일반 문자열로 처리하도록 이스케이프 합니다.
            string escapedWord = Regex.Escape(patternText);

            // RegexOptions.IgnoreCase를 주어 대소문자 구분 없이 매칭합니다.
            MatchCollection collection = Regex.Matches(searchText, escapedWord, RegexOptions.IgnoreCase);

            foreach (Match match in collection)
                matches.Add(match);

            isMatchSearched = true;
        }

        public void MoveMatchCursor(bool down = false)
        {
            if (isMatchSearched == false)
                SearchAllMatches();

            if (down)
            {
                if (++matchCursor >= matches.Count)
                    matchCursor = 0;
            }
            else
            {
                if (--matchCursor < 0)
                    matchCursor = matches.Count - 1;
            }

            onSearchClicked?.Invoke(Match);
        }
    }
}