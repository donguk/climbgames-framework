using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace ClimbGames
{
    public static class JenkinsInitializer
    {
        private const string JenkinsfileGUID = "c318bb60c53c5b74dbaeb3ef40bcb015";
        private const string JenkinsScriptsGUID = "707ccc579e3a01c47a7752f000ab767c";

        [MenuItem("Tools/ClimbGames/Import Jenkinsfile")]
        public static void ImportJenkinsfile()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string jenkinsfileAssetPath = AssetDatabase.GUIDToAssetPath(JenkinsfileGUID);
            string scriptsFolderAssetPath = AssetDatabase.GUIDToAssetPath(JenkinsScriptsGUID);
            if (string.IsNullOrEmpty(jenkinsfileAssetPath) || string.IsNullOrEmpty(scriptsFolderAssetPath))
            {
                Debug.LogError("[BuildSettings] Could not find GUID for original asset. Please check framework asset status.");
                return;
            }

            string gitUrl = GetGitRemoteUrl(projectRoot);
            if (string.IsNullOrEmpty(gitUrl))
                gitUrl = "https://"; // fallback

            string targetPath = Path.Combine(projectRoot, "Jenkins");
            Directory.CreateDirectory(targetPath);
            string destJenkinsfilePath = Path.Combine(targetPath, "Jenkinsfile");
            File.Copy(Path.GetFullPath(jenkinsfileAssetPath), destJenkinsfilePath, true);
            string text = File.ReadAllText(destJenkinsfilePath);

            text = text.Replace("#GIT_URL#", gitUrl);
            UTF8Encoding encoding = new UTF8Encoding(false);
            File.WriteAllText(destJenkinsfilePath, text, encoding);

            string scriptsPath = Path.Combine(projectRoot, "Jenkins/Scripts");
            Directory.CreateDirectory(scriptsPath);
            string[] scriptFiles = Directory.GetFiles(scriptsFolderAssetPath);
            foreach (var filePath in scriptFiles)
            {
                if (filePath.EndsWith(".meta")) continue;

                string fileName = Path.GetFileName(filePath);
                string destFilePath = Path.Combine(scriptsPath, fileName);
                File.Copy(filePath, destFilePath, overwrite: true);
            }

            Debug.Log($"[BuildSettings] Import Jenkinsfile: {targetPath}");
        }

        /// <summary>
        /// Git CLI를 실행하여 현재 Repository의 Remote URL(origin)을 가져옵니다.
        /// </summary>
        private static string GetGitRemoteUrl(string workingDirectory)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "remote get-url origin",
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process == null) return string.Empty;

                    string output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();

                    if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                    {
                        return output;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BuildSettings] Failed to get Git remote URL: {ex.Message}");
            }

            return string.Empty;
        }
    }
}