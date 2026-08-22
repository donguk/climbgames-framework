using System.IO;
using UnityEditor;

namespace ClimbGames.Editor
{
    public static class BuildSettings
    {
        private const string BUILD_ROOT_KEY = "BuildSettings_BuildRootPath";
        private const string PATCH_URL_KEY = "BuildSettings_PatchUrl";

        private static string buildRootPath;

        public static string bundleVersion;
        public static int versionCode;
        public static int buildNumber;
        private static string patchUrl;

        public static string BuildRootPath
        {
            get => buildRootPath;
            set
            {
                if (buildRootPath != value)
                    EditorPrefs.SetString(BUILD_ROOT_KEY, buildRootPath = value);
            }
        }
        public static string PatchUrl
        {
            get => patchUrl;
            set
            {
                if (patchUrl != value)
                    EditorPrefs.SetString(PATCH_URL_KEY, patchUrl = value);
            }
        }
        public static string TargetPlatform => EditorUserBuildSettings.activeBuildTarget.ToString();
        public static BuildTargetGroup TargetGroup => BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        public static string AddressablesRootPath => Path.Combine(BuildRootPath, $"{TargetPlatform}/Addressables");

        static BuildSettings()
        {
            buildRootPath = EditorPrefs.GetString(BUILD_ROOT_KEY, "Build");
            bundleVersion = PlayerSettings.bundleVersion;
            buildNumber = 1;
            patchUrl = EditorPrefs.GetString(PATCH_URL_KEY, string.Empty);
        }

        public static void ApplySettings()
        {
            PlayerSettings.bundleVersion = bundleVersion;
        }
    }
}