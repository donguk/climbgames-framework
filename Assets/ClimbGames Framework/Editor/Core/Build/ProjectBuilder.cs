using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;


namespace ClimbGames.Editor
{
    public static partial class ProjectBuilder
    {
        public static event Action PreBuildProcess;

        public static void BuildAndroid()
        {
            PreBuildProcess?.Invoke();

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.DoNotBuildWithPlayer;
            EditorUtility.SetDirty(settings);

            string fileName = @$"{PlayerSettings.productName}_
                {BuildSettings.BuildType}_
                {BuildSettings.BundleVersion}({BuildSettings.VersionCode})_
                {BuildSettings.BuildNumber}.{(BuildSettings.BuildAppBundle ? "aab" : "apk")}";

            string buildPathName = Path.Combine($"{BuildSettings.BuildPath}", $"{fileName}");
            BuildPlayerOptions options = new BuildPlayerOptions()
            {
                scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
                locationPathName = buildPathName,
                target = BuildTarget.Android,
                options = BuildSettings.DevelopmentBuild ? BuildOptions.Development : BuildOptions.None,
            };

            Console.Out.WriteLine($"[ProjectBuilder] BuildAndroid: {buildPathName}");
            BuildPipeline.BuildPlayer(options);
        }
    }

    public static class CommandLineBuilder
    {
        public static void BuildAndroid()
        {
            if (EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android) == false)
                throw new Exception($"Platform switch failed! Please check if the corresponding target module (Android/iOS Build Support) is installed.");

            CustomArgs customArgs = CustomArgs.ParseCommandLineArgs();
            string profileName = customArgs.GetValue<string>("profileName");

            bool existProfile = false;
            string profilePath = Path.Combine(BuildProfile.TargetPath, profileName);
            if (File.Exists(profilePath))
                existProfile = true;

            if (existProfile == false)
            {
                string branchName = customArgs.GetValue<string>("branchName");
                profilePath = Path.Combine(BuildProfile.TargetPath, branchName);
                if (File.Exists(profilePath) == false)
                    throw new Exception($"Can not find buildProfile: {profileName} or {branchName}");
            }

            BuildProfile profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(profilePath);
            profile.bundleVersion = customArgs.GetValue<string>("bundleVersion");
            profile.versionCode = customArgs.GetValue<int>("versionCode");

            BuildSettings.LoadFromProfile(profile);
            BuildSettings.BuildNumber = customArgs.GetValue<int>("buildNumber");

            bool isContentUpdates = customArgs.GetValue<bool>("isContentUpdates");
            if (isContentUpdates)
            {
                string contentStateFilePath = Path.Combine(BuildSettings.AddressablesPath, $"ContentState/{profile.bundleVersion}/addressables_content_state.bin");
                if (File.Exists(contentStateFilePath) == false)
                    throw new Exception($"Not Exist {profile.bundleVersion}'s addressables_content_state.bin");

                ProjectBuilder.BuildContentUpdate(contentStateFilePath);
            }
            else
            {
                ProjectBuilder.BuildPlayerContent();
                ProjectBuilder.BuildAndroid();
            }
        }
    }
}