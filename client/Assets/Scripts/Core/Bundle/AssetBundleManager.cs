using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MMORPG.Client.Core.Bundle
{
    public enum BundleRuntimeMode
    {
        Development = 0,
        AssetBundle = 1,
    }

    public sealed class AssetBundleManager
    {
        private sealed class BundleEntry
        {
            public AssetBundle Bundle;
            public string[] Dependencies;
            public int RefCount;

            public BundleEntry(AssetBundle bundle, string[] dependencies)
            {
                Bundle = bundle;
                Dependencies = dependencies ?? Array.Empty<string>();
                RefCount = 0;
            }
        }

        private static readonly AssetBundleManager s_instance = new AssetBundleManager();
        public static AssetBundleManager Instance => s_instance;

        private readonly Dictionary<string, BundleEntry> _loadedBundles =
            new Dictionary<string, BundleEntry>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, Task<AssetBundle>> _loadingBundles =
            new Dictionary<string, Task<AssetBundle>>(StringComparer.OrdinalIgnoreCase);

        private string _bundleRootFolderName = "AssetBundles";
        private BundleRuntimeMode _runtimeMode = BundleRuntimeMode.AssetBundle;
        private AssetBundleManifest _manifest;
        private bool _initialized;
        private Task _initializeTask;

        private AssetBundleManager()
        {
        }

        public bool IsDevelopmentMode
        {
            get
            {
#if UNITY_EDITOR
                return _runtimeMode == BundleRuntimeMode.Development;
#else
                return false;
#endif
            }
        }

        public void Configure(bool developmentMode, string bundleRootFolderName = "AssetBundles")
        {
            _bundleRootFolderName = string.IsNullOrWhiteSpace(bundleRootFolderName)
                ? "AssetBundles"
                : bundleRootFolderName.Trim();

#if UNITY_EDITOR
            _runtimeMode = developmentMode ? BundleRuntimeMode.Development : BundleRuntimeMode.AssetBundle;
#else
            _runtimeMode = BundleRuntimeMode.AssetBundle;
#endif
        }

        public void Initialize()
        {
            EnsureRuntimeAsyncOnly(nameof(Initialize), "manifest");
            BundleLogger.Info("Initialize", "editor development mode initialized");
        }

        public async Task InitializeAsync()
        {
            await EnsureManifestLoadedAsync();
        }

        public AssetHandle<T> LoadAsset<T>(AssetAddress address) where T : UnityEngine.Object
        {
            ValidateAddressOrThrow(address);
            EnsureRuntimeAsyncOnly(nameof(LoadAsset), address.ToString());

#if UNITY_EDITOR
            var editorAsset = LoadFromEditor<T>(address);
            BundleLogger.Info("LoadAsset", $"sync editor load asset={address}");
            return new AssetHandle<T>(address, editorAsset, null);
#else
            throw BundleException.SyncLoadNotAllowed(nameof(LoadAsset), address.ToString());
#endif
        }

        public async Task<AssetHandle<T>> LoadAssetAsync<T>(AssetAddress address) where T : UnityEngine.Object
        {
            ValidateAddressOrThrow(address);

            if (ShouldLoadFromEditorSource())
            {
#if UNITY_EDITOR
                var editorAsset = LoadFromEditor<T>(address);
                BundleLogger.Info("LoadAssetAsync", $"editor load asset={address}");
                return new AssetHandle<T>(address, editorAsset, null);
#else
                throw BundleException.AssetLoadFailed(address.ToString());
#endif
            }

            try
            {
                var bundle = await AcquireBundleAsync(address.BundleName);
                var request = bundle.LoadAssetAsync<T>(address.AssetName);
                while (!request.isDone)
                {
                    await Task.Yield();
                }

                var asset = request.asset as T;
                if (asset == null)
                {
                    ReleaseBundle(address.BundleName);
                    throw BundleException.AssetLoadFailed(address.ToString());
                }

                BundleLogger.Info("LoadAssetAsync", $"asset={address} bundle={address.BundleName}");
                return new AssetHandle<T>(address, asset, () => ReleaseBundle(address.BundleName));
            }
            catch (BundleException)
            {
                throw;
            }
            catch (Exception ex)
            {
                BundleLogger.Error("LoadAssetAsync", $"asset={address} ex={ex.Message}");
                throw BundleException.AssetLoadFailed(address.ToString(), ex);
            }
        }

        public void PreloadBundle(string bundleName)
        {
            if (string.IsNullOrWhiteSpace(bundleName))
            {
                throw BundleException.InvalidAddress("bundleName is null or empty.");
            }

            EnsureRuntimeAsyncOnly(nameof(PreloadBundle), bundleName);
            BundleLogger.Info("PreloadBundle", $"sync editor preload bundle={bundleName}");
        }

        public async Task PreloadBundleAsync(string bundleName)
        {
            if (string.IsNullOrWhiteSpace(bundleName))
            {
                throw BundleException.InvalidAddress("bundleName is null or empty.");
            }

            if (ShouldLoadFromEditorSource())
            {
                BundleLogger.Info("PreloadBundleAsync", $"editor mode noop bundle={bundleName}");
                return;
            }

            await AcquireBundleAsync(bundleName);
        }

        public void ReleaseHandle<T>(AssetHandle<T> handle) where T : UnityEngine.Object
        {
            handle?.Release();
        }

        public void ReleaseBundle(string bundleName, bool unloadAllLoadedObjects = false)
        {
            if (string.IsNullOrWhiteSpace(bundleName))
            {
                BundleLogger.Warn("ReleaseBundle", "bundleName is null or empty.");
                return;
            }

            if (!_loadedBundles.TryGetValue(bundleName, out var entry))
            {
                BundleLogger.Warn("ReleaseBundle", $"bundle not loaded: {bundleName}");
                return;
            }

            if (entry.RefCount <= 0)
            {
                BundleLogger.Warn("ReleaseBundle", $"invalid refCount bundle={bundleName} refCount={entry.RefCount}");
                return;
            }

            entry.RefCount--;
            BundleLogger.Info("ReleaseBundle", $"bundle={bundleName} refCount={entry.RefCount}");

            if (entry.RefCount > 0)
            {
                return;
            }

            _loadedBundles.Remove(bundleName);

            var dependencies = entry.Dependencies;
            entry.Bundle.Unload(unloadAllLoadedObjects);
            BundleLogger.Info("ReleaseBundle", $"unload bundle={bundleName} dependencyCount={dependencies.Length}");

            for (var i = 0; i < dependencies.Length; i++)
            {
                ReleaseBundle(dependencies[i], unloadAllLoadedObjects);
            }
        }

        public void UnloadAllBundles(bool unloadAllLoadedObjects = false)
        {
            foreach (var pair in _loadedBundles)
            {
                pair.Value.Bundle.Unload(unloadAllLoadedObjects);
            }

            _loadedBundles.Clear();
            _loadingBundles.Clear();
            BundleLogger.Info("UnloadAllBundles", "all loaded bundles cleared");
        }

        public async Task UnloadUnusedAssetsAsync(bool forceGC = false)
        {
            var request = Resources.UnloadUnusedAssets();
            while (!request.isDone)
            {
                await Task.Yield();
            }

            if (forceGC)
            {
                GC.Collect();
            }

            BundleLogger.Info("UnloadUnusedAssetsAsync", $"completed forceGC={forceGC}");
        }

        public LoadedBundleInfo[] GetLoadedBundleInfos()
        {
            var result = new LoadedBundleInfo[_loadedBundles.Count];
            var index = 0;

            foreach (var pair in _loadedBundles)
            {
                result[index++] = new LoadedBundleInfo
                {
                    BundleName = pair.Key,
                    RefCount = pair.Value.RefCount,
                    Dependencies = pair.Value.Dependencies
                };
            }

            return result;
        }

        private void ValidateAddressOrThrow(AssetAddress address)
        {
            if (!address.IsValid())
            {
                throw BundleException.InvalidAddress(address.ToString());
            }
        }

        private void EnsureRuntimeAsyncOnly(string apiName, string target)
        {
            if (!ShouldLoadFromEditorSource())
            {
                throw BundleException.SyncLoadNotAllowed(apiName, target);
            }
        }

        private bool ShouldLoadFromEditorSource()
        {
#if UNITY_EDITOR
            return _runtimeMode == BundleRuntimeMode.Development;
#else
            return false;
#endif
        }

        private async Task EnsureManifestLoadedAsync()
        {
            if (_initialized || ShouldLoadFromEditorSource())
            {
                return;
            }

            if (_initializeTask == null)
            {
                _initializeTask = LoadManifestAsync();
            }

            try
            {
                await _initializeTask;
            }
            catch (BundleException)
            {
                throw;
            }
            catch (Exception ex)
            {
                BundleLogger.Error("Manifest", ex.Message);
                throw BundleException.ManifestLoadFailed(BundlePathUtility.GetRuntimePlatformName(), ex);
            }
        }

        private async Task LoadManifestAsync()
        {
            var manifestBundleName = BundlePathUtility.GetRuntimePlatformName();
            var manifestBundle = await LoadBundleFileAsync(manifestBundleName);

            try
            {
                var request = manifestBundle.LoadAssetAsync<AssetBundleManifest>("AssetBundleManifest");
                while (!request.isDone)
                {
                    await Task.Yield();
                }

                _manifest = request.asset as AssetBundleManifest;
                if (_manifest == null)
                {
                    throw BundleException.ManifestLoadFailed($"manifest bundle={manifestBundleName}");
                }

                _initialized = true;
                BundleLogger.Info("Manifest", $"loaded manifest bundle={manifestBundleName}");
            }
            finally
            {
                manifestBundle.Unload(false);
            }
        }

        private async Task<AssetBundle> AcquireBundleAsync(string bundleName)
        {
            await EnsureManifestLoadedAsync();

            if (_loadedBundles.TryGetValue(bundleName, out var loadedEntry))
            {
                loadedEntry.RefCount++;
                BundleLogger.Info("AcquireBundleAsync", $"reuse bundle={bundleName} refCount={loadedEntry.RefCount}");
                return loadedEntry.Bundle;
            }

            if (_loadingBundles.TryGetValue(bundleName, out var existingTask))
            {
                var existingBundle = await existingTask;
                _loadedBundles[bundleName].RefCount++;
                BundleLogger.Info("AcquireBundleAsync", $"join loading bundle={bundleName} refCount={_loadedBundles[bundleName].RefCount}");
                return existingBundle;
            }

            var task = InternalAcquireBundleAsync(bundleName);
            _loadingBundles[bundleName] = task;

            try
            {
                var bundle = await task;
                _loadedBundles[bundleName].RefCount++;
                BundleLogger.Info("AcquireBundleAsync", $"loaded bundle={bundleName} refCount={_loadedBundles[bundleName].RefCount}");
                return bundle;
            }
            catch (BundleException)
            {
                throw;
            }
            catch (Exception ex)
            {
                BundleLogger.Error("AcquireBundleAsync", $"bundle={bundleName} ex={ex.Message}");
                throw BundleException.BundleLoadFailed(bundleName, ex);
            }
            finally
            {
                _loadingBundles.Remove(bundleName);
            }
        }

        private async Task<AssetBundle> InternalAcquireBundleAsync(string bundleName)
        {
            var dependencies = GetDependencies(bundleName);

            try
            {
                for (var i = 0; i < dependencies.Length; i++)
                {
                    await AcquireBundleAsync(dependencies[i]);
                }

                var bundle = await LoadBundleFileAsync(bundleName);
                _loadedBundles[bundleName] = new BundleEntry(bundle, dependencies);
                return bundle;
            }
            catch
            {
                for (var i = dependencies.Length - 1; i >= 0; i--)
                {
                    ReleaseBundle(dependencies[i]);
                }

                throw;
            }
        }

        private string[] GetDependencies(string bundleName)
        {
            if (_manifest == null)
            {
                return Array.Empty<string>();
            }

            return _manifest.GetAllDependencies(bundleName) ?? Array.Empty<string>();
        }

        private async Task<AssetBundle> LoadBundleFileAsync(string bundleName)
        {
            var path = BundlePathUtility.GetBundleFilePath(_bundleRootFolderName, bundleName);

#if UNITY_ANDROID && !UNITY_EDITOR
            using (var request = UnityWebRequestAssetBundle.GetAssetBundle(path))
            {
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw BundleException.BundleLoadFailed($"bundle={bundleName}, path={path}, error={request.error}");
                }

                var bundle = DownloadHandlerAssetBundle.GetContent(request);
                if (bundle == null)
                {
                    throw BundleException.BundleLoadFailed($"bundle={bundleName}, path={path}, bundle is null");
                }

                return bundle;
            }
#else
            var request = AssetBundle.LoadFromFileAsync(path);
            while (!request.isDone)
            {
                await Task.Yield();
            }

            if (request.assetBundle == null)
            {
                throw BundleException.BundleLoadFailed($"bundle={bundleName}, path={path}");
            }

            return request.assetBundle;
#endif
        }

#if UNITY_EDITOR
        private T LoadFromEditor<T>(AssetAddress address) where T : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(address.EditorAssetPath))
            {
                throw BundleException.AssetLoadFailed($"EditorAssetPath is empty address={address}");
            }

            var asset = AssetDatabase.LoadAssetAtPath<T>(address.EditorAssetPath);
            if (asset == null)
            {
                throw BundleException.AssetLoadFailed($"Editor asset not found path={address.EditorAssetPath}");
            }

            return asset;
        }
#endif
    }
}
