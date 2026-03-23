using System;

public sealed class AssetHandle<T> : IDisposable where T : UnityEngine.Object
{
    private Action _releaseAction;
    private bool _released;

    public AssetAddress Address { get; }
    public T Asset { get; }
    public bool IsReleased => _released;

    internal AssetHandle(AssetAddress address, T asset, Action releaseAction)
    {
        Address = address;
        Asset = asset;
        _releaseAction = releaseAction;
    }

    public void Release()
    {
        if (_released)
        {
            BundleLogger.Warn("AssetHandle", $"duplicate release address={Address}");
            return;
        }

        _released = true;
        _releaseAction?.Invoke();
        _releaseAction = null;
    }

    public void Dispose()
    {
        Release();
    }

    public static implicit operator T(AssetHandle<T> handle)
    {
        return handle.Asset;
    }
}
