using System.Diagnostics;
using UnityEngine;

namespace ClimbGames
{
    public static class Debug
    {
        [HideInCallstack]
        [Conditional("UNITY_EDITOR"), Conditional("DEV_BUILD"), Conditional("QA_BUILD")]
        public static void Log(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        [HideInCallstack]
        [Conditional("UNITY_EDITOR"), Conditional("DEV_BUILD"), Conditional("QA_BUILD")]
        public static void Assert(bool condition, object message)
        {
            UnityEngine.Debug.Assert(condition, message);
        }

        [HideInCallstack]
        [Conditional("UNITY_EDITOR"), Conditional("DEV_BUILD"), Conditional("QA_BUILD")]
        public static void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        [HideInCallstack]
        [Conditional("UNITY_EDITOR"), Conditional("DEV_BUILD"), Conditional("QA_BUILD"), Conditional("LIVE_BUILD")]
        public static void LogError(object message)
        {
            UnityEngine.Debug.LogError(message);
        }
    }
}
