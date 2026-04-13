using System;

[Serializable]
public sealed class UIConfig
{
    public UIType type = UIType.None;
    public UILayer defaultLayer = UILayer.FullScreen;
    public string bundlePath;
    public string assetPath;
}

