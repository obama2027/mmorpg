using System;

public enum SocketProtocol
{
    Tcp = 0,
    Udp = 1,
}

public enum SocketAddressFamily
{
    IPv4 = 0,
    IPv6 = 1,
    DualStack = 2,
}

public enum SocketConnectionState
{
    Disconnected = 0,
    Connecting = 1,
    Connected = 2,
    Reconnecting = 3,
    Closed = 4,
}

[Serializable]
public struct SocketConnectionConfig
{
    public string Host;
    public int Port;
    public SocketProtocol Protocol;
    public SocketAddressFamily AddressFamily;
    public int ConnectTimeoutMs;
    public int ReconnectIntervalMs;
    public int MaxReconnectAttempts;
    public int SendBufferSize;
    public int RecvBufferSize;
    public bool AutoReconnect;
    public int HeartbeatIntervalMs;
    public int HeartbeatTimeoutMs;
    public ushort HeartbeatRequestOpcode;
    public ushort HeartbeatResponseOpcode;
    public int SendQueueCapacity;

    public static SocketConnectionConfig Default(string host, int port)
    {
        return new SocketConnectionConfig
        {
            Host = host,
            Port = port,
            Protocol = SocketProtocol.Tcp,
            AddressFamily = SocketAddressFamily.IPv4,
            ConnectTimeoutMs = 5000,
            ReconnectIntervalMs = 3000,
            MaxReconnectAttempts = 5,
            SendBufferSize = 65536,
            RecvBufferSize = 65536,
            AutoReconnect = true,
            HeartbeatIntervalMs = 0,
            HeartbeatTimeoutMs = 0,
            HeartbeatRequestOpcode = 0,
            HeartbeatResponseOpcode = 0,
            SendQueueCapacity = 1024,
        };
    }
}

public delegate void SocketMessageHandler(int connectionId, ushort opcode, byte[] payload);
public delegate void SocketStateChangedHandler(int connectionId, SocketConnectionState oldState, SocketConnectionState newState);
