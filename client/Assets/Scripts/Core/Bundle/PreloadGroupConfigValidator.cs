#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MMORPG.Client.Core.Bundle
{
    public static class PreloadGroupConfigValidator
    {
        [MenuItem("MMORPG/Bundle/Validate Selected PreloadGroupConfig")]
        public static void ValidateSelected()
        {
            var groupConfig = Selection.activeObject as PreloadGroupConfig;
            if (groupConfig == null)
            {
                Debug.LogError("Please select a PreloadGroupConfig asset first.");
                return;
            }

            var addressConfig = FindAssetAddressConfig();
            if (addressConfig == null)
            {
                Debug.LogError("AssetAddressConfig not found in project.");
                return;
            }

            Validate(groupConfig, addressConfig, out var errors, out var warnings);

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

                throw new BundleException(BundleErrorCode.ConfigValidationFailed, "PreloadGroupConfig validation failed.", groupConfig.name);
            }

            Debug.Log($"PreloadGroupConfig validate success: {groupConfig.name}");
        }

        public static void Validate(
            PreloadGroupConfig groupConfig,
            AssetAddressConfig addressConfig,
            out List<string> errors,
            out List<string> warnings)
        {
            errors = new List<string>();
            warnings = new List<string>();

            if (groupConfig == null)
            {
                errors.Add("PreloadGroupConfig is null.");
                return;
            }

            if (addressConfig == null)
            {
                errors.Add("AssetAddressConfig is null.");
                return;
            }

            var groupKeys = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            var knownBundleNames = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

            var assetEntries = addressConfig.AssetEntries;
            for (var i = 0; i < assetEntries.Length; i++)
            {
                var entry = assetEntries[i];
                if (entry != null && !string.IsNullOrWhiteSpace(entry.BundleName))
                {
                    knownBundleNames.Add(entry.BundleName);
                }
            }

            var sceneEntries = addressConfig.SceneEntries;
            for (var i = 0; i < sceneEntries.Length; i++)
            {
                var entry = sceneEntries[i];
                if (entry != null && !string.IsNullOrWhiteSpace(entry.BundleName))
                {
                    knownBundleNames.Add(entry.BundleName);
                }
            }

            var groups = groupConfig.Groups;
            for (var i = 0; i < groups.Length; i++)
            {
                var group = groups[i];
                if (group == null)
                {
                    errors.Add($"Group entry[{i}] is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(group.GroupKey))
                {
                    errors.Add($"Group entry[{i}] GroupKey is empty.");
                    continue;
                }

                if (!groupKeys.Add(group.GroupKey))
                {
                    errors.Add($"Duplicate group key: {group.GroupKey}");
                }

                if ((group.BundleNames == null || group.BundleNames.Length == 0) &&
                    (group.AssetKeys == null || group.AssetKeys.Length == 0))
                {
                    warnings.Add($"Group={group.GroupKey} has no bundleNames and no assetKeys.");
                }

                if (group.AssetKeys != null)
                {
                    for (var j = 0; j < group.AssetKeys.Length; j++)
                    {
                        var assetKey = group.AssetKeys[j];
                        if (string.IsNullOrWhiteSpace(assetKey))
                        {
                            warnings.Add($"Group={group.GroupKey} contains empty assetKey.");
                            continue;
                        }

                        if (!addressConfig.TryGetAssetAddress(assetKey, out _))
                        {
                            errors.Add($"Group={group.GroupKey} references unknown assetKey: {assetKey}");
                        }
                    }
                }

                if (group.BundleNames != null)
                {
                    for (var j = 0; j < group.BundleNames.Length; j++)
                    {
                        var bundleName = group.BundleNames[j];
                        if (string.IsNullOrWhiteSpace(bundleName))
                        {
                            warnings.Add($"Group={group.GroupKey} contains empty bundleName.");
                            continue;
                        }

                        if (!knownBundleNames.Contains(bundleName))
                        {
                            warnings.Add($"Group={group.GroupKey} bundleName not found in address config: {bundleName}");
                        }
                    }
                }
            }
        }

        private static AssetAddressConfig FindAssetAddressConfig()
        {
            var guids = AssetDatabase.FindAssets("t:AssetAddressConfig");
            if (guids == null || guids.Length == 0)
            {
                return null;
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<AssetAddressConfig>(path);
        }
    }
}
#endif
