using System.Threading.Tasks;

public sealed class LocalBundlePackageService : IBundlePackageService
{
    public bool IsBundleReady(string bundleName)
    {
        return true;
    }

    public Task EnsureBundleReadyAsync(string bundleName)
    {
        return Task.CompletedTask;
    }

    public bool IsPackageReady(string packageName)
    {
        return true;
    }

    public Task EnsurePackageReadyAsync(string packageName)
    {
        return Task.CompletedTask;
    }
}
