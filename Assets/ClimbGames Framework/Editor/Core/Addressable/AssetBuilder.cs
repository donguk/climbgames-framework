using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;

namespace ClimbGames.Editor
{
    public static class AssetBuilder
    {
        public static string BuildRoot => $"Build/{Builder.TargetPlatform}/Addressables";

        public static string GetRemoteBuildPath()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                return string.Empty;

            string profileId = settings.activeProfileId;

            // 1. 현재 활성화된 프로필에서 'Remote.BuildPath'의 원본 템플릿 문자열 가져오기 (예: "ServerData/[BuildTarget]")
            string rawPath = settings.profileSettings.GetValueByName(profileId, "Remote.BuildPath");

            // 2. 프로필 변수([BuildTarget] 등)가 모두 변환된 실제 경로 가져오기 (예: "ServerData/Android")
            string evaluatedPath = settings.profileSettings.EvaluateString(profileId, rawPath);

            return evaluatedPath;
        }

        public static void BuildAssetBundle()
        {
            //var settings = AddressableAssetSettingsDefaultObject.Settings;
        }

        public static void BuildNewContent(string bundleVersion, int buildNumber)
        {
            // Setup BuildOptions
            Builder.Setup(bundleVersion, buildNumber);

            // 기존 파일 삭제
            Builder.DeleteFiles(GetRemoteBuildPath());

            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            if (string.IsNullOrEmpty(result.Error))
            {
                BackupContentState(result.ContentStateFilePath);
                BackupEditorState();
                BackupServerData();

                Debug.Log($"[AssetBuilder] Success BuildNewContent");
            }
            else
            {
                Debug.LogError($"[AssetBuilder] Fail BuildNewContent: {result?.Error}");
            }
        }

        public static void BuildContentUpdate(string contentStateFilePath, string bundleVersion, int buildNumber)
        {
            // Setup BuildOptions
            Builder.Setup(bundleVersion, buildNumber);

            // 기존 파일 삭제
            Builder.DeleteFiles(GetRemoteBuildPath());

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressablesPlayerBuildResult result = ContentUpdateScript.BuildContentUpdate(settings, contentStateFilePath);
            if (result != null && string.IsNullOrEmpty(result.Error))
            {
                BackupEditorState();
                BackupServerData();

                Debug.Log("[AssetBuilder] Success BuildContentUpdate");
            }
            else
            {
                Debug.LogError($"[AssetBuilder] Fail BuildContentUpdate: {result?.Error}");
            }
        }

        public static void BackupContentState(string contentStateFilePath)
        {
            string folderPath = Path.Combine(BuildRoot, "ContentState", $"{Builder.BundleVersion}");
            Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, "addressables_content_state.bin");
            if (File.Exists(filePath))
                File.Delete(filePath);

            File.Copy(contentStateFilePath, Path.Combine(folderPath, "addressables_content_state.bin"));
        }

        public static void BackupEditorState()
        {
            // 압축 대상 소스 폴더: Library/com.unity.addressables/aa/{TargetPlatform}
            string sourcePath = Path.Combine("Library", "com.unity.addressables", "aa", Builder.TargetPlatform);

            if (Directory.Exists(sourcePath) == false)
                return;

            // 백업 폴더 및 Zip 파일 경로 설정: AddressablesState/EditorState_0.1.0_1.zip
            string folderPath = Path.Combine(BuildRoot, "ContentState", $"{Builder.BundleVersion}");
            Directory.CreateDirectory(folderPath);

            string zipFileName = $"EditorState_{Builder.BuildNumber}.zip";
            string destinationZipPath = Path.Combine(folderPath, zipFileName);

            try
            {
                // 기존에 동일한 이름의 zip 파일이 있다면 삭제
                if (File.Exists(destinationZipPath))
                    File.Delete(destinationZipPath);

                // 폴더 통째로 Zip 압축 (CompressionLevel.Optimal: 기본 최적 압축)
                ZipFile.CreateFromDirectory(sourcePath, destinationZipPath, CompressionLevel.Optimal, includeBaseDirectory: false);
                Debug.Log($"[AssetBuilder] Zip Library: {destinationZipPath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AssetBuilder] Fail Zip Library: {ex.Message}");
            }
        }

        public static void BackupServerData()
        {
            var destinationPath = Path.Combine(BuildRoot, $"ServerData/{Builder.BundleVersion}_{Builder.BuildNumber}");

            if (Directory.Exists(destinationPath))
                Builder.DeleteFiles(destinationPath);

            Builder.CopyDirectory(GetRemoteBuildPath(), destinationPath);
        }
    }
}