using System;

namespace MMORPG.Client.Core.Bundle
{
    [Serializable]
    public struct SceneAddress
    {
        public string BundleName;
        public string ScenePath;
        public string EditorScenePath;

        public SceneAddress(string bundleName, string scenePath, string editorScenePath = null)
        {
            BundleName = bundleName;
            ScenePath = scenePath;
            EditorScenePath = editorScenePath;
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(BundleName) && !string.IsNullOrWhiteSpace(ScenePath);
        }

        public override string ToString()
        {
            return $"{BundleName}:{ScenePath}";
        }
    }
}
