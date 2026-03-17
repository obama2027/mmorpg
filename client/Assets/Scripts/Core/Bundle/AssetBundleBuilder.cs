#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MMORPG.Client.Core.Bundle
{
    public static class AssetBundleBuilder
    {
        private const string BundleRootFolderName = "AssetBundles";
        private const string OutputRoot = "Assets/StreamingAssets";

        [MenuItem("MMORPG/Bundle/Build Current Target")]
        public static void BuildCurrentTarget()
        {
            Build(EditorUserBuildSettings.activeBuildTarget);
        }

        [MenuItem("MMORPG/Bundle/Build Android")]
        public static void BuildAndroid()
        {
            Build(BuildTarget.Android);
        }

        [MenuItem("MMORPG/Bundle/Build iOS")]
        public static void BuildIOS()
        {
            Build(BuildTarget.iOS);
        }

        [MenuItem("MMORPG/Bundle/Build Windows64")]
        public static void BuildWindows64()
        {
            Build(BuildTarget.StandaloneWindows64);
        }

        [MenuItem("MMORPG/Bundle/Clear Current Target Output")]
        public static void ClearCurrentTargetOutput()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            var outputPath = GetOutputPath(target);

            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
                FileUtil.DeleteFileOrDirectory(outputPath + ".meta");
                AssetDatabase.Refresh();
            }

            Debug.Log($"Clear AssetBundle output completed: {outputPath}");
        }

        public static void Build(BuildTarget target)
        {
            EnsureOutputDirectory();

            var outputPath = GetOutputPath(target);

            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
                FileUtil.DeleteFileOrDirectory(outputPath + ".meta");
            }

            Directory.CreateDirectory(outputPath);

            var manifest = BuildPipeline.BuildAssetBundles(
                outputPath,
                BuildAssetBundleOptions.ChunkBasedCompression,
                target);

            AssetDatabase.Refresh();

            if (manifest == null)
            {
                Debug.LogError($"Build AssetBundle failed. target={target}, output={outputPath}");
                return;
            }

            Debug.Log($"Build AssetBundle success. target={target}, output={outputPath}");
            Debug.Log($"Manifest bundle path: {Path.Combine(outputPath, GetPlatformName(target))}");
        }

        private static void EnsureOutputDirectory()
        {
            if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets"))
            {
                AssetDatabase.CreateFolder("Assets", "StreamingAssets");
            }

            var bundleRootPath = $"{OutputRoot}/{BundleRootFolderName}";
            if (!AssetDatabase.IsValidFolder(bundleRootPath))
            {
                AssetDatabase.CreateFolder(OutputRoot, BundleRootFolderName);
            }
        }

        private static string GetOutputPath(BuildTarget target)
        {
            return Path.Combine(OutputRoot, BundleRootFolderName, GetPlatformName(target)).Replace("\\", "/");
        }

        private static string GetPlatformName(BuildTarget target)
        {
            return BuildPipeline.GetBuildTargetName(target);
        }
    }
}
#endif
