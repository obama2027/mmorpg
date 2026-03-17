using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AssetAddressConfig", menuName = "MMORPG/Bundle/Asset Address Config")]
public sealed class AssetAddressConfig : ScriptableObject
{
        [Serializable]
        public sealed class AssetEntry
        {
            public string Key;
            public string BundleName;
            public string AssetName;
            public string EditorAssetPath;
        }

        [Serializable]
        public sealed class SceneEntry
        {
            public string Key;
            public string BundleName;
            public string ScenePath;
            public string EditorScenePath;
        }

        [SerializeField] private AssetEntry[] _assetEntries = Array.Empty<AssetEntry>();
        [SerializeField] private SceneEntry[] _sceneEntries = Array.Empty<SceneEntry>();

        private readonly Dictionary<string, AssetAddress> _assetMap =
            new Dictionary<string, AssetAddress>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, SceneAddress> _sceneMap =
            new Dictionary<string, SceneAddress>(StringComparer.OrdinalIgnoreCase);

        public AssetEntry[] AssetEntries => _assetEntries;
        public SceneEntry[] SceneEntries => _sceneEntries;

        private void OnEnable()
        {
            RebuildLookup();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            RebuildLookup();
        }
#endif

        public AssetAddress GetAssetAddress(string key)
        {
            if (TryGetAssetAddress(key, out var address))
            {
                return address;
            }

            throw BundleException.InvalidAddress($"assetKey={key}");
        }

        public bool TryGetAssetAddress(string key, out AssetAddress address)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                address = default;
                return false;
            }

            return _assetMap.TryGetValue(key, out address);
        }

        public SceneAddress GetSceneAddress(string key)
        {
            if (TryGetSceneAddress(key, out var address))
            {
                return address;
            }

            throw BundleException.InvalidAddress($"sceneKey={key}");
        }

        public bool TryGetSceneAddress(string key, out SceneAddress address)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                address = default;
                return false;
            }

            return _sceneMap.TryGetValue(key, out address);
        }

        private void RebuildLookup()
        {
            _assetMap.Clear();
            _sceneMap.Clear();

            if (_assetEntries != null)
            {
                for (var i = 0; i < _assetEntries.Length; i++)
                {
                    var entry = _assetEntries[i];
                    if (entry == null ||
                        string.IsNullOrWhiteSpace(entry.Key) ||
                        string.IsNullOrWhiteSpace(entry.BundleName) ||
                        string.IsNullOrWhiteSpace(entry.AssetName))
                    {
                        continue;
                    }

                    _assetMap[entry.Key] = new AssetAddress(entry.BundleName, entry.AssetName, entry.EditorAssetPath);
                }
            }

            if (_sceneEntries != null)
            {
                for (var i = 0; i < _sceneEntries.Length; i++)
                {
                    var entry = _sceneEntries[i];
                    if (entry == null ||
                        string.IsNullOrWhiteSpace(entry.Key) ||
                        string.IsNullOrWhiteSpace(entry.BundleName) ||
                        string.IsNullOrWhiteSpace(entry.ScenePath))
                    {
                        continue;
                    }

                    _sceneMap[entry.Key] = new SceneAddress(entry.BundleName, entry.ScenePath, entry.EditorScenePath);
                }
            }
        }
}
