using System;
using System.Reflection;

public static class ConfigBridge
{
    private static Type _configType;
    private static FieldInfo _isDebugField;

    private static bool EnsureCache()
    {
        if (_configType != null) return true;

        _configType = Type.GetType("Config, Assembly-CSharp");
        if (_configType == null) return false;

        _isDebugField = _configType.GetField("isDebug", BindingFlags.Public | BindingFlags.Static);
        return _isDebugField != null;
    }

    public static bool GetIsDebug(bool fallback = false)
    {
        if (!EnsureCache()) return fallback;
        try { return (bool)_isDebugField.GetValue(null); }
        catch { return fallback; }
    }
}