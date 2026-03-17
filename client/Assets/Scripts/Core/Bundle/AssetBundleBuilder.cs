#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class AssetBundleBuilder
{
    private const string OutputRoot = "Assets/StreamingAssets";
    private const string BundleVersionManifestFileName = "bundle_manifest.json";
    private const string BundleVersion = "1.0.1";
    private const string DefaultBundleGroup = "Main";

    [MenuItem("MMORPG/Bundle/Auto Set Bundle Names")]
    public static void AutoSetBundleNames()
    {
        var buildRoots = LoadBuildRoots();
        if (buildRoots.Count == 0)
        {
            Debug.LogWarning($"No build roots found in {BuildConfig.AssetPath}.");
            return;
        }

        var updatedCount = 0;
        foreach (var configuredRoot in buildRoots)
        {
            updatedCount += ApplyBundleNamesForRoot(configuredRoot);
        }

        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Auto set bundle names completed. updatedAssets={updatedCount}");
    }

    [MenuItem("MMORPG/Bundle/Build Current Target")]
    public static void BuildCurrentTarget()
    {
        Build(EditorUserBuildSettings.activeBuildTarget);
    }

    [MenuItem("MMORPG/Bundle/Rebuild Current Target")]
    public static void RebuildCurrentTarget()
    {
        Rebuild(EditorUserBuildSettings.activeBuildTarget);
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
        BuildInternal(target, cleanOutput: false);
    }

    public static void Rebuild(BuildTarget target)
    {
        BuildInternal(target, cleanOutput: true);
    }

    private static void BuildInternal(BuildTarget target, bool cleanOutput)
    {
        EnsureOutputDirectory();

        var outputPath = GetOutputPath(target);

        if (cleanOutput && Directory.Exists(outputPath))
        {
            Directory.Delete(outputPath, true);
            FileUtil.DeleteFileOrDirectory(outputPath + ".meta");
        }

        Directory.CreateDirectory(outputPath);

        var manifest = BuildPipeline.BuildAssetBundles(
            outputPath,
            BuildAssetBundleOptions.ChunkBasedCompression,
            target);

        if (manifest == null)
        {
            AssetDatabase.Refresh();
            Debug.LogError($"Build AssetBundle failed. target={target}, output={outputPath}");
            return;
        }

        CleanupStaleBundleFiles(outputPath, target);
        GenerateBundleVersionManifest(outputPath, target);
        AssetDatabase.Refresh();

        Debug.Log($"Build AssetBundle success. target={target}, output={outputPath}, cleanOutput={cleanOutput}");
        Debug.Log($"Manifest bundle path: {Path.Combine(outputPath, GetPlatformName(target))}");
    }

    private static int ApplyBundleNamesForRoot(string configuredRoot)
    {
        var rootAssetPath = ToAssetPath(configuredRoot);
        if (!AssetDatabase.IsValidFolder(rootAssetPath))
        {
            Debug.LogWarning($"Build root folder not found: {configuredRoot}");
            return 0;
        }

        var updatedCount = 0;
        var firstLevelFolders = AssetDatabase.GetSubFolders(rootAssetPath);
        foreach (var firstLevelFolder in firstLevelFolders)
        {
            var folderName = Path.GetFileName(firstLevelFolder);
            var bundleName = BundlePathUtility.GetRuntimeBundleName(CombineBundleName(configuredRoot, folderName));
            updatedCount += ApplyBundleNameForSearchFilter(firstLevelFolder, bundleName, "t:Prefab");
            updatedCount += ApplyBundleNameForSearchFilter(firstLevelFolder, bundleName, "t:Scene");
        }

        return updatedCount;
    }

    private static int ApplyBundleNameForSearchFilter(string searchFolder, string bundleName, string filter)
    {
        var updatedCount = 0;
        var assetGuids = AssetDatabase.FindAssets(filter, new[] { searchFolder });
        foreach (var guid in assetGuids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null || importer.assetBundleName == bundleName)
            {
                continue;
            }

            importer.assetBundleName = bundleName;
            updatedCount++;
        }

        return updatedCount;
    }

    private static List<string> LoadBuildRoots()
    {
        var config = AssetDatabase.LoadAssetAtPath<BuildConfig>(BuildConfig.AssetPath);
        if (config == null)
        {
            Debug.LogError($"BuildConfig not found: {BuildConfig.AssetPath}");
            return new List<string>();
        }

        var serializedObject = new SerializedObject(config);
        var buildRootListProperty = serializedObject.FindProperty("buildRootList");
        if (buildRootListProperty == null || !buildRootListProperty.isArray)
        {
            Debug.LogError("BuildConfig.buildRootList not found.");
            return new List<string>();
        }

        var result = new List<string>();
        for (var i = 0; i < buildRootListProperty.arraySize; i++)
        {
            var element = buildRootListProperty.GetArrayElementAtIndex(i);
            var value = element.stringValue?.Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            result.Add(value.Replace("\\", "/").Trim('/'));
        }

        return result;
    }

    private static string ToAssetPath(string configuredRoot)
    {
        var normalized = configuredRoot.Replace("\\", "/").Trim('/');
        if (normalized.StartsWith("Assets/"))
        {
            return normalized;
        }

        return $"Assets/{normalized}";
    }

    private static string CombineBundleName(string configuredRoot, string folderName)
    {
        var normalizedRoot = configuredRoot.Replace("\\", "/").Trim('/');
        var normalizedFolderName = folderName.Replace("\\", "/").Trim('/');
        return $"{normalizedRoot}/{normalizedFolderName}";
    }

    private static void CleanupStaleBundleFiles(string outputPath, BuildTarget target)
    {
        if (!Directory.Exists(outputPath))
        {
            return;
        }

        var platformName = GetPlatformName(target);
        var expectedFiles = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            platformName,
            $"{platformName}.manifest",
        };

        var bundleNames = AssetDatabase.GetAllAssetBundleNames();
        foreach (var bundleName in bundleNames)
        {
            var runtimeBundleName = BundlePathUtility.GetRuntimeBundleName(bundleName);
            expectedFiles.Add(runtimeBundleName);
            expectedFiles.Add($"{runtimeBundleName}.manifest");
        }

        var files = Directory.GetFiles(outputPath);
        for (var i = 0; i < files.Length; i++)
        {
            var filePath = files[i];
            var fileName = Path.GetFileName(filePath);
            if (fileName.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (expectedFiles.Contains(fileName))
            {
                continue;
            }

            DeleteOutputFileWithMeta(filePath);
        }
    }

    private static void GenerateBundleVersionManifest(string outputPath, BuildTarget target)
    {
        var manifest = new BundleVersionManifest
        {
            Platform = GetPlatformName(target),
            Version = BundleVersion,
            BuildTime = DateTime.UtcNow.ToString("O"),
            Bundles = new List<BundleVersionEntry>()
        };

        var bundleFiles = Directory.GetFiles(outputPath, "*.bundle");
        System.Array.Sort(bundleFiles, System.StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < bundleFiles.Length; i++)
        {
            var bundleFilePath = bundleFiles[i];
            var fileInfo = new FileInfo(bundleFilePath);
            manifest.Bundles.Add(new BundleVersionEntry
            {
                BundleName = fileInfo.Name,
                Md5 = CalculateFileMd5(bundleFilePath),
                Size = fileInfo.Length,
                Group = DefaultBundleGroup,
            });
        }

        var manifestFilePath = Path.Combine(outputPath, BundleVersionManifestFileName);
        var json = JsonUtility.ToJson(manifest, true);
        File.WriteAllText(manifestFilePath, json, Encoding.UTF8);
    }

    private static void EnsureOutputDirectory()
    {
        if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets"))
        {
            AssetDatabase.CreateFolder("Assets", "StreamingAssets");
        }
    }

    private static string GetOutputPath(BuildTarget target)
    {
        return Path.Combine(OutputRoot, GetPlatformName(target)).Replace("\\", "/");
    }

    private static string GetPlatformName(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.StandaloneWindows64:
                return "StandaloneWindows64";
            case BuildTarget.StandaloneOSX:
                return "StandaloneOSX";
            case BuildTarget.StandaloneLinux64:
                return "StandaloneLinux64";
            case BuildTarget.Android:
                return "Android";
            case BuildTarget.iOS:
                return "iOS";
            default:
                return BuildPipeline.GetBuildTargetName(target);
        }
    }

    private static void DeleteOutputFileWithMeta(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        var metaPath = filePath + ".meta";
        if (File.Exists(metaPath))
        {
            File.Delete(metaPath);
        }
    }

    private static string CalculateFileMd5(string filePath)
    {
        using (var md5 = MD5.Create())
        using (var stream = File.OpenRead(filePath))
        {
            var hashBytes = md5.ComputeHash(stream);
            var builder = new StringBuilder(hashBytes.Length * 2);
            for (var i = 0; i < hashBytes.Length; i++)
            {
                builder.Append(hashBytes[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }

    [Serializable]
    private sealed class BundleVersionManifest
    {
        public string Platform;
        public string Version;
        public string BuildTime;
        public List<BundleVersionEntry> Bundles;
    }

    [Serializable]
    private sealed class BundleVersionEntry
    {
        public string BundleName;
        public string Md5;
        public long Size;
        public string Group;
    }

}
#endif
