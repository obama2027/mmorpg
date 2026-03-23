#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using UnityEditor;
using UnityEngine;

public sealed class BuildResWindow : EditorWindow
{
    private sealed class BuildStep
    {
        public string Name;
        public bool Enabled;
        public Action Run;
    }

    private readonly List<BuildStep> _steps = new List<BuildStep>();
    private Vector2 _scroll;
    private bool _isRunning;

    [MenuItem("Tools/Build Res")]
    public static void Open()
    {
        var window = GetWindow<BuildResWindow>("Build Res");
        window.minSize = new Vector2(520f, 260f);
        window.Show();
    }

    private void OnEnable()
    {
        _steps.Clear();
        _steps.Add(new BuildStep
        {
            Name = "update proto",
            Enabled = true,
            Run = RunUpdateProto,
        });
        _steps.Add(new BuildStep
        {
            Name = "update dll",
            Enabled = true,
            Run = RunUpdateDll,
        });
        _steps.Add(new BuildStep
        {
            Name = "update bundle name",
            Enabled = true,
            Run = RunUpdateBundleName,
        });
        _steps.Add(new BuildStep
        {
            Name = "update res",
            Enabled = true,
            Run = RunUpdateRes,
        });
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Build Steps", EditorStyles.boldLabel);
        EditorGUILayout.Space(6f);

        using (new EditorGUI.DisabledScope(_isRunning))
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var step in _steps)
            {
                DrawStepRow(step);
            }
            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            EditorGUILayout.Space(8f);

            if (GUILayout.Button("Build", GUILayout.Height(32f)))
            {
                RunSelectedStepsInOrder();
            }

