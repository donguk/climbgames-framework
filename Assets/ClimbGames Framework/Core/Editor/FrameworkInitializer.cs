using System.IO;
using UnityEditor;
using UnityEngine;

namespace ClimbGames.Core.Editor
{
    [InitializeOnLoad]
    public static class FrameworkInitializer
    {
        private const string FolderPath = "Assets/ClimbGames/Resources";
        private const string AssetPath = "Assets/ClimbGames/Resources/FrameworkSettings.asset";

        static FrameworkInitializer()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                CheckAndCreateSettings();
        }

        private static void CheckAndCreateSettings()
        {
            // 1. 이미 파일이 존재하면 아무것도 하지 않음
            if (File.Exists(Path.Combine(Application.dataPath, "..", AssetPath)))
                return;

            // 2. 폴더 생성 (Assets/ClimbGames)
            if (AssetDatabase.IsValidFolder(FolderPath) == false)
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "ClimbGames/Resources"));
                AssetDatabase.Refresh();
            }

            // 3. 에셋 생성
            FrameworkSettings newSettings = ScriptableObject.CreateInstance<FrameworkSettings>();

            // 주의: 반드시 AssetDatabase를 통해 생성해야 유니티가 메타파일을 관리합니다.
            AssetDatabase.CreateAsset(newSettings, AssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}