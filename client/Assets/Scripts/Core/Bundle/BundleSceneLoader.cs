using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace MMORPG.Client.Core.Bundle
{
    public sealed class BundleSceneLoader
    {
        private sealed class LoadedSceneEntry
        {
            public string BundleName;
            public int RefCount;
        }

        private static readonly BundleSceneLoader s_instance = new BundleSceneLoader();
        public static BundleSceneLoader Instance => s_instance;

        private readonly Dictionary<string, LoadedSceneEntry> _loadedScenes =
            new Dictionary<string, LoadedSceneEntry>(StringComparer.OrdinalIgnoreCase);

        private bool _initialized;

        private BundleSceneLoader()
        {
            Initialize();
        }

        public async Task LoadSceneAsync(SceneAddress address, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            ValidateAddress(address);
            Initialize();

#if UNITY_EDITOR
            if (AssetBundleManager.Instance.IsDevelopmentMode)
            {
                if (string.IsNullOrWhiteSpace(address.EditorScenePath))
                {
                    throw BundleException.SceneLoadFailed($"EditorScenePath is empty address={address}");
                }

                var parameters = new LoadSceneParameters(loadSceneMode);
                var editorOp = EditorSceneManager.LoadSceneAsyncInPlayMode(address.EditorScenePath, parameters);
                if (editorOp == null)
                {
                    throw BundleException.SceneLoadFailed($"editor scene load failed path={address.EditorScenePath}");
                }

                while (!editorOp.isDone)
                {
                    await Task.Yield();
                }

                BundleLogger.Info("LoadSceneAsync", $"editor scene load scene={address.EditorScenePath}");
                return;
            }
#endif

            await AssetBundleManager.Instance.PreloadBundleAsync(address.BundleName);

            try
            {
                var loadOp = SceneManager.LoadSceneAsync(address.ScenePath, loadSceneMode);
                if (loadOp == null)
                {
                    throw BundleException.SceneLoadFailed($"scenePath={address.ScenePath}");
                }

                while (!loadOp.isDone)
                {
                    await Task.Yield();
                }

                RegisterLoadedScene(address.ScenePath, address.BundleName);
                BundleLogger.Info("LoadSceneAsync", $"scene={address.ScenePath} bundle={address.BundleName}");
            }
            catch (BundleException)
            {
                AssetBundleManager.Instance.ReleaseBundle(address.BundleName);
                throw;
            }
            catch (Exception ex)
            {
                AssetBundleManager.Instance.ReleaseBundle(address.BundleName);
                throw BundleException.SceneLoadFailed(address.ToString(), ex);
            }
        }

        public async Task UnloadSceneAsync(SceneAddress address, bool unloadUnusedAssets = true, bool forceGC = false)
        {
            ValidateAddress(address);

#if UNITY_EDITOR
            if (AssetBundleManager.Instance.IsDevelopmentMode)
            {
                var editorScenePath = string.IsNullOrWhiteSpace(address.EditorScenePath)
                    ? address.ScenePath
                    : address.EditorScenePath;

                var editorScene = SceneManager.GetSceneByPath(editorScenePath);
                if (!editorScene.IsValid() || !editorScene.isLoaded)
                {
                    return;
                }

                var editorUnloadOp = SceneManager.UnloadSceneAsync(editorScene);
                if (editorUnloadOp == null)
                {
                    return;
                }

                while (!editorUnloadOp.isDone)
                {
                    await Task.Yield();
                }

                if (unloadUnusedAssets)
                {
                    await AssetBundleManager.Instance.UnloadUnusedAssetsAsync(forceGC);
                }

                BundleLogger.Info("UnloadSceneAsync", $"editor scene unload scene={editorScenePath}");
                return;
            }
#endif

            var runtimeScene = SceneManager.GetSceneByPath(address.ScenePath);
            if (!runtimeScene.IsValid() || !runtimeScene.isLoaded)
            {
                return;
            }

            var unloadOp = SceneManager.UnloadSceneAsync(runtimeScene);
            if (unloadOp == null)
            {
                return;
            }

            while (!unloadOp.isDone)
            {
                await Task.Yield();
            }

            if (unloadUnusedAssets)
            {
                await AssetBundleManager.Instance.UnloadUnusedAssetsAsync(forceGC);
            }

            BundleLogger.Info("UnloadSceneAsync", $"scene={address.ScenePath}");
        }

        private void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            SceneManager.sceneUnloaded += OnSceneUnloaded;
            _initialized = true;
        }

        private void ValidateAddress(SceneAddress address)
        {
            if (!address.IsValid())
            {
                throw BundleException.InvalidAddress(address.ToString());
            }
        }

        private void RegisterLoadedScene(string scenePath, string bundleName)
        {
            if (_loadedScenes.TryGetValue(scenePath, out var entry))
            {
                entry.RefCount++;
                return;
            }

            _loadedScenes[scenePath] = new LoadedSceneEntry
            {
                BundleName = bundleName,
                RefCount = 1,
            };
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (string.IsNullOrWhiteSpace(scene.path))
            {
                return;
            }

            if (!_loadedScenes.TryGetValue(scene.path, out var entry))
            {
                return;
            }

            entry.RefCount--;
            if (entry.RefCount > 0)
            {
                return;
            }

            _loadedScenes.Remove(scene.path);
            AssetBundleManager.Instance.ReleaseBundle(entry.BundleName);
            BundleLogger.Info("SceneUnloaded", $"scene={scene.path} release bundle={entry.BundleName}");
        }
    }
}
