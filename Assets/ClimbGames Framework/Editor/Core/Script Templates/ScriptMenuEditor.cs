using System.IO;
using UnityEditor;
using UnityEngine;

namespace ClimbGames.Editor
{
    public class ScriptMenuEditor
    {
        public static string GetTemplatePath(string fileName)
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
            Texture2D icon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<ScriptKeywordProcessor>(), "NewMonoBehaviourScript.cs", icon, path);
        }

        [MenuItem("Assets/Create/Scripting/ClimbGames/Empty C# Script")]
        public static void CreateCSharpScript()
        {
            string path = GetTemplatePath("ClimbGames C# Script");
            Texture2D icon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<ScriptKeywordProcessor>(), "NewEmptyScript.cs", icon, path);
        }
    }
}

