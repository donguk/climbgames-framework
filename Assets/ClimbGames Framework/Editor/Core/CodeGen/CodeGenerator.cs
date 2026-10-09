using System.IO;
using System.Text;
using UnityEditor;

namespace ClimbGames.Editor
{
    public interface ICodeGenerator
    {
        bool Write(string path);
    }

    public abstract class CodeGenerator : ICodeGenerator
    {
        public virtual bool Write(string path)
        {
            return false;
        }

        public static bool Write(string text, string filePath)
        {
            string path = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(path);

            if (File.Exists(filePath))
            {
                string oldText = File.ReadAllText(filePath);
                if (oldText == text)
                    return false;
            }

            UTF8Encoding encoding = new UTF8Encoding(true);
            File.WriteAllText(filePath, text, encoding);

            // 동일한 파일이라도 호출시 컴파일 발생
            AssetDatabase.ImportAsset(filePath);
            return true;
        }

        public static string CreateScript(string templateGUID, string scriptName)
        {
            return CreateScript(templateGUID, null, scriptName);
        }

        public static string CreateScript(string templateGUID, string @namespace, string scriptName)
        {
            string templatePath = AssetDatabase.GUIDToAssetPath(templateGUID);
            string scriptText = File.ReadAllText(templatePath);

            scriptText = scriptText.Replace("#NAMESPACE#", @namespace);
            scriptText = scriptText.Replace("#SCRIPTNAME#", scriptName);

            return scriptText;
        }
    }
}