using System;
using UnityEngine;
using Google.Protobuf;
using System.Text;

public sealed class SocketService : MonoSingle<SocketService>
{
    public static event Action OnReady;
    public bool IsReady { get; private set; }

    [Header("Log Switch")]
    [SerializeField] private bool _enableSocketInfoLog = true;
    [SerializeField] private bool _enableSocketWarningLog = true;
    [SerializeField] private bool _enableSocketErrorLog = true;

    private readonly SocketOpcodeDispatcher _dispatcher = new SocketOpcodeDispatcher();

    public event SocketStateChangedHandler OnConnectionStateChanged;

    private bool _isInit = false;
    public override void Init()
    {
        if (_isInit) return;
        SocketLogger.EnableInfoLog = _enableSocketInfoLog;
        SocketLogger.EnableWarningLog = _enableSocketWarningLog;
        SocketLogger.EnableErrorLog = _enableSocketErrorLog;

        IsReady = true;
        OnReady?.Invoke();
        SocketLogger.Info("SocketService", "initialized");
    }

    private void Update()
    {
        SocketManager.Instance.PollAllConnections(_dispatcher, OnConnectionStateChanged);
    }

    /// <summary>
    /// 创建一个新的 Socket 连接实例，返回连接 ID。
    /// </summary>
    public int CreateConnection(SocketConnectionConfig config)
    {
        return SocketManager.Instance.CreateConnection(config);
    }

    /// <summary>
    /// 创建使用默认配置的连接并返回连接 ID。
    /// </summary>
    public int CreateConnection(string host, int port)
    {
        var config = SocketConnectionConfig.Default(host, port);
        return SocketManager.Instance.CreateConnection(config);
    }

    /// <summary>
    /// 发起连接。
    /// </summary>
    public void Connect(int connectionId)
    {
        SocketManager.Instance.Connect(connectionId);
    }

    /// <summary>
    /// 创建并立即连接，返回连接 ID。
    /// </summary>
    public int CreateAndConnect(SocketConnectionConfig config)
    {
        int id = SocketManager.Instance.CreateConnection(config);
        SocketManager.Instance.Connect(id);
        return id;
    }

    /// <summary>
    /// 创建并立即连接（默认配置），返回连接 ID。
    /// </summary>
    public int CreateAndConnect(string host, int port)
    {
        var config = SocketConnectionConfig.Default(host, port);
        int id = SocketManager.Instance.CreateConnection(config);
        SocketManager.Instance.Connect(id);
        return id;
    }

    /// <summary>
    /// 发送 protobuf 消息。
    /// </summary>
    public void Send(int connectionId, ushort opcode, IMessage message)
    {
        if (message != null)
        {
            byte[] payload = message.ToByteArray();
            SocketManager.Instance.Send(connectionId, opcode, payload, payload.Length);
        }
        else
        {
            SocketManager.Instance.Send(connectionId, opcode, null, 0);
        }
    }

    /// <summary>
    /// 发送原始字节数据。
    /// </summary>
    public void SendRaw(int connectionId, ushort opcode, byte[] payload)
    {
        SocketManager.Instance.Send(connectionId, opcode, payload, payload != null ? payload.Length : 0);
    }

    /// <summary>
    /// 注册 protobuf 消息处理器。
    /// </summary>
    public void RegisterHandler<T>(ushort opcode, Action<int, T> handler) where T : IMessage<T>, new()
    {
        _dispatcher.Register(opcode, handler);
    }

    /// <summary>
    /// 注册原始字节处理器。payload 来自 ArrayPool，不要持有引用，需要保留数据请自行拷贝。
    /// </summary>
    public void RegisterRawHandler(ushort opcode, Action<int, byte[], int> handler)
    {
        _dispatcher.RegisterRaw(opcode, handler);
    }

    /// <summary>
    /// 取消注册指定 opcode 的处理器。
    /// </summary>
    public void UnregisterHandler(ushort opcode)
    {
        _dispatcher.Unregister(opcode);
    }

    /// <summary>
    /// 取消所有处理器。
    /// </summary>
    public void UnregisterAllHandlers()
    {
        _dispatcher.UnregisterAll();
    }

    /// <summary>
    /// 断开指定连接（可自动重连）。
    /// </summary>
    public void Disconnect(int connectionId)
    {
        SocketManager.Instance.Disconnect(connectionId);
    }

    /// <summary>
    /// 销毁指定连接（释放资源，不会重连）。
    /// </summary>
    public void DestroyConnection(int connectionId)
    {
        SocketManager.Instance.DestroyConnection(connectionId);
    }

    /// <summary>
    /// 断开所有连接。
    /// </summary>
    public void DisconnectAll()
    {
        SocketManager.Instance.DisconnectAll();
    }

    /// <summary>
    /// 销毁所有连接。
    /// </summary>
    public void DestroyAll()
    {
        SocketManager.Instance.DestroyAll();
    }

    /// <summary>
    /// 获取指定连接的当前状态。
    /// </summary>
    public SocketConnectionState GetConnectionState(int connectionId)
    {
        return SocketManager.Instance.GetConnectionState(connectionId);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SocketManager.Instance.DestroyAll();
            _dispatcher.UnregisterAll();
            SocketLogger.Info("SocketService", "destroyed");
        }
    }
}
