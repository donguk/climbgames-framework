using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace ClimbGames.Editor
{
    public class FrameworkSettingsBuildPreprocess : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            // FrameworkSettings 에셋 로드 (AssetDatabase 활용)
            var settings = FrameworkSettings.Instance;
            if (settings == null)
            {
                Debug.LogWarning("[FrameworkSettings] Could not find FrameworkSettings asset to include in the build.");
                return;
            }

            // Preloaded Assets 목록 가져오기
            var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
            preloadedAssets.RemoveAll(asset => asset == null); // 유실된 널 에셋 정리

            // 목록에 없으면 등록
            if (!preloadedAssets.Contains(settings))
            {
                preloadedAssets.Add(settings);
                PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
                Debug.Log("[FrameworkSettings] Register Preloaded Assets.");
            }
        }
    }
}