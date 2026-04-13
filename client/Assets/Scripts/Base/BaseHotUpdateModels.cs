using System;
using System.Collections.Generic;

[Serializable]
public sealed class GetResVersionResponse
{
    public bool ok;
    public string version;
}

[Serializable]
public sealed class VersionJsonManifest
{
    public List<VersionJsonDllEntry> Dlls;
    public List<VersionJsonBundleEntry> Bundles;
}

[Serializable]
public sealed class VersionJsonDllEntry
{
    public string Path;
    public string Md5;
}

[Serializable]
public sealed class VersionJsonBundleEntry
{
    public string Path;
    public string Md5;
}
