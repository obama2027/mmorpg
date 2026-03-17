using System;
using System.IO;
using UnityEngine;

namespace MMORPG.Client.Core.Bundle
{
    public static class BundlePathUtility
    {
        public static string GetRuntimePlatformName()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return "StandaloneWindows64";
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            return "StandaloneOSX";
#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            return "StandaloneLinux64";
#elif UNITY_ANDROID
            return "Android";
#elif UNITY_IOS
            return "iOS";
#else
            throw new PlatformNotSupportedException("Unsupported AssetBundle runtime platform.");
#endif
        }

        public static string GetBundleRootPath(string rootFolderName)
        {
            var path = Path.Combine(Application.streamingAssetsPath, rootFolderName, GetRuntimePlatformName());
            return Normalize(path);
        }

        public static string GetBundleFilePath(string rootFolderName, string bundleName)
        {
            var path = Path.Combine(GetBundleRootPath(rootFolderName), bundleName);
            return Normalize(path);
        }

        private static string Normalize(string path)
        {
            return path.Replace("\\", "/");
        }
    }
}
