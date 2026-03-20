public enum DownloadErrorCode
{
    Unknown = 0,
    InvalidUrl = 4001,
    InvalidPath = 4002,
    DownloadFailed = 4003,
    DownloadTimeout = 4004,
    RetryExceeded = 4005,
    IoError = 4006,
    ChecksumFailed = 4007,
}
