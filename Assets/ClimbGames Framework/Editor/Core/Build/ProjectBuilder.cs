using System;
using System.IO;
using System.Linq;
using UnityEditor;


namespace ClimbGames.Editor
{
    public static partial class ProjectBuilder
    {
        public static void BuildAndroid()
        {
            string fileName = $"{PlayerSettings.productName}_{BuildSettings.BuildType}_{BuildSettings.BundleVersion}({BuildSettings.VersionCode})_{BuildSettings.BuildNumber}.{(BuildSettings.BuildAppBundle ? "aab" : "apk")}";
            string buildPathName = Path.Combine($"{BuildSettings.BuildPath}", $"{fileName}");
            BuildPlayerOptions options = new BuildPlayerOptions()
            {
                scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
                locationPathName = buildPathName,
                target = BuildTarget.Android,
                options = BuildOptions.Development,
            };

            Console.Out.WriteLine($"[ProjectBuilder] BuildAndroid: {buildPathName}");
            BuildPipeline.BuildPlayer(options);
        }
    }
}