using System;

public sealed class DownloadModuleException : Exception
{
    public DownloadErrorCode ErrorCode { get; }
    public string Detail { get; }

    public DownloadModuleException(DownloadErrorCode errorCode, string message, string detail = null, Exception inner = null)
        : base(message, inner)
    {
        ErrorCode = errorCode;
        Detail = detail;
    }

    public override string ToString()
    {
        return $"[{ErrorCode}] {Message} detail={Detail}\n{base.ToString()}";
    }

    public static DownloadModuleException InvalidUrl(string detail)
    {
        return new DownloadModuleException(DownloadErrorCode.InvalidUrl, "Invalid download url.", detail);
    }

    public static DownloadModuleException InvalidPath(string detail)
    {
        return new DownloadModuleException(DownloadErrorCode.InvalidPath, "Invalid download path.", detail);
    }

    public static DownloadModuleException DownloadFailed(string detail, Exception inner = null)
    {
        return new DownloadModuleException(DownloadErrorCode.DownloadFailed, "Download failed.", detail, inner);
    }

    public static DownloadModuleException RetryExceeded(string detail, Exception inner = null)
    {
        return new DownloadModuleException(DownloadErrorCode.RetryExceeded, "Download retry exceeded.", detail, inner);
    }

    public static DownloadModuleException IoError(string detail, Exception inner = null)
    {
        return new DownloadModuleException(DownloadErrorCode.IoError, "Download io error.", detail, inner);
    }
}
