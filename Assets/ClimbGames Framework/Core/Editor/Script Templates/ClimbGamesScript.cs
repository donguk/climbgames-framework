using System.IO;
using UnityEditor;

namespace ClimbGames.Core.Editor
{
    public class ClimbGamesScript
    {
        private static string GetTemplatePath(string fileName)
        {
            string[] guids = AssetDatabase.FindAssets($"{fileName} t:TextAsset");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == fileName)
                    return path;
            }

            return null;
        }

        [MenuItem("Assets/Create/Scripting/ClimbGames/MonoBehaviour Script")]
        public static void CreateMonoBehaviourScript()
        {
            string path = GetTemplatePath("ClimbGames C# MonoBehaviour Script");
            if (path != null)
                ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "New MonoBehaviour Script.cs");
        }

        [MenuItem("Assets/Create/Scripting/ClimbGames/Empty C# Script")]
        public static void CreateCSharpScript()
        {
            string path = GetTemplatePath("ClimbGames C# Script");
            if (path != null)
                ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "New Empty Script.cs");
        }
    }
}

