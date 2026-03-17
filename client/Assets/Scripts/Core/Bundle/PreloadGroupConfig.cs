using System;
using UnityEngine;

namespace MMORPG.Client.Core.Bundle
{
    [CreateAssetMenu(fileName = "PreloadGroupConfig", menuName = "MMORPG/Bundle/Preload Group Config")]
    public sealed class PreloadGroupConfig : ScriptableObject
    {
        [Serializable]
        public sealed class GroupEntry
        {
            [Header("Group Info")]
            public string GroupKey;
            public string PackageName;

            [Header("Bundle Preloads")]
            public string[] BundleNames = Array.Empty<string>();

            [Header("Asset Preloads")]
            public string[] AssetKeys = Array.Empty<string>();
        }

        [SerializeField] private GroupEntry[] _groups = Array.Empty<GroupEntry>();

        public GroupEntry[] Groups => _groups;

        public bool TryGetGroup(string groupKey, out GroupEntry group)
        {
            group = null;
            if (string.IsNullOrWhiteSpace(groupKey) || _groups == null)
            {
                return false;
            }

            for (var i = 0; i < _groups.Length; i++)
            {
                var entry = _groups[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.GroupKey))
                {
                    continue;
                }

                if (string.Equals(entry.GroupKey, groupKey, StringComparison.OrdinalIgnoreCase))
                {
                    group = entry;
                    return true;
                }
            }

            return false;
        }
    }
}
