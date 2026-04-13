using System;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Loads base.dll.bytes from persistentDataPath and invokes its hot-update entry.
/// </summary>
public static class BaseDllLoader
{
    private const string BaseDllFileName = "base.dll.bytes";
    private const string EntryTypeName = "BaseHotUpdateEntry";
    private const string EntryMethodName = "Run";

    private static Assembly s_baseAssembly;

    public static IEnumerator RunBaseHotUpdate()
    {
        if (!TryGetBaseAssembly(out var assembly, out var loadError))
        {
            Debug.LogError(loadError);
            yield break;
        }

        var entryType = assembly.GetType(EntryTypeName);
        if (entryType == null)
        {
            Debug.LogError($"[BaseDllLoader] Entry type not found: {EntryTypeName}");
            yield break;
        }

        var method = entryType.GetMethod(EntryMethodName, BindingFlags.Public | BindingFlags.Static);
        if (method == null)
        {
            Debug.LogError($"[BaseDllLoader] Entry method not found: {EntryTypeName}.{EntryMethodName}");
            yield break;
        }

        object result;
        try
        {
            result = method.Invoke(null, null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BaseDllLoader] Invoke entry failed: {ex}");
            yield break;
        }

        if (result is IEnumerator routine)
        {
            yield return routine;
            yield break;
        }

        Debug.LogWarning($"[BaseDllLoader] {EntryTypeName}.{EntryMethodName} did not return IEnumerator.");
    }

    private static bool TryGetBaseAssembly(out Assembly assembly, out string error)
    {
        error = string.Empty;

        if (s_baseAssembly != null)
        {
            assembly = s_baseAssembly;
            return true;
        }

        var dllPath = Path.Combine(Application.persistentDataPath, BaseDllFileName);
        if (!File.Exists(dllPath))
        {
            assembly = null;
            error = $"[BaseDllLoader] Missing file: {dllPath}";
            return false;
        }

        byte[] rawBytes;
        try
        {
            rawBytes = File.ReadAllBytes(dllPath);
        }
        catch (Exception ex)
        {
            assembly = null;
            error = $"[BaseDllLoader] Read dll failed: {ex.Message}";
            return false;
        }

        try
        {
            s_baseAssembly = Assembly.Load(rawBytes);
            assembly = s_baseAssembly;
            return true;
        }
        catch (Exception ex)
        {
            assembly = null;
            error = $"[BaseDllLoader] Assembly.Load failed: {ex.Message}";
            return false;
        }
    }
}
