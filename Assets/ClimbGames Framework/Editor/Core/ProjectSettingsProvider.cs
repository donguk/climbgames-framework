using UnityEditor;
using UnityEngine;

namespace ClimbGames.Editor
{
    public static class ProjectSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateClimbGamesProvider()
        {
            var provider = new SettingsProvider("Project/ClimbGames", SettingsScope.Project)
            {
                label = "ClimbGames",
                guiHandler = (searchContext) =>
                {
                    EditorGUILayout.Space(10);
                    //EditorGUILayout.LabelField("ClimbGames Framework", EditorStyles.boldLabel);
                    EditorGUILayout.HelpBox(
                        "ClimbGames Framework Setup Pages",
                        MessageType.Info
                    );
                },

                keywords = new[] { "ClimbGames", "Framework" }
            };

            return provider;
        }

        private static UnityEditor.Editor cachedEditorSettings;
        private static UnityEditor.Editor cachedRuntimeSettings;

        [SettingsProvider]
        public static SettingsProvider CreateFrameworkSettingsProvider()
        {
            // SettingsProvider("카테고리 경로", 검색/표시 범위)
            var provider = new SettingsProvider("Project/ClimbGames/Framework Settings", SettingsScope.Project)
            {
                label = "Settings",
                guiHandler = (searchContext) =>
                {
                    EditorGUILayout.Space(10);

                    EditorGUILayout.LabelField("Editor Environment", EditorStyles.boldLabel);
                    ScriptableObject settings = FrameworkEditorSettings.instance;
                    if (settings != null)
                    {
                        // 에디터 인스턴스가 없거나 대상이 바뀌었으면 재생성
                        UnityEditor.Editor.CreateCachedEditor(settings, null, ref cachedEditorSettings);
                        // 기본 Inspector 전체를 한 번에 렌더링 (m_Script 자동 처리 및 ApplyModifiedProperties 자동 포함)
                        cachedEditorSettings.OnInspectorGUI();
                    }

                    EditorGUILayout.Space(10);

                    EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);
                    settings = FrameworkSettings.Instance;
                    if (settings != null)
                    {
                        UnityEditor.Editor.CreateCachedEditor(settings, null, ref cachedRuntimeSettings);
                        cachedRuntimeSettings.OnInspectorGUI();
                    }
                },

                // Project Settings 창 검색창 지원 키워드
                keywords = new[] { "ClimbGames", "Framework", "Settings", "Transition", "Namespace" }
            };

            return provider;
        }
    }
}