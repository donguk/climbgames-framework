using System;
using UnityEditor;
using UnityEditor.Compilation;

namespace ClimbGames.Editor
{
    public static class CodeCompilation<T>
    {
        private static readonly string ReloadKey = $"{typeof(T)}_Reload";
        private static readonly string DataKey = $"{typeof(T)}_Data";
        private static bool compilationFailed;

        static CodeCompilation()
        {
            compilationFailed = false;

            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
        }

        public static bool IsFinished()
        {
            return SessionState.GetBool(ReloadKey, false);
        }

        public static void Clear()
        {
            SessionState.SetBool(ReloadKey, false);
            SessionState.SetString(DataKey, string.Empty);
        }

        public static string GetData()
        {
            return SessionState.GetString(DataKey, string.Empty);
        }

        public static void SetData(string value)
        {
            SessionState.SetString(DataKey, value);
        }

        private static void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            foreach (var message in messages)
            {
                if (message.type == CompilerMessageType.Error)
                {
                    Debug.LogError(
                            $"[{typeof(T).Name}] Compile Error\n" +
                            $"Assembly: {assemblyPath}\n" +
                            $"File: {message.file}\n" +
                            $"Line: {message.line}\n" +
                            $"Column: {message.column}\n" +
                            $"{message.message}");

                    compilationFailed = true;
                    break;
                }
            }
        }

        static void OnCompilationFinished(object context)
        {
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;

            if (compilationFailed)
                return;

            SessionState.SetBool(ReloadKey, true);
        }

        public static void Start()
        {
            compilationFailed = false;

            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;

            CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
        }
    }
}