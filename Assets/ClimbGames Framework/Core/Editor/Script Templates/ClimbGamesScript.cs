using UnityEditor;

namespace ClimbGames.Core.Editor
{
    public class ClimbGamesScript
    {
        private const string TemplatePath = "Assets/ClimbGames/Core/Editor/Templates/C# MonoBehaviour Script.txt";

        [MenuItem("Assets/Create/Scripting/ClimbGames/MonoBehaviour Script")]
        public static void CreateScript()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(TemplatePath, "New MonoBehaviour Script.cs");
        }
    }
}

