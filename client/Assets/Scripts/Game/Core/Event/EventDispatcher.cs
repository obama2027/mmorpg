using System;
using System.Collections.Generic;

public class EventArgs
{
    
}

public class StringEventArgs : EventArgs
{
    public string Message { get; set; }
}

public static class EventDispatcher
{
    // 存储所有事件类型对应的监听器列表
    // 键：事件名称（string），值：委托列表（多播委托）
    private static Dictionary<int, Delegate> _events = new Dictionary<int, Delegate>();

    // 添加监听器（泛型方法，确保委托类型正确）
    public static void AddListener<T>(int eventId, Action<T> listener) where T : EventArgs
    {
        if (_events.TryGetValue(eventId, out var existingDelegate))
        {
            // 将新的监听器合并到现有委托中
            _events[eventId] = Delegate.Combine(existingDelegate, listener);
        }
        else
        {
            _events[eventId] = listener;
        }
    }

    // 移除监听器（泛型方法）
    public static void RemoveListener<T>(int eventId, Action<T> listener) where T : EventArgs
    {
        if (_events.TryGetValue(eventId, out var existingDelegate))
        {
            var newDelegate = Delegate.Remove(existingDelegate, listener);
            if (newDelegate == null)
                _events.Remove(eventId);
            else
                _events[eventId] = newDelegate;
        }
    }

    // 触发事件（无参数版本）
    public static void Dispatch(int eventId)
    {
        if (_events.TryGetValue(eventId, out var del) && del is Action action)
        {
            action?.Invoke();
        }
    }

    // 触发事件（带参数版本）
    public static void Dispatch<T>(int eventId, T args) where T : EventArgs
    {
        if (_events.TryGetValue(eventId, out var del) && del is Action<T> action)
        {
            action?.Invoke(args);
        }
    }

    // 移除所有监听器（可选，用于清理）
    public static void ClearAll()
    {
        _events.Clear();
    }
}

