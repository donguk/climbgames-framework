using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.ProjectWindowCallback;

namespace ClimbGames.Editor
{
    public class BuildProfile : ScriptableObject
    {
        public static string TargetPath = "Assets/ClimbGames/Editor/Build";
        private static string DefaultFileName = "NewBuildProfile.asset";

        public BuildType buildType = Editor.BuildType.Dev;
        public string bundleVersion = "0.1.0";
        public int versionCode = 1;
        public string patchUrl;

        [MenuItem("Tools/ClimbGames/Create BuildProfile")]
        public static void CreateProfile()
        {
            if (!Directory.Exists(TargetPath))
            {
                Directory.CreateDirectory(TargetPath);
                AssetDatabase.Refresh();
            }

            string defaultPath = Path.Combine(TargetPath, DefaultFileName);

            // 중복되지 않는 고유 경로 생성 (예: 이미 있으면 NewBuildProfile 1.asset)
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(defaultPath);

            // 아이콘 설정 (ScriptableObject 기본 아이콘)
            Texture2D icon = EditorGUIUtility.IconContent("ScriptableObject Icon").image as Texture2D;

            // 지정된 경로(uniquePath)로 이름 편집 모드 시작
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                ScriptableObject.CreateInstance<DoCreateBuildProfile>(),
                uniquePath,
                icon,
                null
            );
        }
    }

    // 이름 편집 완료(Enter 입력) 시 호출되는 콜백
    internal class DoCreateBuildProfile : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            BuildProfile profile = ScriptableObject.CreateInstance<BuildProfile>();

            AssetDatabase.CreateAsset(profile, pathName);
            AssetDatabase.SaveAssets();

            ProjectWindowUtil.ShowCreatedAsset(profile);
        }
    }
}