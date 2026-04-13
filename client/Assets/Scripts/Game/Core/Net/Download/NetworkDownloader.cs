using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

public sealed class NetworkDownloader
{
    private readonly IHttpClient _httpClient;

    public NetworkDownloader(IHttpClient httpClient = null)
    {
        _httpClient = httpClient ?? new UnityHttpClient();
    }

    public async Task<byte[]> DownloadToMemoryAsync(string url, DownloadOptions options = null)
    {
        ValidateUrl(url);
        options ??= DownloadOptions.Default();

        try
        {
            return await _httpClient.GetBytesAsync(url, options.ToHttpOptions());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpModuleException ex)
        {
            throw DownloadModuleException.DownloadFailed($"memory url={url} detail={ex.Detail}", ex);
        }
        catch (Exception ex)
        {
            throw DownloadModuleException.DownloadFailed($"memory url={url}", ex);
        }
    }

    public async Task DownloadToStreamAsync(string url, Stream stream, DownloadOptions options = null)
    {
        if (stream == null || !stream.CanWrite)
        {
            throw DownloadModuleException.InvalidPath("target stream is null or not writable.");
        }

        var bytes = await DownloadToMemoryAsync(url, options);
        await stream.WriteAsync(bytes, 0, bytes.Length);
    }

    public async Task<DownloadResult> DownloadToFileAsync(string url, string destPath, DownloadOptions options = null)
    {
        ValidateUrl(url);
        if (string.IsNullOrWhiteSpace(destPath))
        {
            throw DownloadModuleException.InvalidPath(destPath);
        }

        options ??= DownloadOptions.Default();
        string directory = Path.GetDirectoryName(destPath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw DownloadModuleException.InvalidPath(destPath);
        }

        Directory.CreateDirectory(directory);
        string tempPath = options.UseTempFile ? destPath + ".tmp" : destPath;

        Exception lastEx = null;
        int maxRetries = options.MaxRetries < 0 ? 0 : options.MaxRetries;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                await DownloadAttemptToFileAsync(url, destPath, tempPath, options);
                var fileInfo = new FileInfo(destPath);
                return new DownloadResult
                {
                    Url = url,
                    FilePath = destPath,
                    ByteCount = fileInfo.Exists ? fileInfo.Length : 0,
                    StatusCode = 200,
                };
            }
            catch (OperationCanceledException)
            {
                CleanupTempFile(tempPath, options.UseTempFile);
                throw;
            }
            catch (Exception ex)
            {
                lastEx = ex;
                bool canRetry = attempt < maxRetries;
                DownloadLogger.Warn("File", $"download failed url={url} attempt={attempt + 1}/{maxRetries + 1} retry={canRetry} ex={ex.Message}");
                CleanupTempFile(tempPath, options.UseTempFile);

                if (!canRetry)
                {
                    break;
                }

                if (options.RetryDelayMs > 0)
                {
                    await Task.Delay(options.RetryDelayMs * (attempt + 1), options.CancellationToken);
                }
            }
        }

        throw DownloadModuleException.RetryExceeded($"url={url} path={destPath}", lastEx);
    }

    private async Task DownloadAttemptToFileAsync(string url, string destPath, string tempPath, DownloadOptions options)
    {
        using (var request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerFile(tempPath);
            request.timeout = options.TimeoutSeconds;

            var operation = request.SendWebRequest();
            var ct = options.CancellationToken;
            var onProgress = options.OnProgress;

            while (!operation.isDone)
            {
                if (ct.IsCancellationRequested)
                {
                    request.Abort();
                    ct.ThrowIfCancellationRequested();
                }

                onProgress?.Invoke(request.downloadProgress);
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw DownloadModuleException.DownloadFailed($"url={url} error={request.error} result={request.result}");
            }

            onProgress?.Invoke(1f);
        }

        try
        {
            if (options.UseTempFile)
            {
                if (File.Exists(destPath))
                {
                    if (!options.Overwrite)
                    {
                        throw DownloadModuleException.IoError($"dest exists and overwrite disabled path={destPath}");
                    }
                    File.Delete(destPath);
                }
                File.Move(tempPath, destPath);
            }
        }
        catch (DownloadModuleException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw DownloadModuleException.IoError($"url={url} path={destPath}", ex);
        }
    }

    private static void CleanupTempFile(string tempPath, bool useTempFile)
    {
        if (!useTempFile)
        {
            return;
        }

        try
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
        catch
        {
            // Ignore cleanup failure, next download can overwrite.
        }
    }

    private static void ValidateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw DownloadModuleException.InvalidUrl(url);
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw DownloadModuleException.InvalidUrl(url);
        }
    }
}
