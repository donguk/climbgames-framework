using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;

namespace ClimbGames.Editor
{
    public static class AssetBuilder
    {
        public static string BuildRoot => $"Build/{BuildSettings.TargetPlatform}/Addressables";
        public static string RemoteBuildPath
        {
            get
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                if (settings == null)
                    return string.Empty;

                string profileId = settings.activeProfileId;
                string rawPath = settings.profileSettings.GetValueByName(profileId, "Remote.BuildPath");
                string evaluatedPath = settings.profileSettings.EvaluateString(profileId, rawPath);

                return evaluatedPath;
            }
        }

        public static void BuildAssetBundle()
        {
            //var settings = AddressableAssetSettingsDefaultObject.Settings;
        }

        public static void BuildPlayerContent()
        {
            var remoteBuildPath = RemoteBuildPath;
            if (Directory.Exists(remoteBuildPath))
                Directory.Delete(remoteBuildPath, true);

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            string rawPath = settings.profileSettings.GetValueByName(settings.activeProfileId, "Remote.LoadPath");

            string newPath = $"{BuildSettings.PatchUrl}/{BuildSettings.TargetPlatform}/{BuildSettings.bundleVersion}";
            settings.profileSettings.SetValue(settings.activeProfileId, "Remote.LoadPath", newPath);

            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            if (string.IsNullOrEmpty(result.Error))
            {
                CopyContentState(result.ContentStateFilePath);
                SaveEditorEnv();
                CopyServerData();

                Debug.Log($"[AssetBuilder] Success BuildPlayerContent");
            }
            else
            {
                Debug.LogError($"[AssetBuilder] Fail BuildPlayerContent: {result?.Error}");
            }

            settings.profileSettings.SetValue(settings.activeProfileId, "Remote.LoadPath", rawPath);
        }

        public static void BuildContentUpdate(string contentStateFilePath)
        {
            var remoteBuildPath = RemoteBuildPath;
            if (Directory.Exists(remoteBuildPath))
                Directory.Delete(remoteBuildPath, true);

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            string rawPath = settings.profileSettings.GetValueByName(settings.activeProfileId, "Remote.LoadPath");

            string newPath = $"{BuildSettings.PatchUrl}/{BuildSettings.TargetPlatform}/{BuildSettings.bundleVersion}";
            settings.profileSettings.SetValue(settings.activeProfileId, "Remote.LoadPath", newPath);

            AddressablesPlayerBuildResult result = ContentUpdateScript.BuildContentUpdate(settings, contentStateFilePath);
            if (result != null && string.IsNullOrEmpty(result.Error))
            {
                SaveEditorEnv();
                CopyServerData();

                Debug.Log("[AssetBuilder] Success BuildContentUpdate");
            }
            else
            {
                Debug.LogError($"[AssetBuilder] Fail BuildContentUpdate: {result?.Error}");
            }

            settings.profileSettings.SetValue(settings.activeProfileId, "Remote.LoadPath", rawPath);
        }

        public static void CopyContentState(string contentStateFilePath)
        {
            string destinationPath = Path.Combine(BuildRoot, "ContentState", $"{BuildSettings.bundleVersion}");
            Directory.CreateDirectory(destinationPath);

            File.Copy(contentStateFilePath, Path.Combine(destinationPath, "addressables_content_state.bin"), true);
        }

        public static void SaveEditorEnv()
        {
            // 압축 대상 소스 폴더: Library/com.unity.addressables/aa/{TargetPlatform}
            string libraryPath = Path.Combine("Library", "com.unity.addressables", "aa", BuildSettings.TargetPlatform);
            if (Directory.Exists(libraryPath) == false)
                return;

            // 백업 폴더 및 Zip 파일 경로 설정: AddressablesState/EditorEnv_0.1.0_1.zip
            string destinationPath = Path.Combine(BuildRoot, "ContentState", $"{BuildSettings.bundleVersion}");
            Directory.CreateDirectory(destinationPath);

            string zipFileName = $"EditorEnv_{BuildSettings.bundleVersion}_{BuildSettings.buildNumber}.zip";
            string destinationZipPath = Path.Combine(destinationPath, zipFileName);

            try
            {
                // 기존에 동일한 이름의 zip 파일이 있다면 삭제
                if (File.Exists(destinationZipPath))
                    File.Delete(destinationZipPath);

                // 폴더 통째로 Zip 압축 (CompressionLevel.Optimal: 기본 최적 압축)
                ZipFile.CreateFromDirectory(libraryPath, destinationZipPath, CompressionLevel.Optimal, includeBaseDirectory: false);
                Debug.Log($"[AssetBuilder] Zip Library: {destinationZipPath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AssetBuilder] Fail Zip Library: {ex.Message}");
            }
        }

        public static void CopyServerData()
        {
            var remoteBuildPath = RemoteBuildPath;
            if (Directory.Exists(remoteBuildPath) == false)
                return;

            var destinationPath = Path.Combine(BuildRoot, $"ServerData/{BuildSettings.bundleVersion}");
            Directory.CreateDirectory(destinationPath);

            string[] targetExtensions = { ".json", ".bin", ".hash" };
            DirectoryInfo directoryInfo = new DirectoryInfo(remoteBuildPath);

            var buildFiles = directoryInfo.GetFiles();
            foreach (FileInfo file in buildFiles)
            {
                string fileName = file.Name;
                if (file.Name.StartsWith("catalog") && targetExtensions.Contains(file.Extension))
                    fileName = $"catalog_{BuildSettings.bundleVersion}_{BuildSettings.buildNumber}{file.Extension}";

                string filePath = Path.Combine(destinationPath, fileName);
                file.CopyTo(filePath, overwrite: true);
            }
        }
    }
}