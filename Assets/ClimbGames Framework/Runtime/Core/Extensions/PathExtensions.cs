using System.IO;
using UnityEngine;

namespace ClimbGames
{
    public static class Paths
    {
        public static string GetUnityRelativePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            string projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
            string relativePath = Path.GetRelativePath(projectPath, path).Replace("\\", "/");

            return relativePath;
        }
    }

    public static class PathExtensions
    {
        public static string ToUnityRelativePath(this string path)
        {
            return Paths.GetUnityRelativePath(path);
        }
    }
}