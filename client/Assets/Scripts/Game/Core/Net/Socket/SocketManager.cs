using System;
using System.Collections.Generic;

public sealed class SocketManager
{
    private static readonly SocketManager s_instance = new SocketManager();
    public static SocketManager Instance => s_instance;

    private readonly Dictionary<int, SocketConnection> _connections = new Dictionary<int, SocketConnection>();
    private int _nextConnectionId;

    private readonly List<SocketConnection.ReceivedPacket> _packetBuffer = new List<SocketConnection.ReceivedPacket>();
    private readonly List<SocketConnection.SocketEvent> _eventBuffer = new List<SocketConnection.SocketEvent>();
    private readonly List<SocketConnection> _pollSnapshot = new List<SocketConnection>();

    private SocketManager() { }

    public int CreateConnection(SocketConnectionConfig config)
    {
        ValidateConfig(config);

        int id = ++_nextConnectionId;
        var conn = new SocketConnection(id, config);
        _connections[id] = conn;

        SocketLogger.Info("Manager", $"created connection id={id} host={config.Host}:{config.Port} protocol={config.Protocol}");
        return id;
    }

    public void Connect(int connectionId)
    {
        var conn = GetConnectionOrThrow(connectionId);
        conn.Connect();
    }

    public void Send(int connectionId, ushort opcode, byte[] payload, int payloadLength)
    {
        var conn = GetConnectionOrThrow(connectionId);
        conn.Send(opcode, payload, payloadLength);
    }

    public void Disconnect(int connectionId)
    {
        if (!_connections.TryGetValue(connectionId, out var conn))
        {
            return;
        }

        conn.Disconnect();
    }

    public void DestroyConnection(int connectionId)
    {
        if (!_connections.TryGetValue(connectionId, out var conn))
        {
            return;
        }

        _connections.Remove(connectionId);
        conn.Dispose();

        SocketLogger.Info("Manager", $"destroyed connection id={connectionId}");
    }

    public void DisconnectAll()
    {
        foreach (var pair in _connections)
        {
            pair.Value.Disconnect();
        }
    }

    public void DestroyAll()
    {
        foreach (var pair in _connections)
        {
            pair.Value.Dispose();
        }

        _connections.Clear();
        SocketLogger.Info("Manager", "all connections destroyed");
    }

    public SocketConnectionState GetConnectionState(int connectionId)
    {
        var conn = GetConnectionOrThrow(connectionId);
        return conn.State;
    }

    public SocketConnection GetConnection(int connectionId)
    {
        _connections.TryGetValue(connectionId, out var conn);
        return conn;
    }

    public void PollAllConnections(SocketOpcodeDispatcher dispatcher, SocketStateChangedHandler stateHandler = null)
    {
        _pollSnapshot.Clear();
        foreach (var pair in _connections)
        {
            _pollSnapshot.Add(pair.Value);
        }

        for (int c = 0; c < _pollSnapshot.Count; c++)
        {
            var conn = _pollSnapshot[c];

            _eventBuffer.Clear();
            conn.DrainEvents(_eventBuffer);
            for (int i = 0; i < _eventBuffer.Count; i++)
            {
                var evt = _eventBuffer[i];
                try
                {
                    stateHandler?.Invoke(conn.Id, evt.OldState, evt.NewState);
                }
                catch (Exception ex)
                {
                    SocketLogger.Error("Manager", $"stateHandler error id={conn.Id} ex={ex.Message}");
                }
            }

            _packetBuffer.Clear();
            conn.DrainReceivedPackets(_packetBuffer);
            for (int i = 0; i < _packetBuffer.Count; i++)
            {
                var pkt = _packetBuffer[i];
                try
                {
                    dispatcher.Dispatch(conn.Id, pkt.Opcode, pkt.Payload, pkt.PayloadLength);
                }
                catch (Exception ex)
                {
                    SocketLogger.Error("Manager", $"dispatch error id={conn.Id} opcode={pkt.Opcode} ex={ex.Message}");
                }

                SocketPacketFramer.ReturnBuffer(pkt.Payload);
            }
        }

        _pollSnapshot.Clear();
    }

    private SocketConnection GetConnectionOrThrow(int connectionId)
    {
        if (!_connections.TryGetValue(connectionId, out var conn))
        {
            throw SocketModuleException.ConnectionNotFound(connectionId);
        }

        return conn;
    }

    private static void ValidateConfig(SocketConnectionConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.Host))
        {
            throw SocketModuleException.InvalidConfig("Host is null or empty.");
        }

        if (config.Port <= 0 || config.Port > 65535)
        {
            throw SocketModuleException.InvalidConfig($"Invalid port: {config.Port}");
        }

        if (config.ConnectTimeoutMs <= 0)
        {
            throw SocketModuleException.InvalidConfig($"Invalid ConnectTimeoutMs: {config.ConnectTimeoutMs}");
        }

        if (config.SendBufferSize <= 0)
        {
            throw SocketModuleException.InvalidConfig($"Invalid SendBufferSize: {config.SendBufferSize}");
        }

        if (config.RecvBufferSize <= 0)
        {
            throw SocketModuleException.InvalidConfig($"Invalid RecvBufferSize: {config.RecvBufferSize}");
        }
    }
}
