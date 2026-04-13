using System;

/// <summary>
/// View from <see cref="PropHub.GetProp"/>: read value and attach local listeners. Must use on the Unity main thread.
/// </summary>
public readonly struct PropHandle
{
    //public static readonly PropHandle Invalid = new PropHandle(null, null);

    //private readonly string _key;
    //private readonly PropEntry _entry;

    //internal PropHandle(string key, PropEntry entry)
    //{
    //    _key = key;
    //    _entry = entry;
    //}

    //public bool IsValid => _entry != null;

    ///// <summary>Declared type from <see cref="PropHub.CreateProp{T}"/>.</summary>
    //public Type DeclaredValueType => _entry?.DeclaredType;

    //public object Get()
    //{
    //    PropMainThread.Assert();

    //    return _entry?.Value;
    //}

    //public T Get<T>()
    //{
    //    PropMainThread.Assert();

    //    var v = _entry?.Value;
    //    if (v == null)
    //    {
    //        return default;
    //    }

    //    if (v is T typed)
    //    {
    //        return typed;
    //    }

    //    try
    //    {
    //        return (T)Convert.ChangeType(v, typeof(T));
    //    }
    //    catch
    //    {
    //        return default;
    //    }
    //}

    //public PropSubscription AddSignal(Action<object, object> onOldNew)
    //{
    //    PropMainThread.Assert();

    //    if (_entry == null)
    //    {
    //        UnityEngine.Debug.LogWarning($"[PropHub] GetProp('{_key}') is invalid (key not created). Subscription ignored.");
    //        return PropSubscription.Ignored;
    //    }

    //    return _entry.AddSignal(onOldNew);
    //}

    //public PropSubscription AddSignal(Action onChanged)
    //{
    //    PropMainThread.Assert();

    //    if (onChanged == null)
    //    {
    //        throw new ArgumentNullException(nameof(onChanged));
    //    }

    //    return AddSignal((_, __) => onChanged());
    //}
}
