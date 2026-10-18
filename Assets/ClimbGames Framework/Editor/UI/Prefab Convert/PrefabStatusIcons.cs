using UnityEditor;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    [InitializeOnLoad]
    public static class PrefabStatusIcons
    {
        static readonly string FailedIconGUID = "c9c4e3db2eeebe84db64403a65768423";
        static readonly string IdleIconGUID = "b0c21ed44319e004e925d67212461471";
        static readonly string ProcessingIconGUID = "3fe0e73f30d443444a71c347bd1abe42";
        static readonly string SuccessIconGUID = "5916f925efce8804da9116a44dcc6c6c";

        public static readonly Texture2D Failed = AssetDatabase.LoadAssetByGUID<Texture2D>(new GUID(FailedIconGUID));
        public static readonly Texture2D Idle = AssetDatabase.LoadAssetByGUID<Texture2D>(new GUID(IdleIconGUID));
        public static readonly Texture2D Processing = AssetDatabase.LoadAssetByGUID<Texture2D>(new GUID(ProcessingIconGUID));
        public static readonly Texture2D Success = AssetDatabase.LoadAssetByGUID<Texture2D>(new GUID(SuccessIconGUID));

        static PrefabStatusIcons()
        {
            //
        }
    }
}