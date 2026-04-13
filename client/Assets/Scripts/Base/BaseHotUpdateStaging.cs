using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class BaseHotUpdateStaging
{
    public static Dictionary<string, string> BuildMd5Map(VersionJsonManifest manifest)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (manifest?.Dlls != null)
        {
            for (var i = 0; i < manifest.Dlls.Count; i++)
            {
                var path = NormalizePath(manifest.Dlls[i]?.Path);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                map[path] = (manifest.Dlls[i]?.Md5 ?? string.Empty).Trim();
            }
        }

        if (manifest?.Bundles != null)
        {
            for (var i = 0; i < manifest.Bundles.Count; i++)
            {
                var path = NormalizePath(manifest.Bundles[i]?.Path);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                map[path] = (manifest.Bundles[i]?.Md5 ?? string.Empty).Trim();
            }
        }

        return map;
    }

    public static List<string> BuildNeedUpdateList(Dictionary<string, string> localMap, Dictionary<string, string> serverMap, string excludedFileName)
    {
        var result = new List<string>();
        foreach (var pair in serverMap)
        {
            var path = pair.Key;
            if (path.Equals(excludedFileName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var serverMd5 = pair.Value ?? string.Empty;
            if (!localMap.TryGetValue(path, out var localMd5) ||
                !string.Equals(localMd5 ?? string.Empty, serverMd5, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(path);
            }
        }

        return result;
    }

    public static string BuildStageRoot(string persistentDataPath, string stageRootFolderName, string serverVersion)
    {
        return Path.Combine(persistentDataPath, stageRootFolderName, serverVersion);
    }

    public static void ResetStageDirectory(string stageRoot)
    {
        if (Directory.Exists(stageRoot))
        {
            Directory.Delete(stageRoot, true);
        }

        Directory.CreateDirectory(stageRoot);
    }

    public static string BuildStageFilePath(string stageRoot, string relativePath)
    {
        return Path.Combine(stageRoot, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
    }

    public static bool VerifyStagedFiles(List<string> needUpdate, Dictionary<string, string> serverMap, string stageRoot, out string errorMessage)
    {
        for (var i = 0; i < needUpdate.Count; i++)
        {
            var relPath = needUpdate[i];
            if (!serverMap.TryGetValue(relPath, out var expectedMd5))
            {
                errorMessage = $"Missing md5 in server manifest: {relPath}";
                return false;
            }

            var stagePath = BuildStageFilePath(stageRoot, relPath);
            if (!File.Exists(stagePath))
            {
                errorMessage = $"Missing staged file: {relPath}";
                return false;
            }

            var actualMd5 = CalculateMd5(stagePath);
            if (!string.Equals(actualMd5, expectedMd5 ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = $"Md5 mismatch: {relPath}, expected={expectedMd5}, actual={actualMd5}";
                return false;
            }
        }

        errorMessage = string.Empty;
        return true;
    }

    public static void ApplyStagedFiles(List<string> needUpdate, string stageRoot, string persistentDataPath, string excludedFileName)
    {
        for (var i = 0; i < needUpdate.Count; i++)
        {
            var relPath = needUpdate[i];
            if (relPath.Equals(excludedFileName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var stagePath = BuildStageFilePath(stageRoot, relPath);
            var livePath = Path.Combine(persistentDataPath, relPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
            var liveDir = Path.GetDirectoryName(livePath);
            if (!string.IsNullOrEmpty(liveDir))
            {
                Directory.CreateDirectory(liveDir);
            }

            File.Copy(stagePath, livePath, true);
        }
    }

    private static string NormalizePath(string value)
    {
        return (value ?? string.Empty).Replace("\\", "/").Trim();
    }

    private static string CalculateMd5(string filePath)
    {
        using (var md5 = MD5.Create())
        using (var stream = File.OpenRead(filePath))
        {
            var hash = md5.ComputeHash(stream);
            var sb = new StringBuilder(hash.Length * 2);
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