            if (GUILayout.Button("UpLoad", GUILayout.Height(32f)))
            {
                RunUpLoadFlow();
            }
        }
    }

    private void RunUpLoadFlow()
    {
        try
        {
            _isRunning = true;
            RunUpLoadInternal();
            Debug.Log("[BuildRes] UpLoad completed.");
            EditorUtility.DisplayDialog("Build Res", "UpLoad completed.", "OK");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("Build Res", $"UpLoad failed:\n{ex.Message}", "OK");
        }
        finally
        {
            _isRunning = false;
            Repaint();
        }
    }

    private static void DrawStepRow(BuildStep step)
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            step.Enabled = EditorGUILayout.Toggle(step.Enabled, GUILayout.Width(20f));
            EditorGUILayout.LabelField(step.Name, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Run", GUILayout.Width(96f)))
            {
                try
                {
                    step.Run?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    EditorUtility.DisplayDialog("Build Res", $"Step failed: {step.Name}\n{ex.Message}", "OK");
                }
            }
        }
    }

    private void RunSelectedStepsInOrder()
    {
        try
        {
            _isRunning = true;
            foreach (var step in _steps)
            {
                if (!step.Enabled)
                {
                    continue;
                }

                Debug.Log($"[BuildRes] Running step: {step.Name}");
                step.Run?.Invoke();
            }

            Debug.Log("[BuildRes] Build completed.");
            EditorUtility.DisplayDialog("Build Res", "Build completed.", "OK");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("Build Res", $"Build failed:\n{ex.Message}", "OK");
        }
        finally
        {
            _isRunning = false;
            Repaint();
        }
    }

    private static void RunUpdateProto()
    {
        // MMORPG/Generate Protocol
        ProtocBuild.GenerateAll();
    }

    private static void RunUpdateDll()
    {
        EnsureStreamingAssetsPlatformDirectory();
        var executed = EditorApplication.ExecuteMenuItem("HybridCLR/CompileDll/ActiveBuildTarget");
        if (!executed)
        {
            throw new InvalidOperationException("Cannot execute menu: HybridCLR/CompileDll/ActiveBuildTarget");
        }

        CopyHotUpdateDllsToStreamingAssets();
        var platformName = GetPlatformName(EditorUserBuildSettings.activeBuildTarget);
        var platformDir = Path.Combine(Application.dataPath, "StreamingAssets", platformName);
        AssetBundleBuilder.UpdateVersionJsonWithDlls(platformDir);
        AssetDatabase.Refresh();
    }

    private static void RunUpdateRes()
    {
        EnsureStreamingAssetsPlatformDirectory();
        AssetBundleBuilder.BuildCurrentTarget();
    }

    private static void RunUpdateBundleName()
    {
        var executed = EditorApplication.ExecuteMenuItem("MMORPG/Bundle/Auto Set Bundle Names");
        if (!executed)
        {
            throw new InvalidOperationException("Cannot execute menu: MMORPG/Bundle/Auto Set Bundle Names");
        }
    }

    private static void EnsureStreamingAssetsPlatformDirectory()
    {
        var target = EditorUserBuildSettings.activeBuildTarget;
        var platformName = GetPlatformName(target);
        var platformDir = Path.Combine(Application.dataPath, "StreamingAssets", platformName);
        if (!Directory.Exists(platformDir))
        {
            Directory.CreateDirectory(platformDir);
            AssetDatabase.Refresh();
        }
    }

    private static void CopyHotUpdateDllsToStreamingAssets()
    {
        var target = EditorUserBuildSettings.activeBuildTarget;
        var platformName = GetPlatformName(target);
        var targetDir = Path.Combine("Assets/StreamingAssets", platformName).Replace("\\", "/");
        Directory.CreateDirectory(targetDir);

        var hotUpdateOutputDir = Path.Combine("HybridCLRData/HotUpdateDlls", platformName).Replace("\\", "/");
        if (!Directory.Exists(hotUpdateOutputDir))
        {
            throw new DirectoryNotFoundException(
                $"Hot update dll output path not found: {hotUpdateOutputDir}");
        }

        CopyOneDll(hotUpdateOutputDir, targetDir, "Base");
        CopyOneDll(hotUpdateOutputDir, targetDir, "Game");
        AssetDatabase.Refresh();
    }

    private static void CopyOneDll(string sourceDir, string targetDir, string assemblyName)
    {
        var dllPath = Path.Combine(sourceDir, $"{assemblyName}.dll");
        if (!File.Exists(dllPath))
        {
            throw new FileNotFoundException($"Missing dll: {dllPath}");
        }

        var dstBytesPath = Path.Combine(targetDir, $"{assemblyName.ToLowerInvariant()}.dll.bytes");
        File.Copy(dllPath, dstBytesPath, true);
        Debug.Log($"[BuildRes] Copied {assemblyName}.dll -> {dstBytesPath}");
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

    private static void RunUpLoadInternal()
    {
        EnsureStreamingAssetsPlatformDirectory();

        var target = EditorUserBuildSettings.activeBuildTarget;
        var platformName = GetPlatformName(target);

        var phpServerBaseUrl = "http://localhost:8080";
        var srcPlatformDir = Path.Combine(Application.dataPath, "StreamingAssets", platformName);
        var versionJsonPath = Path.Combine(srcPlatformDir, "version.json");
        if (!File.Exists(versionJsonPath))
        {
            throw new FileNotFoundException(
                $"Missing version.json: {versionJsonPath}. Please run update dll/res first.");
        }

        var localManifest = ReadVersionJson(versionJsonPath);
        if (localManifest == null || string.IsNullOrWhiteSpace(localManifest.Version))
        {
            throw new InvalidOperationException($"Invalid version.json: {versionJsonPath}");
        }

        // Case B: if server has version.txt, it returns nextVersion; we must update local version.json first.
        var nextVersionInfo = GetNextResVersionFromServer(phpServerBaseUrl, platformName);
        if (nextVersionInfo.hasVersionTxt && !string.IsNullOrWhiteSpace(nextVersionInfo.nextVersion))
        {
            localManifest.Version = nextVersionInfo.nextVersion.Trim();
            localManifest.BuildTime = DateTime.UtcNow.ToString("O");
            WriteVersionJson(versionJsonPath, localManifest);
            AssetDatabase.Refresh();
        }

        var zipPath = CreateZipOfStreamingAssetsPlatform(srcPlatformDir, platformName);
        try
        {
            var uploadResp = UploadResZipToServer(phpServerBaseUrl, platformName, zipPath);
            if (uploadResp != null && !string.IsNullOrWhiteSpace(uploadResp.version))
            {
                // Ensure local manifest version matches server's final version.
                var finalVersion = uploadResp.version.Trim();
                if (!string.Equals(localManifest.Version, finalVersion, System.StringComparison.Ordinal))
                {
                    localManifest.Version = finalVersion;
                    localManifest.BuildTime = DateTime.UtcNow.ToString("O");
                    WriteVersionJson(versionJsonPath, localManifest);
                    AssetDatabase.Refresh();
                }
            }

            Debug.Log(
                $"[BuildRes] UpLoad done. platform={platformName}, localVersion={localManifest.Version}, zip={Path.GetFileName(zipPath)}");
        }
        finally
        {
            try
            {
                File.Delete(zipPath);
            }
            catch
            {
                // ignore
            }
        }
    }

    private static NextResVersionResponse GetNextResVersionFromServer(string baseUrl, string platformName)
    {
        var url = $"{baseUrl}/api/getNextResVersion?platform={Uri.EscapeDataString(platformName)}";
        using (var client = new HttpClient())
        {
            var json = client.GetStringAsync(url).GetAwaiter().GetResult();
            var resp = JsonUtility.FromJson<NextResVersionResponse>(json);
            if (resp == null || !resp.ok)
            {
                throw new InvalidOperationException($"GetNextResVersion failed. json={json}");
            }

            return resp;
        }
    }

    private static UploadResResponse UploadResZipToServer(string baseUrl, string platformName, string zipPath)
    {
        var url = $"{baseUrl}/api/uploadRes?platform={Uri.EscapeDataString(platformName)}";
        var bytes = File.ReadAllBytes(zipPath);
        using (var client = new HttpClient())
        using (var content = new ByteArrayContent(bytes))
        {
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip");
            var resp = client.PostAsync(url, content).GetAwaiter().GetResult();
            var json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!resp.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"UploadResZipToServer failed. status={resp.StatusCode}, json={json}");
            }

            var parsed = JsonUtility.FromJson<UploadResResponse>(json);
            if (parsed == null || !parsed.ok)
            {
                throw new InvalidOperationException($"UploadResZipToServer invalid response. json={json}");
            }

            return parsed;
        }
    }

    private static string CreateZipOfStreamingAssetsPlatform(string srcPlatformDir, string platformName)
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "mmorpg_upload_" + platformName + "_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        var zipPath = Path.Combine(tmpDir, "res.zip");

        var files = Directory.GetFiles(srcPlatformDir, "*", SearchOption.AllDirectories);
        using (var zipStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
        {
            for (var i = 0; i < files.Length; i++)
            {
                var filePath = files[i];
                var fileName = Path.GetFileName(filePath);
                if (fileName.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var relativePath = filePath.Substring(srcPlatformDir.Length)
                    .TrimStart(Path.DirectorySeparatorChar, '/', '\\')
                    .Replace("\\", "/");

                var entry = archive.CreateEntry(relativePath, System.IO.Compression.CompressionLevel.Optimal);
                using (var entryStream = entry.Open())
                using (var sourceStream = File.OpenRead(filePath))
                {
                    sourceStream.CopyTo(entryStream);
                }
            }
        }

        return zipPath;
    }

    private static string IncrementVersionLastSegment(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return "1.0.0";
        }

        var parts = version.Trim().Split('.');
        if (parts.Length == 0)
        {
            return "1.0.0";
        }

        var lastIndex = parts.Length - 1;
        if (int.TryParse(parts[lastIndex], out var last))
        {
            parts[lastIndex] = (last + 1).ToString();
        }
        else
        {
            parts[lastIndex] = "1";
        }

        return string.Join(".", parts);
    }

    [Serializable]
    private sealed class NextResVersionResponse
    {
        public bool ok;
        public string platform;
        public bool hasVersionTxt;
        public string currentVersion;
        public string nextVersion;
    }

    [Serializable]
    private sealed class UploadResResponse
    {
        public bool ok;
        public string platform;
        public string version;
    }

    private static void WriteVersionJson(string versionJsonPath, VersionJsonManifest manifest)
    {
        var json = JsonUtility.ToJson(manifest, true);
        File.WriteAllText(versionJsonPath, json, System.Text.Encoding.UTF8);
    }

    private static string GetWorkspaceRootPath()
    {
        // Application.dataPath: <workspace>/client/Assets
        var clientDir = Directory.GetParent(Application.dataPath).FullName;
        var workspaceRoot = Directory.GetParent(clientDir).FullName;
        return workspaceRoot;
    }

    private static VersionJsonManifest ReadVersionJson(string versionJsonPath)
    {
        var json = File.ReadAllText(versionJsonPath);
        return JsonUtility.FromJson<VersionJsonManifest>(json);
    }

    [Serializable]
    private sealed class VersionJsonManifest
    {
        public string Platform;
        public string Version;
        public string BuildTime;
        public List<VersionJsonDllEntry> Dlls;
        public List<VersionJsonBundleEntry> Bundles;
    }

    [Serializable]
    private sealed class VersionJsonDllEntry
    {
        public string Path;
        public string Md5;
        public long Size;
    }

    [Serializable]
    private sealed class VersionJsonBundleEntry
    {
        public string Path;
        public string Md5;
        public long Size;
        public string Group;
    }
}
#endif
