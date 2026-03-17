using System;

namespace MMORPG.Client.Core.Bundle
{
    [Serializable]
    public struct LoadedBundleInfo
    {
        public string BundleName;
        public int RefCount;
        public string[] Dependencies;
    }

    [Serializable]
    public struct PreloadedBundleInfo
    {
        public string BundleName;
        public int RefCount;
    }

    [Serializable]
    public struct PreloadedAssetInfo
    {
        public string AssetKey;
        public string AssetType;
        public int RefCount;
    }

    [Serializable]
    public struct PreloadedGroupInfo
    {
        public string GroupKey;
        public string PackageName;
        public int RefCount;
        public int BundleCount;
        public int AssetCount;
    }
}
