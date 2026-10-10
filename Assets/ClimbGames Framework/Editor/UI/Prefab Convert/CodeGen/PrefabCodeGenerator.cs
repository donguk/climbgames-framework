using UnityEditor;

namespace ClimbGames.Editor.UI
{
    public class PrefabCodeGenerator : CodeGenerator
    {
        public static readonly string TemplateGUID = "3f97c32d3d31ede44b44bb53e15dfb6d";
        public static readonly string ImplementTemplateGUID = "6308713ee9fe38a4789b4a9d023614f7";






        public override bool Write(string path)
        {
            bool isChanged = false;




            //AssetDatabase.ImportAsset(filePath);
            return isChanged;
        }

    }
}