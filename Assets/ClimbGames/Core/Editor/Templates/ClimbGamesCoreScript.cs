using UnityEditor;

namespace ClimbGames.Core.Editor
{
    public class ClimbGamesCoreScript
    {
        private const string TemplatePath = "Assets/ClimbGames/Core/Editor/Templates/C# ClimbGamesCoreScript.txt";

        [MenuItem("Assets/Create/Scripting/ClimbGames/Core Script")]
        public static void CreateScript()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(TemplatePath, "NewCoreScript.cs");
        }
    }
}

