using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace ClimbGames.Editor
{
    public enum BuildType
    {
        Dev,
        Qa,
        Live,
    }

    public static class BuildSettings
    {
        private static string EditorKey => $"{Application.dataPath.GetHashCode()}";

        // common
        private static string rootPath;
        private static BuildType buildType = BuildType.Dev;
        private static string bundleVersion = "0.1.0";
        private static int versionCode = 1;
        private static int buildNumber = 1;
        private static string patchUrl;
        private static bool developmentBuild = true;

        // Android
        private static bool buildAppBundle;
        private static string keystoreName;
        private static string keystorePass;
        private static string keyaliasName;
        private static string keyaliasPass;
        //

        public static string TargetPlatform => EditorUserBuildSettings.activeBuildTarget.ToString();
        public static BuildTargetGroup TargetGroup => BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

        public static string RootPath
        {
            get => rootPath;
            set => EditorPrefs.SetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(rootPath)}", rootPath = value);
        }
        public static string BuildPath => Path.Combine(RootPath, $"{TargetPlatform}");
        public static string AddressablesPath => Path.Combine(RootPath, $"{TargetPlatform}/Addressables");
        public static BuildType BuildType
        {
            get => buildType;
            set => EditorPrefs.SetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(buildType)}", (buildType = value).ToString());
        }
        public static string BundleVersion
        {
            get => bundleVersion;
            set => EditorPrefs.SetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(bundleVersion)}", bundleVersion = value);
        }
        public static int VersionCode
        {
            get => versionCode;
            set => EditorPrefs.SetInt($"{EditorKey}_{nameof(BuildSettings)}_{nameof(versionCode)}", versionCode = value);
        }
        public static int BuildNumber
        {
            get => buildNumber;
            set => EditorPrefs.SetInt($"{EditorKey}_{nameof(BuildSettings)}_{nameof(buildNumber)}", buildNumber = value);
        }
        public static string PatchUrl
        {
            get => patchUrl;
            set => EditorPrefs.SetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(patchUrl)}", patchUrl = value);
        }
        public static bool DevelopmentBuild
        {
            get => developmentBuild;
            set => EditorPrefs.SetBool($"{EditorKey}_{nameof(BuildSettings)}_{nameof(developmentBuild)}", developmentBuild = value);
        }
        public static string KeystoreName
        {
            get => keystoreName;
            set => EditorPrefs.SetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(keystoreName)}", keystoreName = value);
        }
        public static string KeystorePass
        {
            get => keystorePass;
            set => EditorPrefs.SetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(keystorePass)}", keystorePass = value);
        }
        public static string KeyaliasName
        {
            get => keyaliasName;
            set => EditorPrefs.SetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(keyaliasName)}", keyaliasName = value);
        }
        public static string KeyaliasPass
        {
            get => keyaliasPass;
            set => EditorPrefs.SetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(keyaliasPass)}", keyaliasPass = value);
        }
        public static bool BuildAppBundle
        {
            get => buildAppBundle;
            set => EditorPrefs.SetBool($"{EditorKey}_{nameof(BuildSettings)}_{nameof(buildAppBundle)}", buildAppBundle = value);
        }

        static BuildSettings()
        {
            string defatulRootPath = Path.Combine(Directory.GetCurrentDirectory(), "Build");
            rootPath = EditorPrefs.GetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(rootPath)}", defatulRootPath);

            if (System.Enum.TryParse(typeof(BuildType), EditorPrefs.GetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(buildType)}", "Dev"), out var result))
                buildType = (BuildType)result;

            bundleVersion = EditorPrefs.GetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(bundleVersion)}", "0.1.0");
            versionCode = EditorPrefs.GetInt($"{EditorKey}_{nameof(BuildSettings)}_{nameof(versionCode)}", 1);
            buildNumber = EditorPrefs.GetInt($"{EditorKey}_{nameof(BuildSettings)}_{nameof(buildNumber)}", 1);
            patchUrl = EditorPrefs.GetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(patchUrl)}", string.Empty);

            // Android
            keystoreName = EditorPrefs.GetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(keystoreName)}", string.Empty);
            keystorePass = EditorPrefs.GetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(keystorePass)}", string.Empty);
            keyaliasName = EditorPrefs.GetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(keyaliasName)}", string.Empty);
            keyaliasPass = EditorPrefs.GetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(keyaliasPass)}", string.Empty);
        }

        public static void ApplySettings()
        {
            PlayerSettings.bundleVersion = bundleVersion;
            switch (TargetGroup)
            {
                case BuildTargetGroup.Android:
                    {
                        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);

                        PlayerSettings.Android.bundleVersionCode = versionCode;
                        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;

                        PlayerSettings.Android.keystoreName = keystoreName;
                        PlayerSettings.Android.keystorePass = keystorePass;
                        PlayerSettings.Android.keyaliasName = keyaliasName;
                        PlayerSettings.Android.keyaliasPass = keyaliasPass;

                        EditorUserBuildSettings.connectProfiler = developmentBuild;
                        EditorUserBuildSettings.allowDebugging = developmentBuild;
                        EditorUserBuildSettings.buildAppBundle = buildAppBundle;
                        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
                        break;
                    }
            }
        }

        public static void LoadFromProfile(BuildProfile profile)
        {
            BuildType = profile.buildType;
            BundleVersion = profile.bundleVersion;
            VersionCode = profile.versionCode;
            PatchUrl = profile.patchUrl;
            buildAppBundle = BuildType == BuildType.Live;
        }


        //
        private const string JenkinsfileGUID = "c318bb60c53c5b74dbaeb3ef40bcb015";
        private const string JenkinsScriptsGUID = "707ccc579e3a01c47a7752f000ab767c";

        [MenuItem("Tools/ClimbGames/Import Jenkinsfile")]
        public static void ImportJenkinsfile()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string jenkinsfileAssetPath = AssetDatabase.GUIDToAssetPath(JenkinsfileGUID);
            string scriptsFolderAssetPath = AssetDatabase.GUIDToAssetPath(JenkinsScriptsGUID);
            if (string.IsNullOrEmpty(jenkinsfileAssetPath) || string.IsNullOrEmpty(scriptsFolderAssetPath))
            {
                Debug.LogError("[BuildSettings] Could not find GUID for original asset. Please check framework asset status.");
                return;
            }

            string gitUrl = GetGitRemoteUrl(projectRoot);
            if (string.IsNullOrEmpty(gitUrl))
                gitUrl = "https://"; // fallback

            string targetPath = Path.Combine(projectRoot, "Jenkins");
            Directory.CreateDirectory(targetPath);
            string destJenkinsfilePath = Path.Combine(targetPath, "Jenkinsfile");
            File.Copy(Path.GetFullPath(jenkinsfileAssetPath), destJenkinsfilePath, true);
            string text = File.ReadAllText(destJenkinsfilePath);

            text = text.Replace("#GIT_URL#", gitUrl);
            UTF8Encoding encoding = new UTF8Encoding(false);
            File.WriteAllText(destJenkinsfilePath, text, encoding);

            string scriptsPath = Path.Combine(projectRoot, "Jenkins/Scripts");
            Directory.CreateDirectory(scriptsPath);
            string[] scriptFiles = Directory.GetFiles(scriptsFolderAssetPath);
            foreach (var filePath in scriptFiles)
            {
                if (filePath.EndsWith(".meta")) continue;

                string fileName = Path.GetFileName(filePath);
                string destFilePath = Path.Combine(scriptsPath, fileName);
                File.Copy(filePath, destFilePath, overwrite: true);
            }

            Debug.Log($"[BuildSettings] Import Jenkinsfile: {targetPath}");
        }

        /// <summary>
        /// Git CLI를 실행하여 현재 Repository의 Remote URL(origin)을 가져옵니다.
        /// </summary>
        private static string GetGitRemoteUrl(string workingDirectory)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "remote get-url origin",
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process == null) return string.Empty;

                    string output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();

                    if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                    {
                        return output;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BuildSettings] Failed to get Git remote URL: {ex.Message}");
            }

            return string.Empty;
        }
    }
}