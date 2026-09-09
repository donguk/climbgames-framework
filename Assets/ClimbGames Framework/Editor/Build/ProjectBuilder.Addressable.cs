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
using UnityEditor;

namespace ClimbGames.Editor
{
    struct AddressableAssetSettingsScope : IDisposable
    {
        private AddressableAssetSettings settings;
        private AddressableAssetSettings.PlayerBuildOption playerBuildOption;
        private string remoteLoadPath;
        private bool buildRemoteCatalog;

        public AddressableAssetSettingsScope(AddressableAssetSettings settings)
        {
            this.settings = settings;

            playerBuildOption = settings.BuildAddressablesWithPlayerBuild;
            remoteLoadPath = settings.profileSettings.GetValueByName(settings.activeProfileId, "Remote.LoadPath");
            buildRemoteCatalog = settings.BuildRemoteCatalog;
        }

        public void Dispose()
        {
            settings.BuildAddressablesWithPlayerBuild = playerBuildOption;
            settings.profileSettings.SetValue(settings.activeProfileId, "Remote.LoadPath", remoteLoadPath);
            settings.BuildRemoteCatalog = buildRemoteCatalog;

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }
    }

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
            settings.profileSettings.SetValue(settings.activeProfileId, "Remote.LoadPath", $"{BuildSettings.PatchUrl}/{BuildSettings.TargetPlatform}/{BuildSettings.BundleVersion}");
            settings.BuildRemoteCatalog = true;
            EditorUtility.SetDirty(settings);

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
        }

        public static void BuildContentUpdate(string contentStateFilePath)
        {
            var remoteBuildPath = RemoteBuildPath;
            if (Directory.Exists(remoteBuildPath))
                Directory.Delete(remoteBuildPath, true);

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            settings.profileSettings.SetValue(settings.activeProfileId, "Remote.LoadPath", $"{BuildSettings.PatchUrl}/{BuildSettings.TargetPlatform}/{BuildSettings.BundleVersion}");
            settings.BuildRemoteCatalog = true;
            EditorUtility.SetDirty(settings);

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
        }

        static void CopyContentState(string contentStateFilePath)
        {
            string destinationPath = Path.Combine(BuildSettings.BuildPath, $"{BuildSettings.BundleVersion}");
            Directory.CreateDirectory(destinationPath);

            File.Copy(contentStateFilePath, Path.Combine(destinationPath, "addressables_content_state.bin"), true);
        }

        static void SaveEditorEnv()
        {
            // 압축 대상 소스 폴더: Library/com.unity.addressables/aa/{TargetPlatform}
            string libraryPath = Path.Combine("Library", "com.unity.addressables", "aa", BuildSettings.TargetPlatform);
            if (Directory.Exists(libraryPath) == false)
                return;

            // 백업 폴더 및 Zip 파일 경로 설정: 0.1.0/EditorEnv_0.1.0_1.zip
            string destinationPath = Path.Combine(BuildSettings.BuildPath, $"{BuildSettings.BundleVersion}");
            Directory.CreateDirectory(destinationPath);

            string zipFileName = $"EditorEnv_{BuildSettings.BuildType}_{BuildSettings.BundleVersion}_{BuildSettings.BuildNumber}.zip";
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

            var destinationPath = Path.Combine(BuildSettings.BuildPath, $"{BuildSettings.BundleVersion}/ServerData");
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
            string serverDataPath = Path.Combine(BuildSettings.BuildPath, $"{BuildSettings.BundleVersion}/ServerData");
            if (Directory.Exists(serverDataPath) == false)
                return;

            var targetFiles = Directory.GetFiles(serverDataPath, "*.*", SearchOption.AllDirectories);
            HashSet<string> targetFolders = new HashSet<string>()
            {
                $"{BuildSettings.TargetPlatform}",
                $"{BuildSettings.TargetPlatform}/{BuildSettings.BuildType}",
                $"{BuildSettings.TargetPlatform}/{BuildSettings.BuildType}/{BuildSettings.BundleVersion}",
            };
            foreach (var filePath in targetFiles)
            {
                var directoryName = Path.GetDirectoryName(filePath).Replace("\\", "/");
                int index = directoryName.LastIndexOf("ServerData");
                if (index > -1)
                {
                    var subDirectory = directoryName.Substring(index + "ServerData".Length);
                    if (string.IsNullOrEmpty(subDirectory) == false)
                    {
                        string[] names = subDirectory.Split('/');
                        string resultName = "";
                        foreach (var folderName in names)
                        {
                            if (string.IsNullOrEmpty(folderName) == false)
                            {
                                if (string.IsNullOrEmpty(resultName) == false)
                                    resultName += "/";

                                resultName += folderName;
                                string subFolderPath = $"{BuildSettings.TargetPlatform}/{BuildSettings.BuildType}/{BuildSettings.BundleVersion}/{resultName}";
                                targetFolders.Add(subFolderPath); //
                            }
                        }
                    }
                }
            }
            foreach (var path in targetFolders)
            {
                // HTTP MKCOL 메서드로 폴더 생성 요청
                string folderUrl = $"{BuildSettings.PatchUrl}/{path}";
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
                        // 405: already exsit folder
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
                totalCount = targetFiles.Length
            };
            string uploadPath = $"{BuildSettings.PatchUrl}/{BuildSettings.TargetPlatform}/{BuildSettings.BuildType}/{BuildSettings.BundleVersion}";
            foreach (var filePath in targetFiles)
            {
                uploadInfo.fileName = Path.GetFileName(filePath);

                var directoryName = Path.GetDirectoryName(filePath).Replace("\\", "/");
                int index = directoryName.LastIndexOf("ServerData");
                string destinationUrl = $"{uploadPath}{directoryName.Substring(index + "ServerData".Length)}";

                try
                {
                    var fileData = await File.ReadAllBytesAsync(filePath);
                    var formData = new List<IMultipartFormSection>
                    {
                        new MultipartFormFileSection("file", fileData, uploadInfo.fileName, "application/octet-stream")
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
                            Debug.LogError($"[UploadToHfs] Fail upload..: {filePath} | Error: {www.error} | Response Code: {www.responseCode}");
                    }
                    uploadInfo.currentIndex++;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UploadToHfs] Fail upload ex: {filePath} | ex: {e}");
                }
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