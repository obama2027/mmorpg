using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Runs inside base.dll and updates all files except base.dll.bytes.
/// Writes local version.json only after all required files are updated successfully.
/// </summary>
public static class BaseHotUpdateEntry
{
    private const string VersionJsonFileName = "version.json";
    private const string BaseDllFileName = "base.dll.bytes";
    private const string DefaultResServerBaseUrl = "http://localhost:8080";
    private const string StageRootFolderName = ".update_stage";
    private const int MaxNetworkRetries = 3;
    private const int RetryDelayMs = 1000;
    public static event Action<float, string> OnUpdateProgress;
    public static event Action<string> OnRetryRequired;

    private static bool s_retryRequested;

    public static IEnumerator Run()
    {
        var baseUrl = DefaultResServerBaseUrl.Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
        {
            Debug.LogError("[BaseHotUpdate] Empty resServerBaseUrl.");
            yield break;
        }

        while (true)
        {
            var success = false;
            var failedRetryable = false;
            var failureMessage = string.Empty;

            yield return RunOnce(
                baseUrl,
                () => success = true,
                (msg, retryable) =>
                {
                    failureMessage = msg;
                    failedRetryable = retryable;
                });

            if (success)
            {
                yield break;
            }

            if (!failedRetryable)
            {
                Debug.LogError($"[BaseHotUpdate] Fatal error: {failureMessage}");
                yield break;
            }

            Debug.LogError($"[BaseHotUpdate] Network error after retries: {failureMessage}");
            OnRetryRequired?.Invoke(failureMessage);
            yield return WaitForRetryRequest();
        }
    }

    public static void RequestRetry()
    {
        s_retryRequested = true;
    }

    private static IEnumerator WaitForRetryRequest()
    {
        s_retryRequested = false;
        while (!s_retryRequested)
        {
            yield return null;
        }
    }

    private static IEnumerator RunOnce(string baseUrl, Action onSuccess, Action<string, bool> onFailure)
    {
        var localVersionPath = Path.Combine(Application.persistentDataPath, VersionJsonFileName);
        if (!File.Exists(localVersionPath))
        {
            onFailure?.Invoke($"Missing local version.json: {localVersionPath}", false);
            yield break;
        }

        VersionJsonManifest localManifest;
        try
        {
            localManifest = JsonUtility.FromJson<VersionJsonManifest>(File.ReadAllText(localVersionPath));
        }
        catch (Exception ex)
        {
            onFailure?.Invoke($"Read local version.json failed: {ex.Message}", false);
            yield break;
        }

        if (localManifest == null)
        {
            onFailure?.Invoke("Invalid local version.json.", false);
            yield break;
        }

        var platform = GetPlatformName();

        var serverVersion = string.Empty;
        var reqError = string.Empty;
        yield return BaseHotUpdateRemote.GetServerVersion(
            baseUrl,
            platform,
            MaxNetworkRetries,
            RetryDelayMs,
            s => serverVersion = s,
            e => reqError = e);
        if (string.IsNullOrWhiteSpace(serverVersion))
        {
            onFailure?.Invoke($"Get server version failed: {reqError}", true);
            yield break;
        }

        string serverJson = null;
        reqError = string.Empty;
        yield return BaseHotUpdateRemote.GetServerVersionJson(
            baseUrl,
            platform,
            serverVersion,
            MaxNetworkRetries,
            RetryDelayMs,
            s => serverJson = s,
            e => reqError = e);
        if (string.IsNullOrWhiteSpace(serverJson))
        {
            onFailure?.Invoke($"Get server version.json failed: {reqError}", true);
            yield break;
        }

        var serverManifest = JsonUtility.FromJson<VersionJsonManifest>(serverJson);
        if (serverManifest == null)
        {
            onFailure?.Invoke("Invalid server version.json.", false);
            yield break;
        }

        var localMap = BaseHotUpdateStaging.BuildMd5Map(localManifest);
        var serverMap = BaseHotUpdateStaging.BuildMd5Map(serverManifest);
        var needUpdate = BaseHotUpdateStaging.BuildNeedUpdateList(localMap, serverMap, BaseDllFileName);

        var stageRoot = BaseHotUpdateStaging.BuildStageRoot(Application.persistentDataPath, StageRootFolderName, serverVersion);
        BaseHotUpdateStaging.ResetStageDirectory(stageRoot);

        for (var i = 0; i < needUpdate.Count; i++)
        {
            var relPath = needUpdate[i];
            var url = $"{baseUrl}/api/downloadResFile?platform={Uri.EscapeDataString(platform)}&version={Uri.EscapeDataString(serverVersion)}&file={Uri.EscapeDataString(relPath)}";
            var stagePath = BaseHotUpdateStaging.BuildStageFilePath(stageRoot, relPath);

            var ok = false;
            var err = string.Empty;
            yield return BaseHotUpdateRemote.DownloadFileWithRetry(
                url,
                stagePath,
                MaxNetworkRetries,
                RetryDelayMs,
                fileProgress =>
                {
                    var total = needUpdate.Count <= 0 ? 1f : needUpdate.Count;
                    var overall = (i + Mathf.Clamp01(fileProgress)) / total;
                    Debug.Log($"[BaseHotUpdate] progress={overall:P1}, file={relPath}, fileProgress={fileProgress:P1}");
                    OnUpdateProgress?.Invoke(overall, relPath);
                },
                () => ok = true,
                e => err = e);
            if (!ok)
            {
                onFailure?.Invoke($"Download failed: {relPath}, error={err}", true);
                yield break;
            }
        }

        if (!BaseHotUpdateStaging.VerifyStagedFiles(needUpdate, serverMap, stageRoot, out var verifyError))
        {
            onFailure?.Invoke(verifyError, true);
            yield break;
        }

        BaseHotUpdateStaging.ApplyStagedFiles(needUpdate, stageRoot, Application.persistentDataPath, BaseDllFileName);

        // All updates done. Replace local version.json with latest server manifest.
        File.WriteAllText(localVersionPath, serverJson);
        OnUpdateProgress?.Invoke(1f, "done");
        Debug.Log("[BaseHotUpdate] progress=100.0%, file=done");
        Debug.Log($"[BaseHotUpdate] Updated files={needUpdate.Count}, version={serverVersion}");

        EnterGame();
        Debug.Log("[BaseHotUpdate] EnterGame called. Hand over to Main.cs flow.");
        onSuccess?.Invoke();
    }

    private static void EnterGame()
    {
        string MainSceneAssetPath = "Assets/GameAssets/Scenes/Main/Main.scene";
        var op = SceneManager.LoadSceneAsync(MainSceneAssetPath, LoadSceneMode.Single);
        if (op == null)
        {
            Debug.LogError($"[GameManager] Load Main.scene failed, scene={MainSceneAssetPath}");
        }
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

}
