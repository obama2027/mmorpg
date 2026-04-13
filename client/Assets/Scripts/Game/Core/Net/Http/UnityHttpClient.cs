using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

public sealed class UnityHttpClient : IHttpClient
{
    public async Task<HttpResponseData> SendAsync(string method, string url, byte[] body = null, string contentType = null, HttpRequestOptions options = null)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw HttpModuleException.InvalidUrl(url);
        }

        options ??= HttpRequestOptions.Default();
        int maxRetries = options.MaxRetries < 0 ? 0 : options.MaxRetries;
        Exception lastEx = null;
        long lastStatusCode = 0;
        string lastError = string.Empty;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            using (var request = CreateRequest(method, url, body, contentType, options))
            {
                try
                {
                    var operation = request.SendWebRequest();
                    var ct = options.CancellationToken;
                    while (!operation.isDone)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            request.Abort();
                            ct.ThrowIfCancellationRequested();
                        }
                        await Task.Yield();
                    }

                    lastStatusCode = request.responseCode;
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        return new HttpResponseData
                        {
                            StatusCode = request.responseCode,
                            Text = request.downloadHandler != null ? request.downloadHandler.text : string.Empty,
                            Bytes = request.downloadHandler != null ? request.downloadHandler.data : Array.Empty<byte>(),
                            Headers = request.GetResponseHeaders() ?? new Dictionary<string, string>(),
                            Url = request.url,
                        };
                    }

                    lastError = request.error;
                    bool shouldRetry = attempt < maxRetries && HttpRetryPolicy.ShouldRetry(request, options.RetryOnHttp4xx);
                    HttpLogger.Warn("Client", $"request failed method={method} url={url} status={request.responseCode} error={request.error} attempt={attempt + 1}/{maxRetries + 1} retry={shouldRetry}");

                    if (!shouldRetry)
                    {
                        throw HttpModuleException.RequestFailed(
                            $"method={method} url={url} error={request.error} result={request.result}",
                            request.responseCode);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (HttpModuleException ex)
                {
                    lastEx = ex;
                    if (attempt >= maxRetries)
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    HttpLogger.Warn("Client", $"request exception method={method} url={url} ex={ex.Message} attempt={attempt + 1}/{maxRetries + 1}");
                    if (attempt >= maxRetries)
                    {
                        throw HttpModuleException.RequestFailed($"method={method} url={url}", lastStatusCode, ex);
                    }
                }
            }

            if (options.RetryDelayMs > 0)
            {
                await Task.Delay(options.RetryDelayMs * (attempt + 1), options.CancellationToken);
            }
        }

        throw HttpModuleException.RetryExceeded(
            $"method={method} url={url} status={lastStatusCode} error={lastError}",
            lastStatusCode,
            lastEx);
    }

    public async Task<byte[]> GetBytesAsync(string url, HttpRequestOptions options = null)
    {
        var response = await SendAsync(UnityWebRequest.kHttpVerbGET, url, null, null, options);
        return response.Bytes ?? Array.Empty<byte>();
    }

    public async Task<string> GetStringAsync(string url, HttpRequestOptions options = null)
    {
        var response = await SendAsync(UnityWebRequest.kHttpVerbGET, url, null, null, options);
        return response.Text ?? string.Empty;
    }

    public async Task<string> PostJsonAsync(string url, string jsonBody, HttpRequestOptions options = null)
    {
        byte[] body = string.IsNullOrEmpty(jsonBody) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(jsonBody);
        var response = await SendAsync(UnityWebRequest.kHttpVerbPOST, url, body, "application/json", options);
        return response.Text ?? string.Empty;
    }

    public async Task<string> PostFormAsync(string url, string formData, HttpRequestOptions options = null)
    {
        byte[] body = string.IsNullOrEmpty(formData) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(formData);
        var response = await SendAsync(UnityWebRequest.kHttpVerbPOST, url, body, "application/x-www-form-urlencoded", options);
        return response.Text ?? string.Empty;
    }

    public async Task<HttpResponseData> PostAsync(string url, byte[] body, string contentType = null, HttpRequestOptions options = null)
    {
        return await SendAsync(UnityWebRequest.kHttpVerbPOST, url, body, contentType ?? "application/octet-stream", options);
    }

    private static UnityWebRequest CreateRequest(string method, string url, byte[] body, string contentType, HttpRequestOptions options)
    {
        var request = new UnityWebRequest(url, method ?? UnityWebRequest.kHttpVerbGET);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.timeout = options.TimeoutSeconds;

        if (body != null && body.Length > 0)
        {
            request.uploadHandler = new UploadHandlerRaw(body);
        }

        if (!string.IsNullOrWhiteSpace(contentType))
        {
            request.SetRequestHeader("Content-Type", contentType);
        }

        foreach (var pair in options.Headers)
        {
            request.SetRequestHeader(pair.Key, pair.Value);
        }

        return request;
    }
}
