using UnityEngine;

namespace MMORPG.Client.Core.Bundle
{
    public static class BundleLogger
    {
        public static bool EnableInfoLog = true;
        public static bool EnableWarningLog = true;
        public static bool EnableErrorLog = true;

        public static void Info(string scope, string message)
        {
            if (!EnableInfoLog)
            {
                return;
            }

            Debug.Log($"[Bundle][{scope}] {message}");
        }

        public static void Warn(string scope, string message)
        {
            if (!EnableWarningLog)
            {
                return;
            }

            Debug.LogWarning($"[Bundle][{scope}] {message}");
        }

        public static void Error(string scope, string message)
        {
            if (!EnableErrorLog)
            {
                return;
            }

            Debug.LogError($"[Bundle][{scope}] {message}");
        }
    }
}
