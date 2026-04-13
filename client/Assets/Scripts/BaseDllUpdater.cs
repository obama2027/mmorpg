using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Checks server for newer base.dll.bytes and downloads to persistentDataPath if hash differs.
/// Runs after PersistentAssetCopier. Requires version.json in persistentDataPath.
/// </summary>
public static class BaseDllUpdater
{
    private const string VersionJsonFileName = "version.json";
    private const string BaseDllFileName = "base.dll.bytes";
    private const string DefaultResServerBaseUrl = "http://localhost:8080";

    public static IEnumerator CheckAndUpdateIfNeeded()
    {
        var persistentPath = Application.persistentDataPath;
        var localVersionPath = Path.Combine(persistentPath, VersionJsonFileName);

        if (!File.Exists(localVersionPath))
        {
            Debug.Log("[BaseDllUpdater] No local version.json, skip update.");
            yield break;
        }

        var platformName = GetPlatformName();

        string localJson = null;
        try
        {
            localJson = File.ReadAllText(localVersionPath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BaseDllUpdater] Failed to read local version.json: {ex.Message}");
            yield break;
        }

        var localManifest = JsonUtility.FromJson<VersionJsonManifest>(localJson);
        if (localManifest?.Dlls == null)
        {
            Debug.Log("[BaseDllUpdater] No Dlls in local version.json, skip update.");
            yield break;
        }

        var localBaseEntry = FindBaseDllEntry(localManifest.Dlls);
        if (localBaseEntry == null)
        {
            Debug.Log("[BaseDllUpdater] No base.dll entry in local version.json, skip update.");
            yield break;
        }

        var serverVersion = string.Empty;
        yield return GetServerVersion(platformName, v => serverVersion = v, e => { });
        if (string.IsNullOrWhiteSpace(serverVersion))
        {
            Debug.Log("[BaseDllUpdater] Could not get server version, skip update.");
            yield break;
        }

        string serverJson = null;
        yield return GetServerVersionJson(platformName, serverVersion, json => serverJson = json, e => { });
        if (string.IsNullOrWhiteSpace(serverJson))
        {
            Debug.LogError("[BaseDllUpdater] Could not get server version.json.");
            yield break;
        }

        var serverManifest = JsonUtility.FromJson<VersionJsonManifest>(serverJson);
        if (serverManifest?.Dlls == null)
        {
            Debug.LogError("[BaseDllUpdater] Invalid server version.json.");
            yield break;
        }

        var serverBaseEntry = FindBaseDllEntry(serverManifest.Dlls);
        if (serverBaseEntry == null)
        {
            Debug.Log("[BaseDllUpdater] No base.dll in server version.json, skip update.");
            yield break;
        }

        var localMd5 = (localBaseEntry.Md5 ?? string.Empty).Trim();
        var serverMd5 = (serverBaseEntry.Md5 ?? string.Empty).Trim();

        if (string.Equals(localMd5, serverMd5, StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("[BaseDllUpdater] base.dll up to date.");
            yield break;
        }

        var downloadUrl = $"{DefaultResServerBaseUrl.TrimEnd('/')}/api/downloadResFile?platform={Uri.EscapeDataString(platformName)}&version={Uri.EscapeDataString(serverVersion)}&file={Uri.EscapeDataString(serverBaseEntry.Path)}";
        var destPath = Path.Combine(persistentPath, serverBaseEntry.Path.Replace("/", Path.DirectorySeparatorChar.ToString()));

        var downloadOk = false;
        var downloadError = string.Empty;
        yield return DownloadFile(downloadUrl, destPath, () => downloadOk = true, e => downloadError = e);

        if (!downloadOk)
        {
            Debug.LogError($"[BaseDllUpdater] Download failed: {downloadError}");
            yield break;
        }

        var localManifestCopy = JsonUtility.FromJson<VersionJsonManifest>(localJson);
        UpdateBaseDllMd5Only(localManifestCopy, serverBaseEntry.Md5);

        var updatedJson = JsonUtility.ToJson(localManifestCopy, true);
        File.WriteAllText(localVersionPath, updatedJson);
        Debug.Log($"[BaseDllUpdater] Updated base.dll to server version {serverVersion}");
    }

    private static VersionJsonDllEntry FindBaseDllEntry(List<VersionJsonDllEntry> dlls)
    {
        for (var i = 0; i < dlls.Count; i++)
        {
            var path = dlls[i]?.Path ?? string.Empty;
            if (path.Equals(BaseDllFileName, StringComparison.OrdinalIgnoreCase))
            {
                return dlls[i];
            }
        }
        return null;
    }

    private static void UpdateBaseDllMd5Only(VersionJsonManifest manifest, string serverMd5)
    {
        if (manifest.Dlls == null || string.IsNullOrEmpty(serverMd5)) return;

        for (var i = 0; i < manifest.Dlls.Count; i++)
        {
            var path = manifest.Dlls[i]?.Path ?? string.Empty;
            if (path.Equals(BaseDllFileName, StringComparison.OrdinalIgnoreCase))
            {
                manifest.Dlls[i].Md5 = serverMd5.Trim();
                return;
            }
        }
    }

    private static IEnumerator GetServerVersion(string platformName, Action<string> onSuccess, Action<string> onError)
    {
        var url = $"{DefaultResServerBaseUrl.TrimEnd('/')}/api/getResVersion?platform={Uri.EscapeDataString(platformName)}";
        using (var request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error);
                yield break;
            }

            var resp = JsonUtility.FromJson<GetResVersionResponse>(request.downloadHandler.text);
            if (resp == null || !resp.ok)
            {
                onError?.Invoke("Invalid response");
                yield break;
            }

            onSuccess?.Invoke(resp.version ?? string.Empty);
        }
    }

    private static IEnumerator GetServerVersionJson(string platformName, string version, Action<string> onSuccess, Action<string> onError)
    {
        var url = $"{DefaultResServerBaseUrl.TrimEnd('/')}/api/getVersionJson?platform={Uri.EscapeDataString(platformName)}&version={Uri.EscapeDataString(version)}";
        using (var request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error);
                yield break;
            }

            onSuccess?.Invoke(request.downloadHandler.text);
        }
    }

    private static IEnumerator DownloadFile(string url, string destinationPath, Action onSuccess, Action<string> onError)
    {
        var dir = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using (var request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerFile(destinationPath);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error);
                yield break;
            }
        }

        onSuccess?.Invoke();
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
    private class GetResVersionResponse
    {
        public bool ok;
        public string version;
    }

    [Serializable]
    private class VersionJsonManifest
    {
        public string Platform;
        public string Version;
        public string BuildTime;
        public List<VersionJsonDllEntry> Dlls;
        public List<VersionJsonBundleEntry> Bundles;
    }

    [Serializable]
    private class VersionJsonDllEntry
    {
        public string Path;
        public string Md5;
        public long Size;
    }

    [Serializable]
    private class VersionJsonBundleEntry
    {
        public string Path;
    }
}
