using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace ClimbGames.Editor
{
    public static partial class ProjectBuilder
    {
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

        public static void BuildPlayerContent()
        {
            var remoteBuildPath = RemoteBuildPath;
            if (Directory.Exists(remoteBuildPath))
                Directory.Delete(remoteBuildPath, true);

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            string rawPath = settings.profileSettings.GetValueByName(settings.activeProfileId, "Remote.LoadPath");

            string newPath = $"{BuildSettings.PatchUrl}/{BuildSettings.TargetPlatform}/{BuildSettings.BundleVersion}";
            settings.profileSettings.SetValue(settings.activeProfileId, "Remote.LoadPath", newPath);

            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            if (string.IsNullOrEmpty(result.Error))
            {
                CopyContentState(result.ContentStateFilePath);
                SaveEditorEnv();
                CopyServerData();

                Debug.Log($"[ProjectBuilder] Success BuildPlayerContent");
            }
            else
            {
                Debug.LogError($"[ProjectBuilder] Fail BuildPlayerContent: {result?.Error}");
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

            string newPath = $"{BuildSettings.PatchUrl}/{BuildSettings.TargetPlatform}/{BuildSettings.BundleVersion}";
            settings.profileSettings.SetValue(settings.activeProfileId, "Remote.LoadPath", newPath);

            AddressablesPlayerBuildResult result = ContentUpdateScript.BuildContentUpdate(settings, contentStateFilePath);
            if (result != null && string.IsNullOrEmpty(result.Error))
            {
                SaveEditorEnv();
                CopyServerData();

                Debug.Log("[ProjectBuilder] Success BuildContentUpdate");
            }
            else
            {
                Debug.LogError($"[ProjectBuilder] Fail BuildContentUpdate: {result?.Error}");
            }

            settings.profileSettings.SetValue(settings.activeProfileId, "Remote.LoadPath", rawPath);
        }

        static void CopyContentState(string contentStateFilePath)
        {
            string destinationPath = Path.Combine(BuildSettings.AddressablesPath, $"ContentState/{BuildSettings.BundleVersion}");
            Directory.CreateDirectory(destinationPath);

            File.Copy(contentStateFilePath, Path.Combine(destinationPath, "addressables_content_state.bin"), true);
        }

        static void SaveEditorEnv()
        {
            // 압축 대상 소스 폴더: Library/com.unity.addressables/aa/{TargetPlatform}
            string libraryPath = Path.Combine("Library", "com.unity.addressables", "aa", BuildSettings.TargetPlatform);
            if (Directory.Exists(libraryPath) == false)
                return;

            // 백업 폴더 및 Zip 파일 경로 설정: AddressablesState/EditorEnv_0.1.0_1.zip
            string destinationPath = Path.Combine(BuildSettings.AddressablesPath, $"ContentState/{BuildSettings.BundleVersion}");
            Directory.CreateDirectory(destinationPath);

            string zipFileName = $"EditorEnv_{BuildSettings.BundleVersion}_{BuildSettings.BuildNumber}.zip";
            string destinationZipPath = Path.Combine(destinationPath, zipFileName);

            try
            {
                // 기존에 동일한 이름의 zip 파일이 있다면 삭제
                if (File.Exists(destinationZipPath))
                    File.Delete(destinationZipPath);

                // 폴더 통째로 Zip 압축 (CompressionLevel.Optimal: 기본 최적 압축)
                ZipFile.CreateFromDirectory(libraryPath, destinationZipPath, CompressionLevel.Optimal, includeBaseDirectory: false);
                Debug.Log($"[ProjectBuilder] Zip Library: {destinationZipPath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ProjectBuilder] Fail Zip Library: {ex.Message}");
            }
        }

        static void CopyServerData()
        {
            var remoteBuildPath = RemoteBuildPath;
            if (Directory.Exists(remoteBuildPath) == false)
                return;

            var destinationPath = Path.Combine(BuildSettings.AddressablesPath, $"ServerData/{BuildSettings.BundleVersion}");
            Directory.CreateDirectory(destinationPath);

            string[] targetExtensions = { ".json", ".bin", ".hash" };
            DirectoryInfo directoryInfo = new DirectoryInfo(remoteBuildPath);

            var buildFiles = directoryInfo.GetFiles();
            foreach (FileInfo file in buildFiles)
            {
                string fileName = file.Name;
                if (file.Name.StartsWith("catalog") && targetExtensions.Contains(file.Extension))
                    fileName = $"catalog_{BuildSettings.BundleVersion}_{BuildSettings.BuildNumber}{file.Extension}";

                string filePath = Path.Combine(destinationPath, fileName);
                file.CopyTo(filePath, overwrite: true);
            }
        }

        public static async UniTask UploadToHfs(IProgress<FileUploadInfo> progress = null)
        {
            string uploadUrl = $"{BuildSettings.PatchUrl}/{BuildSettings.TargetPlatform}";

            string serverDataPath = Path.Combine(BuildSettings.AddressablesPath, $"ServerData/{BuildSettings.BundleVersion}");
            if (Directory.Exists(serverDataPath) == false)
                return;

            DirectoryInfo directoryInfo = new DirectoryInfo(serverDataPath);
            var files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);

            // 폴더 생성
            var directories = files.Select(f => f.DirectoryName.Replace("\\", "/")).ToList();
            HashSet<string> folderHash = new HashSet<string>();
            foreach (var path in directories)
            {
                int index = path.LastIndexOf($"{BuildSettings.BundleVersion}");
                if (index > -1)
                    folderHash.Add(path.Substring(index));
            }
            foreach (var folder in folderHash)
            {
                // HTTP MKCOL 메서드로 폴더 생성 요청
                string folderUrl = $"{uploadUrl}/{folder}";
                using (UnityWebRequest www = new UnityWebRequest(folderUrl, "MKCOL"))
                {
                    try
                    {
                        string auth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"climbgames-admin:climbgames2@"));
                        www.SetRequestHeader("Authorization", "Basic " + auth);
                        www.downloadHandler = new DownloadHandlerBuffer();
                        await www.SendWebRequest();
                    }
                    catch (Exception e)
                    {
                        // 405: 폴더 이미 존재
                        if (www.responseCode != 405)
                        {
                            UnityEngine.Debug.LogException(e);
                            return;
                        }
                    }
                }
            }

            FileUploadInfo uploadInfo = new FileUploadInfo()
            {
                totalCount = files.Length
            };
            foreach (var file in files)
            {
                uploadInfo.fileName = file.Name;

                string folderPath = file.DirectoryName.Replace("\\", "/");
                int index = folderPath.LastIndexOf($"{BuildSettings.BundleVersion}");
                string destinationUrl = $"{uploadUrl}/{folderPath.Substring(index)}";

                var fileData = await File.ReadAllBytesAsync(file.FullName);
                var formData = new List<IMultipartFormSection>
                {
                    new MultipartFormFileSection("file", fileData, file.Name, "application/octet-stream")
                };
                using (UnityWebRequest www = UnityWebRequest.Post(destinationUrl, formData))
                {
                    string auth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("climbgames-admin:climbgames2@"));
                    www.SetRequestHeader("Authorization", "Basic " + auth);

                    var operation = www.SendWebRequest();
                    while (operation.isDone == false)
                    {
                        uploadInfo.progress = operation.progress;
                        progress?.Report(uploadInfo);
                        await UniTask.Yield(); // 다음 프레임 대기
                    }

                    if (www.result != UnityWebRequest.Result.Success)
                        Debug.LogError($"[UploadToHfs] Fail File: {file.Name} | Error: {www.error} | Response Code: {www.responseCode}");
                }
                uploadInfo.currentIndex++;
            }
        }
    }

    public struct FileUploadInfo
    {
        public int totalCount;
        public int currentIndex;
        public string fileName;
        public float progress;
    }
}