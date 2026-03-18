#if UNITY_EDITOR
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ProtocBuild
{
    private const string ProtocPath = "Tools/protoc/protoc.exe";
    private const string ProtoDir = "../proto";
    private const string CSharpOutDir = "Assets/Scripts/Proto";

    [MenuItem("MMORPG/Generate Protocol")]
    public static void GenerateAll()
    {
        var protocFullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ProtocPath));
        var protoFullDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ProtoDir));
        var outFullDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", CSharpOutDir));

        if (!File.Exists(protocFullPath))
        {
            EditorUtility.DisplayDialog("Generate Protocol", $"protoc not found:\n{protocFullPath}", "OK");
            return;
        }

        if (!Directory.Exists(protoFullDir))
        {
            EditorUtility.DisplayDialog("Generate Protocol", $"proto directory not found:\n{protoFullDir}", "OK");
            return;
        }

        if (!Directory.Exists(outFullDir))
        {
            Directory.CreateDirectory(outFullDir);
        }

        var protoFiles = Directory.GetFiles(protoFullDir, "*.proto", SearchOption.TopDirectoryOnly);
        if (protoFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("Generate Protocol", "No .proto files found.", "OK");
            return;
        }

        int successCount = 0;
        int failCount = 0;

        foreach (var protoFile in protoFiles)
        {
            var fileName = Path.GetFileName(protoFile);
            var args = $"--proto_path=\"{protoFullDir}\" --csharp_out=\"{outFullDir}\" \"{protoFile}\"";

            var psi = new ProcessStartInfo
            {
                FileName = protocFullPath,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using (var process = Process.Start(psi))
            {
                var stdout = process.StandardOutput.ReadToEnd();
                var stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    successCount++;
                    UnityEngine.Debug.Log($"[ProtocBuild] Generated: {fileName}");
                }
                else
                {
                    failCount++;
                    UnityEngine.Debug.LogError($"[ProtocBuild] Failed: {fileName}\n{stderr}");
                }
            }
        }

        AssetDatabase.Refresh();

        var message = $"Done. success={successCount} fail={failCount}\nOutput: {CSharpOutDir}";
        if (failCount > 0)
        {
            EditorUtility.DisplayDialog("Generate Protocol", message, "OK");
        }
        else
        {
            UnityEngine.Debug.Log($"[ProtocBuild] {message}");
        }
    }
}
#endif
