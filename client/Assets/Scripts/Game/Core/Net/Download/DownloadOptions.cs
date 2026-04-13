using System;
using System.Threading;

public sealed class DownloadOptions
{
    public int TimeoutSeconds = 10;
    public int MaxRetries = 3;
    public int RetryDelayMs = 300;
    public bool RetryOnHttp4xx = false;
    public bool Overwrite = true;
    public bool UseTempFile = true;
    public CancellationToken CancellationToken = default;
    public Action<float> OnProgress;

    public static DownloadOptions Default()
    {
        return new DownloadOptions();
    }

    public HttpRequestOptions ToHttpOptions()
    {
        return new HttpRequestOptions
        {
            TimeoutSeconds = TimeoutSeconds,
            MaxRetries = MaxRetries,
            RetryDelayMs = RetryDelayMs,
            RetryOnHttp4xx = RetryOnHttp4xx,
            CancellationToken = CancellationToken,
        };
    }
}
