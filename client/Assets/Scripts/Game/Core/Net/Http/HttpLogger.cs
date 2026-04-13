using UnityEngine;

public static class HttpLogger
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

        Debug.Log($"[Http][{scope}] {message}");
    }

    public static void Warn(string scope, string message)
    {
        if (!EnableWarningLog)
        {
            return;
        }

        Debug.LogWarning($"[Http][{scope}] {message}");
    }

    public static void Error(string scope, string message)
    {
        if (!EnableErrorLog)
        {
            return;
        }

        Debug.LogError($"[Http][{scope}] {message}");
    }
}
