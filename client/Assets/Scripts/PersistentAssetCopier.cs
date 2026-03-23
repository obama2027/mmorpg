using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Copies StreamingAssets/Platform to persistentDataPath on first launch (or when copy was interrupted).
/// Writes version.json only after all files are copied successfully.
/// </summary>
public static class PersistentAssetCopier
{
    private const string VersionJsonFileName = "version.json";

    public static IEnumerator CopyIfNeeded()
    {
        var targetVersionPath = Path.Combine(Application.persistentDataPath, VersionJsonFileName);
        if (File.Exists(targetVersionPath))
        {
            if (IsValidManifestJson(targetVersionPath))
            {
                yield break;
            }

            TryDeleteFile(targetVersionPath);
            Debug.LogWarning("[Launch] Found invalid version.json, recopying assets.");
        }

        var platformName = GetPlatformName();
        var sourcePlatformDir = CombineUrl(Application.streamingAssetsPath, platformName);
        string downloadedManifestJson = null;

        yield return LoadTextFromStreamingAssets(
            CombineUrl(sourcePlatformDir, VersionJsonFileName),
            content => downloadedManifestJson = content,
            error => downloadedManifestJson = null);

        if (string.IsNullOrWhiteSpace(downloadedManifestJson))
        {
            Debug.LogError($"[Launch] Failed to load source version.json from {sourcePlatformDir}");
            yield break;
        }

        var manifest = JsonUtility.FromJson<VersionJsonManifest>(downloadedManifestJson);
        if (manifest == null)
        {
            Debug.LogError("[Launch] Invalid source version.json content.");
            yield break;
        }

        Directory.CreateDirectory(Application.persistentDataPath);

        var filesToCopy = CollectFilesToCopy(manifest, platformName);
        for (var i = 0; i < filesToCopy.Count; i++)
        {
            var relativePath = filesToCopy[i];
            var sourceUrl = CombineUrl(sourcePlatformDir, relativePath.Replace("\\", "/"));
            var normalizedRelativePath = relativePath.Replace("/", Path.DirectorySeparatorChar.ToString());
            var destinationPath = Path.Combine(Application.persistentDataPath, normalizedRelativePath);

            var destinationDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            var copyDone = false;
            var copyError = string.Empty;
            yield return CopyStreamingAssetFileToPersistent(
                sourceUrl,
                destinationPath,
                () => copyDone = true,
                error => copyError = error);

            if (!copyDone)
            {
                Debug.LogError($"[Launch] Copy failed: {relativePath}, error={copyError}");
                yield break;
            }
        }

        File.WriteAllText(targetVersionPath, downloadedManifestJson);
        Debug.Log($"[Launch] Initial copy done. platform={platformName}, fileCount={filesToCopy.Count}");
    }

    private static bool IsValidManifestJson(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            return JsonUtility.FromJson<VersionJsonManifest>(json) != null;
        }
        catch
        {
            return false;
        }
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Launch] Failed to delete file: {path}, ex={ex.Message}");
        }
    }

    private static List<string> CollectFilesToCopy(VersionJsonManifest manifest, string platformName)
    {
        var result = new List<string>();
        var dedup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (manifest.Dlls != null)
        {
            for (var i = 0; i < manifest.Dlls.Count; i++)
            {
                AddIfValid(result, dedup, manifest.Dlls[i]?.Path);
            }
        }

        if (manifest.Bundles != null)
        {
            for (var i = 0; i < manifest.Bundles.Count; i++)
            {
                AddIfValid(result, dedup, manifest.Bundles[i]?.Path);
            }
        }

        AddIfValid(result, dedup, platformName);
        AddIfValid(result, dedup, platformName + ".manifest");

        return result;
    }

    private static void AddIfValid(List<string> list, HashSet<string> dedup, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var normalized = path.Replace("\\", "/").Trim('/');
        if (normalized.Equals(VersionJsonFileName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (dedup.Add(normalized))
        {
            list.Add(normalized);
        }
    }

    private static IEnumerator CopyStreamingAssetFileToPersistent(
        string sourcePathOrUrl,
        string destinationPath,
        Action onSuccess,
        Action<string> onError)
    {
#if UNITY_ANDROID || UNITY_IOS
        using (var request = UnityWebRequest.Get(sourcePathOrUrl))
        {
            request.downloadHandler = new DownloadHandlerFile(destinationPath);
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error);
                yield break;
            }
        }
#else
        try
        {
            if (!File.Exists(sourcePathOrUrl))
            {
                onError?.Invoke("Source file not found.");
                yield break;
            }

            File.Copy(sourcePathOrUrl, destinationPath, true);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
            yield break;
        }
#endif

        onSuccess?.Invoke();
    }

    private static IEnumerator LoadTextFromStreamingAssets(
        string sourcePathOrUrl,
        Action<string> onSuccess,
        Action<string> onError)
    {
#if UNITY_ANDROID || UNITY_IOS
        using (var request = UnityWebRequest.Get(sourcePathOrUrl))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error);
                yield break;
            }

            onSuccess?.Invoke(request.downloadHandler.text);
        }
#else
        try
        {
            if (!File.Exists(sourcePathOrUrl))
            {
                onError?.Invoke("Source file not found.");
                yield break;
            }

            onSuccess?.Invoke(File.ReadAllText(sourcePathOrUrl));
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
#endif
    }

    private static string CombineUrl(string left, string right)
    {
        if (string.IsNullOrEmpty(left))
        {
            return right ?? string.Empty;
        }

        if (string.IsNullOrEmpty(right))
        {
            return left;
        }

        return left.TrimEnd('/') + "/" + right.TrimStart('/');
    }

    private static string GetPlatformName()
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
        throw new PlatformNotSupportedException("Unsupported runtime platform.");
#endif
    }

    [Serializable]
    private sealed class VersionJsonManifest
    {
        public string Platform;
        public string Version;
        public string BuildTime;
        public List<VersionJsonDllEntry> Dlls;
        public List<VersionJsonBundleEntry> Bundles;
    }

    [Serializable]
    private sealed class VersionJsonDllEntry
    {
        public string Path;
    }

    [Serializable]
    private sealed class VersionJsonBundleEntry
    {
        public string Path;
    }
}
