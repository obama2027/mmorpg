using System.Collections.Generic;
using System.Threading;

public sealed class HttpRequestOptions
{
    public int TimeoutSeconds = 10;
    public int MaxRetries = 3;
    public int RetryDelayMs = 300;
    public bool RetryOnHttp4xx = false;
    public CancellationToken CancellationToken = default;
    public readonly Dictionary<string, string> Headers = new Dictionary<string, string>();

    public static HttpRequestOptions Default()
    {
        return new HttpRequestOptions();
    }
}
