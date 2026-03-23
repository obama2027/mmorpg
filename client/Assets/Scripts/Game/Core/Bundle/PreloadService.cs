using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public sealed class PreloadService
{
        private interface IAssetPreloadEntry : IDisposable
        {
            string Key { get; }
            UnityEngine.Object AssetObject { get; }
            int RefCount { get; set; }
        }

        private sealed class AssetPreloadEntry<T> : IAssetPreloadEntry where T : UnityEngine.Object
        {
            public string Key { get; }
            public AssetHandle<T> Handle { get; }
            public int RefCount { get; set; }
            public UnityEngine.Object AssetObject => Handle.Asset;

            public AssetPreloadEntry(string key, AssetHandle<T> handle)
            {
                Key = key;
                Handle = handle;
                RefCount = 1;
            }

            public void Dispose()
            {
                Handle?.Release();
            }
        }

        private sealed class BundlePreloadEntry
        {
            public string BundleName;
            public int RefCount;

            public BundlePreloadEntry(string bundleName)
            {
                BundleName = bundleName;
                RefCount = 1;
            }
        }

        private sealed class GroupPreloadEntry
        {
            public string GroupKey;
            public string PackageName;
            public readonly HashSet<string> BundleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public readonly HashSet<string> AssetKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public int RefCount;
        }

        private static readonly PreloadService s_instance = new PreloadService();
        public static PreloadService Instance => s_instance;

        private readonly Dictionary<string, IAssetPreloadEntry> _assetEntries =
            new Dictionary<string, IAssetPreloadEntry>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, BundlePreloadEntry> _bundleEntries =
            new Dictionary<string, BundlePreloadEntry>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, GroupPreloadEntry> _groupEntries =
            new Dictionary<string, GroupPreloadEntry>(StringComparer.OrdinalIgnoreCase);

        private AssetAddressConfig _addressConfig;
        private PreloadGroupConfig _groupConfig;
        private IBundlePackageService _packageService;

        private PreloadService()
        {
        }

        public void Initialize(
            AssetAddressConfig addressConfig,
            PreloadGroupConfig groupConfig,
            IBundlePackageService packageService = null)
        {
            _addressConfig = addressConfig;
            _groupConfig = groupConfig;
            _packageService = packageService;
        }

        public bool IsBundlePreloaded(string bundleName)
        {
            return !string.IsNullOrWhiteSpace(bundleName) && _bundleEntries.ContainsKey(bundleName);
        }

        public bool IsAssetPreloaded(string assetKey)
        {
            return !string.IsNullOrWhiteSpace(assetKey) && _assetEntries.ContainsKey(assetKey);
        }

        public bool IsGroupPreloaded(string groupKey)
        {
            return !string.IsNullOrWhiteSpace(groupKey) && _groupEntries.ContainsKey(groupKey);
        }

        public async Task PreloadBundleAsync(string bundleName)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(bundleName))
            {
                throw BundleException.InvalidAddress("bundleName is null or empty.");
            }

            if (_bundleEntries.TryGetValue(bundleName, out var existing))
            {
                existing.RefCount++;
                BundleLogger.Info("PreloadBundle", $"reuse bundle={bundleName} refCount={existing.RefCount}");
                return;
            }

            await EnsureBundleReadyAsync(bundleName);
            await AssetBundleManager.Instance.PreloadBundleAsync(bundleName);

            _bundleEntries[bundleName] = new BundlePreloadEntry(bundleName);
            BundleLogger.Info("PreloadBundle", $"loaded bundle={bundleName}");
        }

        public async Task PreloadBundlesAsync(IList<string> bundleNames, Action<int, int> onProgress = null)
        {
            EnsureInitialized();

            if (bundleNames == null || bundleNames.Count == 0)
            {
                return;
            }

            for (var i = 0; i < bundleNames.Count; i++)
            {
                await PreloadBundleAsync(bundleNames[i]);
                onProgress?.Invoke(i + 1, bundleNames.Count);
            }
        }

        public async Task PreloadAssetAsync<T>(string assetKey) where T : UnityEngine.Object
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(assetKey))
            {
                throw BundleException.InvalidAddress("assetKey is null or empty.");
            }

            if (_assetEntries.TryGetValue(assetKey, out var existing))
            {
                existing.RefCount++;
                BundleLogger.Info("PreloadAsset", $"reuse assetKey={assetKey} refCount={existing.RefCount}");
                return;
            }

            var address = _addressConfig.GetAssetAddress(assetKey);
            await EnsureBundleReadyAsync(address.BundleName);

            var handle = await AssetBundleManager.Instance.LoadAssetAsync<T>(address);
            _assetEntries[assetKey] = new AssetPreloadEntry<T>(assetKey, handle);

            BundleLogger.Info("PreloadAsset", $"loaded assetKey={assetKey} bundle={address.BundleName}");
        }

        public async Task PreloadAssetsAsync<T>(IList<string> assetKeys, Action<int, int> onProgress = null)
            where T : UnityEngine.Object
        {
            EnsureInitialized();

            if (assetKeys == null || assetKeys.Count == 0)
            {
                return;
            }

            for (var i = 0; i < assetKeys.Count; i++)
            {
                await PreloadAssetAsync<T>(assetKeys[i]);
                onProgress?.Invoke(i + 1, assetKeys.Count);
            }
        }

        public async Task PreloadGroupAsync(string groupKey, Action<int, int> onProgress = null)
        {
            EnsureInitialized();

            if (!_groupConfig.TryGetGroup(groupKey, out var group))
            {
                throw BundleException.InvalidAddress($"groupKey={groupKey}");
            }

            if (_groupEntries.TryGetValue(groupKey, out var existingGroup))
            {
                existingGroup.RefCount++;
                BundleLogger.Info("PreloadGroup", $"reuse group={groupKey} refCount={existingGroup.RefCount}");
                return;
            }

            if (!string.IsNullOrWhiteSpace(group.PackageName))
            {
                await EnsurePackageReadyAsync(group.PackageName);
            }

            BundleLogger.Info("PreloadGroup", $"start group={groupKey}");

            var totalCount = (group.BundleNames?.Length ?? 0) + (group.AssetKeys?.Length ?? 0);
            var current = 0;

            var groupEntry = new GroupPreloadEntry
            {
                GroupKey = group.GroupKey,
                PackageName = group.PackageName,
                RefCount = 1,
            };

            _groupEntries[groupKey] = groupEntry;

            try
            {
                if (group.BundleNames != null)
                {
                    for (var i = 0; i < group.BundleNames.Length; i++)
                    {
                        var bundleName = group.BundleNames[i];
                        if (string.IsNullOrWhiteSpace(bundleName))
                        {
                            continue;
                        }

                        await PreloadBundleAsync(bundleName);
                        groupEntry.BundleNames.Add(bundleName);
                        current++;
                        onProgress?.Invoke(current, totalCount);
                    }
                }

                if (group.AssetKeys != null)
                {
                    for (var i = 0; i < group.AssetKeys.Length; i++)
                    {
                        var assetKey = group.AssetKeys[i];
                        if (string.IsNullOrWhiteSpace(assetKey))
                        {
                            continue;
                        }

                        await PreloadAssetAsync<UnityEngine.Object>(assetKey);
                        groupEntry.AssetKeys.Add(assetKey);
                        current++;
                        onProgress?.Invoke(current, totalCount);
                    }
                }

                BundleLogger.Info(
                    "PreloadGroup",
                    $"finish group={groupKey} bundleCount={groupEntry.BundleNames.Count} assetCount={groupEntry.AssetKeys.Count}");
            }
            catch
            {
                ReleaseGroup(groupKey);
                throw;
            }
        }

        public bool TryGetPreloadedAsset<T>(string assetKey, out T asset) where T : UnityEngine.Object
        {
            asset = null;

            if (string.IsNullOrWhiteSpace(assetKey))
            {
                return false;
            }

            if (!_assetEntries.TryGetValue(assetKey, out var entry))
            {
                return false;
            }

            asset = entry.AssetObject as T;
            return asset != null;
        }

        public T GetPreloadedAsset<T>(string assetKey) where T : UnityEngine.Object
        {
            if (TryGetPreloadedAsset<T>(assetKey, out var asset))
            {
                return asset;
            }

            throw BundleException.AssetLoadFailed($"assetKey={assetKey}");
        }

        public void ReleaseBundlePreload(string bundleName, bool unloadAllLoadedObjects = false)
        {
            if (string.IsNullOrWhiteSpace(bundleName))
            {
                BundleLogger.Warn("ReleaseBundlePreload", "bundleName is null or empty.");
                return;
            }

            if (!_bundleEntries.TryGetValue(bundleName, out var entry))
            {
                BundleLogger.Warn("ReleaseBundlePreload", $"bundle preload not found: {bundleName}");
                return;
            }

            if (entry.RefCount <= 0)
            {
                BundleLogger.Warn("ReleaseBundlePreload", $"invalid refCount bundle={bundleName}");
                return;
            }

            entry.RefCount--;
            BundleLogger.Info("ReleaseBundlePreload", $"bundle={bundleName} refCount={entry.RefCount}");

            if (entry.RefCount > 0)
            {
                return;
            }

            _bundleEntries.Remove(bundleName);
            AssetBundleManager.Instance.ReleaseBundle(bundleName, unloadAllLoadedObjects);
        }

        public void ReleaseAssetPreload(string assetKey)
        {
            if (string.IsNullOrWhiteSpace(assetKey))
            {
                BundleLogger.Warn("ReleaseAssetPreload", "assetKey is null or empty.");
                return;
            }

            if (!_assetEntries.TryGetValue(assetKey, out var entry))
            {
                BundleLogger.Warn("ReleaseAssetPreload", $"asset preload not found: {assetKey}");
                return;
            }

            if (entry.RefCount <= 0)
            {
                BundleLogger.Warn("ReleaseAssetPreload", $"invalid refCount assetKey={assetKey}");
                return;
            }

            entry.RefCount--;
            BundleLogger.Info("ReleaseAssetPreload", $"assetKey={assetKey} refCount={entry.RefCount}");

            if (entry.RefCount > 0)
            {
                return;
            }

            _assetEntries.Remove(assetKey);
            entry.Dispose();
        }

        public void ReleaseGroup(string groupKey)
        {
            if (string.IsNullOrWhiteSpace(groupKey))
            {
                BundleLogger.Warn("ReleaseGroup", "groupKey is null or empty.");
                return;
            }

            if (!_groupEntries.TryGetValue(groupKey, out var entry))
            {
                BundleLogger.Warn("ReleaseGroup", $"group preload not found: {groupKey}");
                return;
            }

            if (entry.RefCount <= 0)
            {
                BundleLogger.Warn("ReleaseGroup", $"invalid refCount group={groupKey}");
                return;
            }

            entry.RefCount--;
            BundleLogger.Info("ReleaseGroup", $"group={groupKey} refCount={entry.RefCount}");

            if (entry.RefCount > 0)
            {
                return;
            }

            _groupEntries.Remove(groupKey);

            foreach (var assetKey in entry.AssetKeys)
            {
                ReleaseAssetPreload(assetKey);
            }

            foreach (var bundleName in entry.BundleNames)
            {
                ReleaseBundlePreload(bundleName);
            }
        }

        public async Task ClearAllAsync(bool unloadUnusedAssets = true, bool forceGC = false)
        {
            _groupEntries.Clear();

            foreach (var pair in _assetEntries)
            {
                pair.Value.Dispose();
            }

            _assetEntries.Clear();

            foreach (var pair in _bundleEntries)
            {
                AssetBundleManager.Instance.ReleaseBundle(pair.Key);
            }

            _bundleEntries.Clear();

            if (unloadUnusedAssets)
            {
                await AssetBundleManager.Instance.UnloadUnusedAssetsAsync(forceGC);
            }

            BundleLogger.Info("ClearAllPreloads", "all preload caches cleared");
        }

        public PreloadedBundleInfo[] GetPreloadedBundleInfos()
        {
            var result = new PreloadedBundleInfo[_bundleEntries.Count];
            var index = 0;

            foreach (var pair in _bundleEntries)
            {
                result[index++] = new PreloadedBundleInfo
                {
                    BundleName = pair.Key,
                    RefCount = pair.Value.RefCount,
                };
            }

            return result;
        }

        public PreloadedAssetInfo[] GetPreloadedAssetInfos()
        {
            var result = new PreloadedAssetInfo[_assetEntries.Count];
            var index = 0;

            foreach (var pair in _assetEntries)
            {
                result[index++] = new PreloadedAssetInfo
                {
                    AssetKey = pair.Key,
                    AssetType = pair.Value.AssetObject == null ? "null" : pair.Value.AssetObject.GetType().Name,
                    RefCount = pair.Value.RefCount,
                };
            }

            return result;
        }

        public PreloadedGroupInfo[] GetPreloadedGroupInfos()
        {
            var result = new PreloadedGroupInfo[_groupEntries.Count];
            var index = 0;

            foreach (var pair in _groupEntries)
            {
                result[index++] = new PreloadedGroupInfo
                {
                    GroupKey = pair.Key,
                    PackageName = pair.Value.PackageName,
                    RefCount = pair.Value.RefCount,
                    BundleCount = pair.Value.BundleNames.Count,
                    AssetCount = pair.Value.AssetKeys.Count,
                };
            }

            return result;
        }

        private async Task EnsureBundleReadyAsync(string bundleName)
        {
            if (_packageService == null)
            {
                return;
            }

            if (_packageService.IsBundleReady(bundleName))
            {
                return;
            }

            await _packageService.EnsureBundleReadyAsync(bundleName);
        }

        private async Task EnsurePackageReadyAsync(string packageName)
        {
            if (_packageService == null || string.IsNullOrWhiteSpace(packageName))
            {
                return;
            }

            if (_packageService.IsPackageReady(packageName))
            {
                return;
            }

            await _packageService.EnsurePackageReadyAsync(packageName);
        }

        private void EnsureInitialized()
        {
            if (_addressConfig == null)
            {
                throw new InvalidOperationException("PreloadService address config is null.");
            }

            if (_groupConfig == null)
            {
                throw new InvalidOperationException("PreloadService group config is null.");
            }
        }
}
