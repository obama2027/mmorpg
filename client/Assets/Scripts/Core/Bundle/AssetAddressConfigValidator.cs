#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MMORPG.Client.Core.Bundle
{
    public static class AssetAddressConfigValidator
    {
        [MenuItem("MMORPG/Bundle/Validate Selected AssetAddressConfig")]
        public static void ValidateSelected()
        {
            var config = Selection.activeObject as AssetAddressConfig;
            if (config == null)
            {
                Debug.LogError("Please select an AssetAddressConfig asset first.");
                return;
            }

            Validate(config, out var errors, out var warnings);

            foreach (var warning in warnings)
            {
                Debug.LogWarning(warning);
            }

            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    Debug.LogError(error);
                }

                throw new BundleException(BundleErrorCode.ConfigValidationFailed, "AssetAddressConfig validation failed.", config.name);
            }

            Debug.Log($"AssetAddressConfig validate success: {config.name}");
        }

        public static void Validate(AssetAddressConfig config, out List<string> errors, out List<string> warnings)
        {
            errors = new List<string>();
            warnings = new List<string>();

            if (config == null)
            {
                errors.Add("AssetAddressConfig is null.");
                return;
            }

            var assetKeys = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            var sceneKeys = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

            var assetEntries = config.AssetEntries;
            for (var i = 0; i < assetEntries.Length; i++)
            {
                var entry = assetEntries[i];
                if (entry == null)
                {
                    errors.Add($"Asset entry[{i}] is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.Key))
                {
                    errors.Add($"Asset entry[{i}] key is empty.");
                }
                else if (!assetKeys.Add(entry.Key))
                {
                    errors.Add($"Duplicate asset key: {entry.Key}");
                }

                if (string.IsNullOrWhiteSpace(entry.BundleName))
                {
                    errors.Add($"Asset key={entry.Key} bundleName is empty.");
                }

                if (string.IsNullOrWhiteSpace(entry.AssetName))
                {
                    errors.Add($"Asset key={entry.Key} assetName is empty.");
                }

                if (!string.IsNullOrWhiteSpace(entry.EditorAssetPath))
                {
                    var obj = AssetDatabase.LoadMainAssetAtPath(entry.EditorAssetPath);
                    if (obj == null)
                    {
                        errors.Add($"Asset key={entry.Key} invalid EditorAssetPath: {entry.EditorAssetPath}");
                    }
                }
                else
                {
                    warnings.Add($"Asset key={entry.Key} EditorAssetPath is empty.");
                }
            }

            var sceneEntries = config.SceneEntries;
            for (var i = 0; i < sceneEntries.Length; i++)
            {
                var entry = sceneEntries[i];
                if (entry == null)
                {
                    errors.Add($"Scene entry[{i}] is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.Key))
                {
                    errors.Add($"Scene entry[{i}] key is empty.");
                }
                else if (!sceneKeys.Add(entry.Key))
                {
                    errors.Add($"Duplicate scene key: {entry.Key}");
                }

                if (string.IsNullOrWhiteSpace(entry.BundleName))
                {
                    errors.Add($"Scene key={entry.Key} bundleName is empty.");
                }

                if (string.IsNullOrWhiteSpace(entry.ScenePath))
                {
                    errors.Add($"Scene key={entry.Key} scenePath is empty.");
                }

                if (!string.IsNullOrWhiteSpace(entry.EditorScenePath))
                {
                    var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(entry.EditorScenePath);
                    if (sceneAsset == null)
                    {
                        errors.Add($"Scene key={entry.Key} invalid EditorScenePath: {entry.EditorScenePath}");
                    }
                }
                else
                {
                    warnings.Add($"Scene key={entry.Key} EditorScenePath is empty.");
                }
            }
        }
    }
}
#endif
