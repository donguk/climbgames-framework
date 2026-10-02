using System.IO;
using System.Linq;
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

        public static string[] GetFiles(string path, params string[] extensions)
        {
            string[] files = new string[] { };

            foreach (var extension in extensions)
                files = files.Concat(Directory.GetFiles(path, $"*.{extension}", SearchOption.AllDirectories)).ToArray();

            return files;
        }
    }

    public static class PathExtensions
    {
        public static string ToUnityRelativePath(this string path)
        {
            return Paths.GetUnityRelativePath(path);
        }

        public static string TrimEnd(this string path, params string[] suffix)
        {
            if (suffix == null)
                return path;

            foreach (var value in suffix)
            {
                if (path.EndsWith(value))
                    return path.Substring(0, path.Length - value.Length);
            }

            return path;
        }
    }
}