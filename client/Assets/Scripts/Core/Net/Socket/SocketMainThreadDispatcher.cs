using System;
using System.Collections.Concurrent;
using UnityEngine;

public sealed class SocketMainThreadDispatcher : MonoBehaviour
{
    public static SocketMainThreadDispatcher Instance { get; private set; }

    private readonly ConcurrentQueue<Action> _pendingActions = new ConcurrentQueue<Action>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Enqueue(Action action)
    {
        if (action == null)
        {
            return;
        }

        _pendingActions.Enqueue(action);
    }

    private void Update()
    {
        int safeCount = 0;
        while (_pendingActions.TryDequeue(out var action))
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                SocketLogger.Error("MainThreadDispatcher", $"action error: {ex.Message}");
            }

            safeCount++;
            if (safeCount > 1000)
            {
                break;
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
