using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace ClimbGames.Editor
{
    public enum BuildType
    {
        dev,
        qa,
        live,
    }

    public static class BuildSettings
    {
        private static string EditorKey => $"{Application.dataPath.GetHashCode()}";

        // common
        private static string rootPath;
        private static BuildType buildType = BuildType.dev;
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
        public static string BuildPath => Path.Combine(rootPath, $"{TargetPlatform}/{buildType}");
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

            if (System.Enum.TryParse(typeof(BuildType), EditorPrefs.GetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(buildType)}", "dev"), out var result))
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
            buildAppBundle = BuildType == BuildType.live;
        }
    }
}