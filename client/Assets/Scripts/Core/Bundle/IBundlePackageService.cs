using System.Threading.Tasks;

public interface IBundlePackageService
{
    bool IsBundleReady(string bundleName);
    Task EnsureBundleReadyAsync(string bundleName);

    bool IsPackageReady(string packageName);
    Task EnsurePackageReadyAsync(string packageName);
}
