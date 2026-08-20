using System.IO;
using UnityEditor;
using UnityEngine;

namespace ClimbGames
{
    public static class Builder
    {
        public static string TargetPlatform => EditorUserBuildSettings.activeBuildTarget.ToString();
        public static BuildTargetGroup TargetGroup => BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        public static string BundleVersion => PlayerSettings.bundleVersion;
        public static int BuildNumber { get; private set; }

        public static void Setup(string bundleVersion, int buildNumber)
        {
            PlayerSettings.bundleVersion = bundleVersion;
            BuildNumber = buildNumber;
        }

        public static void CopyDirectory(string sourcePath, string targetPath, bool includeSubDirectory = true)
        {
            if (Directory.Exists(sourcePath) == false)
                return;

            Directory.CreateDirectory(targetPath);

            DirectoryInfo directoryInfo = new DirectoryInfo(sourcePath);
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                string targetFilePath = Path.Combine(targetPath, file.Name);
                file.CopyTo(targetFilePath, overwrite: true);
            }

            // 3. 하위 폴더 재귀 복사
            if (includeSubDirectory)
            {
                foreach (DirectoryInfo subDir in directoryInfo.GetDirectories())
                {
                    string targetSubDir = Path.Combine(targetPath, subDir.Name);
                    CopyDirectory(subDir.FullName, targetSubDir, includeSubDirectory);
                }
            }
        }

        public static void DeleteFiles(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
                foreach (FileInfo file in dirInfo.GetFiles())
                    file.Delete();
            }
        }
    }
}