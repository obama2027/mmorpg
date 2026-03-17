using System;

[Serializable]
public struct AssetAddress
{
    public string BundleName;
    public string AssetName;
    public string EditorAssetPath;

    public AssetAddress(string bundleName, string assetName, string editorAssetPath = null)
    {
        BundleName = bundleName;
        AssetName = assetName;
        EditorAssetPath = editorAssetPath;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(BundleName) && !string.IsNullOrWhiteSpace(AssetName);
    }

    public override string ToString()
    {
        return $"{BundleName}:{AssetName}";
    }
}
