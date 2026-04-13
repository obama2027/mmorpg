using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public static class BaseHotUpdateRemote
{
    public static IEnumerator GetServerVersion(string baseUrl, string platform, int maxRetries, int retryDelayMs, Action<string> onSuccess, Action<string> onError)
    {
        var url = $"{baseUrl}/api/getResVersion?platform={Uri.EscapeDataString(platform)}";
        var ok = false;
        var content = string.Empty;
        var error = string.Empty;
        yield return GetTextWithRetry(url, maxRetries, retryDelayMs, s =>
        {
            ok = true;
            content = s;
        }, e => error = e);

        if (!ok)
        {
            onError?.Invoke(error);
            yield break;
        }

        var data = JsonUtility.FromJson<GetResVersionResponse>(content);
        if (data == null || !data.ok)
        {
            onError?.Invoke("invalid response");
            yield break;
        }

        onSuccess?.Invoke(data.version ?? string.Empty);
    }

    public static IEnumerator GetServerVersionJson(string baseUrl, string platform, string version, int maxRetries, int retryDelayMs, Action<string> onSuccess, Action<string> onError)
    {
        var url = $"{baseUrl}/api/getVersionJson?platform={Uri.EscapeDataString(platform)}&version={Uri.EscapeDataString(version)}";
        yield return GetTextWithRetry(url, maxRetries, retryDelayMs, onSuccess, onError);
    }

    public static IEnumerator DownloadFileWithRetry(string url, string path, int maxRetries, int retryDelayMs, Action<float> onProgress, Action onSuccess, Action<string> onError)
    {
        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            var ok = false;
            var error = string.Empty;
            yield return DownloadFileOnce(url, path, onProgress, () => ok = true, e => error = e);
            if (ok)
            {
                onSuccess?.Invoke();
                yield break;
            }

            var canRetry = attempt < maxRetries - 1;
            if (!canRetry)
            {
                onError?.Invoke(error);
                yield break;
            }

            yield return new WaitForSecondsRealtime(retryDelayMs / 1000f);
        }
    }

    private static IEnumerator GetTextWithRetry(string url, int maxRetries, int retryDelayMs, Action<string> onSuccess, Action<string> onError)
    {
        for (var attempt = 0; attempt < maxRetries; attempt++)
        using (var request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(request.downloadHandler.text);
                yield break;
            }

            var canRetry = attempt < maxRetries - 1;
            if (!canRetry)
            {
                onError?.Invoke(request.error);
                yield break;
            }

            yield return new WaitForSecondsRealtime(retryDelayMs / 1000f);
        }
    }

    private static IEnumerator DownloadFileOnce(string url, string path, Action<float> onProgress, Action onSuccess, Action<string> onError)
    {
        var dir = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
        {
            System.IO.Directory.CreateDirectory(dir);
        }

        using (var request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerFile(path);
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                onProgress?.Invoke(request.downloadProgress);
                yield return null;
            }

            onProgress?.Invoke(1f);
            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error);
                yield break;
            }
        }

        onSuccess?.Invoke();
    }
}
