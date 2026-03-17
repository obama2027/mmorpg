using System;
using System.IO;
using UnityEngine;

public static class BundlePathUtility
{
    private const char BundleNameSeparator = '_';
    private const string BundleFileExtension = ".bundle";

    public static string GetRuntimePlatformName()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return "StandaloneWindows64";
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            return "StandaloneOSX";
#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            return "StandaloneLinux64";
#elif UNITY_ANDROID
            return "Android";
#elif UNITY_IOS
            return "iOS";
#else
            throw new PlatformNotSupportedException("Unsupported AssetBundle runtime platform.");
#endif
    }

    public static string GetBundleRootPath(string rootFolderName)
    {
        var path = Path.Combine(Application.streamingAssetsPath, GetRuntimePlatformName());
        return Normalize(path);
    }

    public static string GetManifestFilePath(string rootFolderName)
    {
        var path = Path.Combine(GetBundleRootPath(rootFolderName), GetRuntimePlatformName());
        return Normalize(path);
    }

    public static string GetBundleFilePath(string rootFolderName, string bundleName)
    {
        var path = Path.Combine(GetBundleRootPath(rootFolderName), GetBundleFileName(bundleName));
        return Normalize(path);
    }

    public static string GetBundleFileName(string bundleName)
    {
        return GetRuntimeBundleName(bundleName);
    }

    public static string GetRuntimeBundleName(string bundleName)
    {
        if (string.IsNullOrWhiteSpace(bundleName))
        {
            return string.Empty;
        }

        var normalizedBundleName = bundleName.Replace("\\", "/").Trim('/');
        if (!normalizedBundleName.Contains("/") &&
            normalizedBundleName.EndsWith(BundleFileExtension, StringComparison.OrdinalIgnoreCase))
        {
            return normalizedBundleName;
        }

        return normalizedBundleName.Replace('/', BundleNameSeparator) + BundleFileExtension;
    }

    private static string Normalize(string path)
    {
        return path.Replace("\\", "/");
    }
}
