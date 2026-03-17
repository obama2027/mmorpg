using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class GameAssetService : MonoBehaviour
{
    public static GameAssetService Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private BuildConfig _buildConfig;
    [SerializeField] private AssetAddressConfig _addressConfig;
    [SerializeField] private PreloadGroupConfig _preloadGroupConfig;

    [Header("Log Switch")]
    [SerializeField] private bool _enableBundleInfoLog = true;
    [SerializeField] private bool _enableBundleWarningLog = true;
    [SerializeField] private bool _enableBundleErrorLog = true;

    private IBundlePackageService _packageService;

    public BuildConfig BuildConfig => _buildConfig;
    public AssetAddressConfig AddressConfig => _addressConfig;
    public PreloadGroupConfig PreloadGroupConfig => _preloadGroupConfig;

        private async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

        BundleLogger.EnableInfoLog = _enableBundleInfoLog;
        BundleLogger.EnableWarningLog = _enableBundleWarningLog;
        BundleLogger.EnableErrorLog = _enableBundleErrorLog;

        AssetBundleManager.Instance.Configure(ShouldUseEditorDevelopmentMode());
        await AssetBundleManager.Instance.InitializeAsync();

        _packageService = new LocalBundlePackageService();
        PreloadService.Instance.Initialize(_addressConfig, _preloadGroupConfig, _packageService);

        BundleLogger.Info("GameAssetService", "initialized");
    }

    private bool ShouldUseEditorDevelopmentMode()
    {
#if UNITY_EDITOR
        var buildConfig = ResolveBuildConfig();
        return buildConfig != null && buildConfig.EditorDevelopmentMode;
#else
        return false;
#endif
    }

#if UNITY_EDITOR
    private BuildConfig ResolveBuildConfig()
    {
        if (_buildConfig != null)
        {
            return _buildConfig;
        }

        _buildConfig = AssetDatabase.LoadAssetAtPath<BuildConfig>(BuildConfig.AssetPath);
        if (_buildConfig == null)
        {
            BundleLogger.Warn("GameAssetService", $"BuildConfig not found: {BuildConfig.AssetPath}");
        }

        return _buildConfig;
    }
#endif

    public AssetHandle<T> LoadAsset<T>(string key) where T : UnityEngine.Object
    {
        var address = _addressConfig.GetAssetAddress(key);
        return AssetBundleManager.Instance.LoadAsset<T>(address);
    }

    public Task<AssetHandle<T>> LoadAssetAsync<T>(string key) where T : UnityEngine.Object
    {
        var address = _addressConfig.GetAssetAddress(key);
        return AssetBundleManager.Instance.LoadAssetAsync<T>(address);
    }

    public Task LoadSceneAsync(string key, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        var address = _addressConfig.GetSceneAddress(key);
        return BundleSceneLoader.Instance.LoadSceneAsync(address, loadSceneMode);
    }

    public Task UnloadSceneAsync(string key, bool unloadUnusedAssets = true, bool forceGC = false)
    {
        var address = _addressConfig.GetSceneAddress(key);
        return BundleSceneLoader.Instance.UnloadSceneAsync(address, unloadUnusedAssets, forceGC);
    }

    public Task PreloadBundleAsync(string bundleName)
    {
        return PreloadService.Instance.PreloadBundleAsync(bundleName);
    }

    public Task PreloadBundlesAsync(IList<string> bundleNames, Action<int, int> onProgress = null)
    {
        return PreloadService.Instance.PreloadBundlesAsync(bundleNames, onProgress);
    }

    public Task PreloadAssetAsync<T>(string assetKey) where T : UnityEngine.Object
    {
        return PreloadService.Instance.PreloadAssetAsync<T>(assetKey);
    }

    public Task PreloadAssetsAsync<T>(IList<string> assetKeys, Action<int, int> onProgress = null)
        where T : UnityEngine.Object
    {
        return PreloadService.Instance.PreloadAssetsAsync<T>(assetKeys, onProgress);
    }

    public Task PreloadGroupAsync(string groupKey, Action<int, int> onProgress = null)
    {
        return PreloadService.Instance.PreloadGroupAsync(groupKey, onProgress);
    }

    public void ReleaseBundlePreload(string bundleName)
    {
        PreloadService.Instance.ReleaseBundlePreload(bundleName);
    }

    public void ReleaseAssetPreload(string assetKey)
    {
        PreloadService.Instance.ReleaseAssetPreload(assetKey);
    }

    public void ReleaseGroup(string groupKey)
    {
        PreloadService.Instance.ReleaseGroup(groupKey);
    }

    public bool TryGetPreloadedAsset<T>(string assetKey, out T asset) where T : UnityEngine.Object
    {
        return PreloadService.Instance.TryGetPreloadedAsset(assetKey, out asset);
    }

    public Task ClearAllPreloadsAsync(bool unloadUnusedAssets = true, bool forceGC = false)
    {
        return PreloadService.Instance.ClearAllAsync(unloadUnusedAssets, forceGC);
    }

    public async Task ReleaseGroupAndCollectAsync(string groupKey, bool forceGC = true)
    {
        ReleaseGroup(groupKey);
        await AssetBundleManager.Instance.UnloadUnusedAssetsAsync(forceGC);
    }

    public LoadedBundleInfo[] GetLoadedBundleInfos()
    {
        return AssetBundleManager.Instance.GetLoadedBundleInfos();
    }

    public PreloadedBundleInfo[] GetPreloadedBundleInfos()
    {
        return PreloadService.Instance.GetPreloadedBundleInfos();
    }

    public PreloadedAssetInfo[] GetPreloadedAssetInfos()
    {
        return PreloadService.Instance.GetPreloadedAssetInfos();
    }

    public PreloadedGroupInfo[] GetPreloadedGroupInfos()
    {
        return PreloadService.Instance.GetPreloadedGroupInfos();
    }
}
