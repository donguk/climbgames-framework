using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ClimbGames.Editor
{
    public static class ConsoleRedirector
    {
        // 커스텀 Debug 클래스의 파일 이름을 지정합니다.
        private const string DebugClassName = "Debug.cs";

        [OnOpenAsset(0)]
#if UNITY_6000_0_OR_NEWER
        public static bool OnOpenAsset(EntityId entityId, int line)
        {
            string assetPath = AssetDatabase.GetAssetPath(entityId);
            return OpenAsset(assetPath);
        }
#else
        public static bool OnOpenAsset(int instanceID, int line)
        {
            string assetPath = AssetDatabase.GetAssetPath(instanceID);
            return OpenAsset(assetPath);
        }
#endif

        static bool OpenAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".cs"))
                return false;

            // 2. 만약 열려고 하는 파일이 우리가 만든 Debug.cs라면 리다이렉션 로직 시작
            if (assetPath.EndsWith(DebugClassName))
            {
                string stackTrace = GetStackTrace();
                if (!string.IsNullOrEmpty(stackTrace))
                {
                    // 정규식: (at 경로:라인번호) 형태 추출
                    var match = Regex.Match(stackTrace, @"\(at (.+):(\d+)\)");

                    while (match.Success)
                    {
                        string path = match.Groups[1].Value;
                        int lineNum = int.Parse(match.Groups[2].Value);
                        // Debug.cs가 아닌 실제 호출부를 찾을 때까지 탐색
                        if (!path.EndsWith(DebugClassName))
                        {
                            var targetAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                            if (targetAsset != null)
                            {
                                AssetDatabase.OpenAsset(targetAsset, lineNum);
                                return true; // 성공적으로 진짜 코드를 열었음
                            }
                        }
                        match = match.NextMatch();
                    }
                }
            }

            return false; // 기본 유니티 동작(Debug.cs 열기 등) 수행
        }

        private static string GetStackTrace()
        {
            var consoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
            var fieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            var consoleWindowInstance = fieldInfo.GetValue(null);

            if (consoleWindowInstance != null)
            {
                // 현재 포커스된 윈도우가 콘솔일 때만 텍스트를 읽어옵니다.
                if ((EditorWindow)consoleWindowInstance == EditorWindow.focusedWindow)
                {
                    var activeTextInfo = consoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
                    return (string)activeTextInfo.GetValue(consoleWindowInstance);
                }
            }
            return null;
        }
    }
}