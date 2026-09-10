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
        Dev,
        QA,
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

        // iOS
        private static bool appleEnableAutomaticSigning;
        private static string appleDeveloperTeamID;
        private static string _iOSManualProvisioningProfileID;
        private static ProvisioningProfileType _iOSManualProvisioningProfileType;
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
        public static bool AppleEnableAutomaticSigning
        {
            get => appleEnableAutomaticSigning;
            set => EditorPrefs.SetBool($"{EditorKey}_{nameof(BuildSettings)}_{nameof(appleEnableAutomaticSigning)}", appleEnableAutomaticSigning = value);
        }
        public static string AppleDeveloperTeamID
        {
            get => appleDeveloperTeamID;
            set => EditorPrefs.SetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(appleDeveloperTeamID)}", appleDeveloperTeamID = value);
        }
        public static string iOSManualProvisioningProfileID
        {
            get => _iOSManualProvisioningProfileID;
            set => EditorPrefs.SetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(iOSManualProvisioningProfileID)}", _iOSManualProvisioningProfileID = value);
        }
        public static ProvisioningProfileType iOSManualProvisioningProfileType
        {
            get => _iOSManualProvisioningProfileType;
            set => EditorPrefs.SetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(iOSManualProvisioningProfileType)}", (_iOSManualProvisioningProfileType = value).ToString());
        }

        static BuildSettings()
        {
            string defatulRootPath = Path.Combine(Directory.GetCurrentDirectory(), "Build");
            rootPath = EditorPrefs.GetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(rootPath)}", defatulRootPath);

            if (System.Enum.TryParse(typeof(BuildType), EditorPrefs.GetString($"{EditorKey}_{nameof(BuildSettings)}_{nameof(buildType)}", $"{BuildType.Dev}"), out var result))
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

                        EditorUserBuildSettings.buildAppBundle = buildAppBundle;
                        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
                        break;
                    }

                case BuildTargetGroup.iOS:
                    {
                        PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);

                        PlayerSettings.iOS.buildNumber = buildNumber.ToString();
                        PlayerSettings.iOS.appleEnableAutomaticSigning = appleEnableAutomaticSigning;
                        PlayerSettings.iOS.appleDeveloperTeamID = appleDeveloperTeamID;
                        PlayerSettings.iOS.iOSManualProvisioningProfileID = _iOSManualProvisioningProfileID;
                        PlayerSettings.iOS.iOSManualProvisioningProfileType = _iOSManualProvisioningProfileType;
                        break;
                    }
            }

            EditorUserBuildSettings.connectProfiler = developmentBuild;
            EditorUserBuildSettings.allowDebugging = developmentBuild;
        }

        public static void LoadFromProfile(BuildProfile profile)
        {
            BuildType = profile.buildType;
            BundleVersion = profile.bundleVersion;
            VersionCode = profile.versionCode;
            PatchUrl = profile.patchUrl;

            buildAppBundle = buildType == BuildType.Live;
            iOSManualProvisioningProfileType = buildType == BuildType.Live ? ProvisioningProfileType.Distribution : ProvisioningProfileType.Development;
            developmentBuild = buildType != BuildType.Live;
        }
    }
}