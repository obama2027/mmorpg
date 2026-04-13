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

    public static string GetBundleNameFromAssetPath(string assetPath)
    {
        if (string.IsNullOrWhiteSpace(assetPath))
        {
            return string.Empty;
        }

        var normalized = assetPath.Replace("\\", "/").Trim();
        if (!normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        var lastSlashIndex = normalized.LastIndexOf('/');
        if (lastSlashIndex <= "Assets".Length)
        {
            return string.Empty;
        }

        // Bundle naming rule follows editor side auto naming:
        // directory path under Assets/, lower-case, '/' => '_'
        var dirPath = normalized.Substring(0, lastSlashIndex);
        var relativeDir = dirPath.Substring("Assets/".Length).Trim('/');
        return relativeDir.ToLowerInvariant().Replace('/', BundleNameSeparator);
    }

    public static string BuildAssetPath(string bundlePath, string assetPath)
    {
        if (string.IsNullOrWhiteSpace(bundlePath) || string.IsNullOrWhiteSpace(assetPath))
        {
            return string.Empty;
        }

        var normalizedBundlePath = bundlePath.Replace("\\", "/").Trim('/');
        var normalizedAssetPath = assetPath.Replace("\\", "/").Trim('/');
        return $"Assets/{normalizedBundlePath}/{normalizedAssetPath}";
    }

    private static string Normalize(string path)
    {
        return path.Replace("\\", "/");
    }
}
