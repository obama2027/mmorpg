using System;

namespace MMORPG.Client.Core.Bundle
{
    public sealed class BundleException : Exception
    {
        public BundleErrorCode ErrorCode { get; }
        public string Detail { get; }

        public BundleException(BundleErrorCode errorCode, string message, string detail = null, Exception inner = null)
            : base(message, inner)
        {
            ErrorCode = errorCode;
            Detail = detail;
        }

        public override string ToString()
        {
            return $"[{ErrorCode}] {Message} detail={Detail}\n{base.ToString()}";
        }

        public static BundleException InvalidAddress(string detail)
        {
            return new BundleException(BundleErrorCode.InvalidAddress, "Invalid asset or scene address.", detail);
        }

        public static BundleException SyncLoadNotAllowed(string apiName, string target)
        {
            return new BundleException(
                BundleErrorCode.SyncLoadNotAllowedInRuntime,
                $"Runtime AssetBundle mode does not allow sync api: {apiName}",
                target);
        }

        public static BundleException ManifestLoadFailed(string detail, Exception inner = null)
        {
            return new BundleException(BundleErrorCode.ManifestLoadFailed, "AssetBundleManifest load failed.", detail, inner);
        }

        public static BundleException BundleLoadFailed(string detail, Exception inner = null)
        {
            return new BundleException(BundleErrorCode.BundleLoadFailed, "AssetBundle load failed.", detail, inner);
        }

        public static BundleException AssetLoadFailed(string detail, Exception inner = null)
        {
            return new BundleException(BundleErrorCode.AssetLoadFailed, "Asset load failed.", detail, inner);
        }

        public static BundleException SceneLoadFailed(string detail, Exception inner = null)
        {
            return new BundleException(BundleErrorCode.SceneLoadFailed, "Scene load failed.", detail, inner);
        }
    }
}
