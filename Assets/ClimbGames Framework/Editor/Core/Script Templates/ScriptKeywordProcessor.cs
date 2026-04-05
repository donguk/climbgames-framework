using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;

namespace ClimbGames.Editor
{
    public class ScriptKeywordProcessor : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            // resourceFile: 템플릿 파일 경로
            // pathName: 사용자가 프로젝트 창에 입력한 최종 파일 경로

            // 템플릿 읽기
            string text = File.ReadAllText(resourceFile);

            // 키워드 치환 (파일명, 프로젝트명, 날짜 등)
            string fileName = Path.GetFileNameWithoutExtension(pathName);
            text = text.Replace("#SCRIPTNAME#", fileName);

            string @namespace = FrameworkSettings.Instance.ProjectNamesapce;
            if (string.IsNullOrEmpty(@namespace))
                @namespace = "ClimbGames";

            text = text.Replace("#NAMESPACE#", @namespace);
            //text = text.Replace("#DATE#", System.DateTime.Now.ToString("yyyy-MM-dd"));

            // 파일 쓰기
            UTF8Encoding encoding = new UTF8Encoding(true);
            File.WriteAllText(pathName, text, encoding);

            // 에셋 데이터베이스 갱신 및 포커스
            AssetDatabase.ImportAsset(pathName);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(pathName);
            ProjectWindowUtil.ShowCreatedAsset(asset);
        }
    }
}