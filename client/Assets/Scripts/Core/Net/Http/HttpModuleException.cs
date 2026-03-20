using System;

public sealed class HttpModuleException : Exception
{
    public HttpErrorCode ErrorCode { get; }
    public string Detail { get; }
    public long StatusCode { get; }

    public HttpModuleException(HttpErrorCode errorCode, string message, string detail = null, long statusCode = 0, Exception inner = null)
        : base(message, inner)
    {
        ErrorCode = errorCode;
        Detail = detail;
        StatusCode = statusCode;
    }

    public override string ToString()
    {
        return $"[{ErrorCode}] {Message} status={StatusCode} detail={Detail}\n{base.ToString()}";
    }

    public static HttpModuleException InvalidUrl(string url)
    {
        return new HttpModuleException(HttpErrorCode.InvalidUrl, "Invalid http url.", url);
    }

    public static HttpModuleException RequestFailed(string detail, long statusCode = 0, Exception inner = null)
    {
        return new HttpModuleException(HttpErrorCode.RequestFailed, "Http request failed.", detail, statusCode, inner);
    }

    public static HttpModuleException Timeout(string detail)
    {
        return new HttpModuleException(HttpErrorCode.Timeout, "Http request timeout.", detail);
    }

    public static HttpModuleException RetryExceeded(string detail, long statusCode = 0, Exception inner = null)
    {
        return new HttpModuleException(HttpErrorCode.RetryExceeded, "Http request retry exceeded.", detail, statusCode, inner);
    }
}
