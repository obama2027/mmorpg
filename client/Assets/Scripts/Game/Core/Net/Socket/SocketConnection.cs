using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public sealed class SocketConnection : IDisposable
{
    private const int ThreadJoinTimeoutMs = 3000;

    public int Id { get; }
    public SocketConnectionConfig Config { get; }

    private volatile SocketConnectionState _state = SocketConnectionState.Disconnected;
    public SocketConnectionState State => _state;

    private Socket _socket;
    private readonly SocketPacketFramer _recvFramer = new SocketPacketFramer();
    private readonly ConcurrentQueue<ReceivedPacket> _recvQueue = new ConcurrentQueue<ReceivedPacket>();
    private readonly ConcurrentQueue<SendPacket> _sendQueue = new ConcurrentQueue<SendPacket>();
    private readonly ConcurrentQueue<SocketEvent> _eventQueue = new ConcurrentQueue<SocketEvent>();
    private int _sendQueueCount;

    private Thread _recvThread;
    private Thread _sendThread;
    private readonly ManualResetEventSlim _sendSignal = new ManualResetEventSlim(false);
    private volatile bool _disposed;
    private int _reconnectAttempts;

    private readonly Stopwatch _ioWatch = new Stopwatch();
    private long _lastRecvMs;
    private long _lastHeartbeatSendMs;

    public struct ReceivedPacket
    {
        public ushort Opcode;
        public byte[] Payload;
        public int PayloadLength;
    }

    private struct SendPacket
    {
        public byte[] Data;
        public int Length;
    }

    public struct SocketEvent
    {
        public SocketConnectionState OldState;
        public SocketConnectionState NewState;
    }

    public SocketConnection(int id, SocketConnectionConfig config)
    {
        Id = id;
        Config = config;
    }

    public void Connect()
    {
        if (_disposed)
        {
            throw SocketModuleException.ConnectionClosed(Id);
        }

        if (_state == SocketConnectionState.Connected || _state == SocketConnectionState.Connecting)
        {
            throw SocketModuleException.AlreadyConnected(Id);
        }

        _reconnectAttempts = 0;
        StartConnect();
    }

    public void Send(ushort opcode, byte[] payload, int payloadLength)
    {
        if (_disposed)
        {
            return;
        }

        if (_state != SocketConnectionState.Connected)
        {
            SocketLogger.Warn("Connection", $"send while not connected id={Id} state={_state}");
            return;
        }

        if (Config.SendQueueCapacity > 0 && _sendQueueCount >= Config.SendQueueCapacity)
        {
            SocketLogger.Warn("Connection", $"send queue full id={Id} capacity={Config.SendQueueCapacity} opcode={opcode}");
            return;
        }

        int packetLength = SocketPacketFramer.EncodePacket(opcode, payload, payloadLength, out var buffer);
        _sendQueue.Enqueue(new SendPacket { Data = buffer, Length = packetLength });
        Interlocked.Increment(ref _sendQueueCount);
        _sendSignal.Set();
    }

    public int DrainReceivedPackets(List<ReceivedPacket> output)
    {
        int count = 0;
        while (_recvQueue.TryDequeue(out var packet))
        {
            output.Add(packet);
            count++;
        }

        return count;
    }

    public int DrainEvents(List<SocketEvent> output)
    {
        int count = 0;
        while (_eventQueue.TryDequeue(out var evt))
        {
            output.Add(evt);
            count++;
        }

        return count;
    }

    public void Disconnect()
    {
        if (_disposed)
        {
            return;
        }

        SetState(SocketConnectionState.Disconnected);
        CloseSocket();
        WaitIOThreadsExit();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        SetState(SocketConnectionState.Closed);
        CloseSocket();
        WaitIOThreadsExit();
        _sendSignal.Dispose();
        _ioWatch.Stop();

        while (_recvQueue.TryDequeue(out var pkt))
        {
            SocketPacketFramer.ReturnBuffer(pkt.Payload);
        }

        ReturnQueuedSendBuffers();
        while (_eventQueue.TryDequeue(out _)) { }
    }

    private void StartConnect()
    {
        SetState(SocketConnectionState.Connecting);

        var thread = new Thread(ConnectWorker)
        {
            IsBackground = true,
            Name = $"Socket-Connect-{Id}",
        };
        thread.Start();
    }

    private void ConnectWorker()
    {
        try
        {
            WaitIOThreadsExit();

            var socket = CreateSocket();
            var endpoints = ResolveEndpoints();

            if (endpoints.Length == 0)
            {
                throw SocketModuleException.ConnectFailed($"Cannot resolve host={Config.Host}");
            }

            bool connected = false;
            Exception lastEx = null;

            foreach (var ep in endpoints)
            {
                try
                {
                    var result = socket.BeginConnect(ep, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(Config.ConnectTimeoutMs);

                    if (success && socket.Connected)
                    {
                        socket.EndConnect(result);
                        connected = true;
                        break;
                    }

                    socket.Close();
                    socket = CreateSocket();
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    try { socket.Close(); } catch { }
                    socket = CreateSocket();
                }
            }

            if (!connected)
            {
                try { socket.Close(); } catch { }

                if (_disposed)
                {
                    return;
                }

                HandleConnectFailed(lastEx);
                return;
            }

            if (_disposed)
            {
                try { socket.Close(); } catch { }
                return;
            }

            _socket = socket;
            _reconnectAttempts = 0;
            _recvFramer.Reset();

            SetState(SocketConnectionState.Connected);
            SocketLogger.Info("Connection", $"connected id={Id} host={Config.Host}:{Config.Port}");

            StartIOThreads();
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                SocketLogger.Error("Connection", $"connect error id={Id} ex={ex.Message}");
                HandleConnectFailed(ex);
            }
        }
    }

    private Socket CreateSocket()
    {
        AddressFamily af;
        switch (Config.AddressFamily)
        {
            case SocketAddressFamily.IPv6:
                af = System.Net.Sockets.AddressFamily.InterNetworkV6;
                break;
            case SocketAddressFamily.DualStack:
                af = System.Net.Sockets.AddressFamily.InterNetworkV6;
                break;
            default:
                af = System.Net.Sockets.AddressFamily.InterNetwork;
                break;
        }

        SocketType socketType;
        ProtocolType protocolType;

        if (Config.Protocol == SocketProtocol.Udp)
        {
            socketType = SocketType.Dgram;
            protocolType = ProtocolType.Udp;
        }
        else
        {
            socketType = SocketType.Stream;
            protocolType = ProtocolType.Tcp;
        }

        var socket = new Socket(af, socketType, protocolType);

        if (Config.AddressFamily == SocketAddressFamily.DualStack)
        {
            socket.DualMode = true;
        }

        if (Config.Protocol == SocketProtocol.Tcp)
        {
            socket.NoDelay = true;
        }

        socket.SendBufferSize = Config.SendBufferSize;
        socket.ReceiveBufferSize = Config.RecvBufferSize;

        return socket;
    }

    private IPEndPoint[] ResolveEndpoints()
    {
        try
        {
            if (IPAddress.TryParse(Config.Host, out var directAddr))
            {
                return new[] { new IPEndPoint(directAddr, Config.Port) };
            }

            var addresses = Dns.GetHostAddresses(Config.Host);
            var results = new List<IPEndPoint>();

            foreach (var addr in addresses)
            {
                bool match = Config.AddressFamily switch
                {
                    SocketAddressFamily.IPv4 => addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork,
                    SocketAddressFamily.IPv6 => addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6,
                    SocketAddressFamily.DualStack => true,
                    _ => true,
                };

                if (match)
                {
                    results.Add(new IPEndPoint(addr, Config.Port));
                }
            }

            return results.ToArray();
        }
        catch (Exception ex)
        {
            SocketLogger.Error("Connection", $"DNS resolve failed id={Id} host={Config.Host} ex={ex.Message}");
            return Array.Empty<IPEndPoint>();
        }
    }

    private void StartIOThreads()
    {
        _ioWatch.Restart();
        Interlocked.Exchange(ref _lastRecvMs, 0);
        _lastHeartbeatSendMs = 0;

        _recvThread = new Thread(RecvLoop)
        {
            IsBackground = true,
            Name = $"Socket-Recv-{Id}",
        };
        _recvThread.Start();

        _sendThread = new Thread(SendLoop)
        {
            IsBackground = true,
            Name = $"Socket-Send-{Id}",
        };
        _sendThread.Start();
    }

    private void WaitIOThreadsExit()
    {
        var recv = _recvThread;
        var send = _sendThread;

        if (recv != null && recv != Thread.CurrentThread && recv.IsAlive)
        {
            recv.Join(ThreadJoinTimeoutMs);
        }

        if (send != null && send != Thread.CurrentThread && send.IsAlive)
        {
            send.Join(ThreadJoinTimeoutMs);
        }

        _recvThread = null;
        _sendThread = null;
    }

    private void RecvLoop()
    {
        byte[] buffer = new byte[Config.RecvBufferSize];

        try
        {
            while (!_disposed && _state == SocketConnectionState.Connected)
            {
                Socket s = _socket;
                if (s == null || !s.Connected)
                {
                    break;
                }

                int bytesRead;
                try
                {
                    bytesRead = s.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (_disposed) return;
                    SocketLogger.Error("RecvLoop", $"id={Id} socketError={ex.SocketErrorCode} ex={ex.Message}");
                    break;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }

                if (bytesRead <= 0)
                {
                    SocketLogger.Info("RecvLoop", $"id={Id} remote closed");
                    break;
                }

                Interlocked.Exchange(ref _lastRecvMs, _ioWatch.ElapsedMilliseconds);

                if (Config.Protocol == SocketProtocol.Udp)
                {
                    ProcessUdpDatagram(buffer, bytesRead);
                }
                else
                {
                    ProcessTcpData(buffer, bytesRead);
                }
            }
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                SocketLogger.Error("RecvLoop", $"id={Id} ex={ex.Message}");
            }
        }

        if (!_disposed)
        {
            HandleDisconnected();
        }
    }

    private void ProcessTcpData(byte[] buffer, int bytesRead)
    {
        _recvFramer.Append(buffer, 0, bytesRead);

        try
        {
            while (_recvFramer.TryReadPacket(out var opcode, out var payload, out var payloadLength))
            {
                if (IsHeartbeatResponse(opcode))
                {
                    SocketPacketFramer.ReturnBuffer(payload);
                    continue;
                }

                _recvQueue.Enqueue(new ReceivedPacket
                {
                    Opcode = opcode,
                    Payload = payload,
                    PayloadLength = payloadLength,
                });
            }
        }
        catch (SocketModuleException ex)
        {
            SocketLogger.Error("RecvLoop", $"id={Id} framing error={ex.Message}");
            HandleDisconnected();
        }
    }

    private void ProcessUdpDatagram(byte[] buffer, int bytesRead)
    {
        if (bytesRead < SocketPacketFramer.MinPacketSize)
        {
            SocketLogger.Warn("RecvLoop", $"id={Id} udp datagram too small size={bytesRead}");
            return;
        }

        int bodyLength = SocketPacketFramer.ReadUInt16BigEndian(buffer, 0);

        if (bodyLength < SocketPacketFramer.OpcodeSize ||
            SocketPacketFramer.HeaderSize + bodyLength > bytesRead)
        {
            SocketLogger.Warn("RecvLoop", $"id={Id} udp invalid bodyLength={bodyLength} bytesRead={bytesRead}");
            return;
        }

        ushort opcode = SocketPacketFramer.ReadUInt16BigEndian(buffer, SocketPacketFramer.HeaderSize);
        int payloadLength = bodyLength - SocketPacketFramer.OpcodeSize;

        if (IsHeartbeatResponse(opcode))
        {
            return;
        }

        byte[] payload = null;
        if (payloadLength > 0)
        {
            payload = ArrayPool<byte>.Shared.Rent(payloadLength);
            Buffer.BlockCopy(buffer, SocketPacketFramer.HeaderSize + SocketPacketFramer.OpcodeSize, payload, 0, payloadLength);
        }

        _recvQueue.Enqueue(new ReceivedPacket
        {
            Opcode = opcode,
            Payload = payload,
            PayloadLength = payloadLength,
        });
    }

    private void SendLoop()
    {
        try
        {
            while (!_disposed && _state == SocketConnectionState.Connected)
            {
                _sendSignal.Wait(100);
                _sendSignal.Reset();

                if (!ProcessHeartbeat())
                {
                    return;
                }

                while (_sendQueue.TryDequeue(out var pkt))
                {
                    Interlocked.Decrement(ref _sendQueueCount);
                    bool success = SendPacketData(pkt.Data, pkt.Length);
                    SocketPacketFramer.ReturnBuffer(pkt.Data);

                    if (!success)
                    {
                        return;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                SocketLogger.Error("SendLoop", $"id={Id} ex={ex.Message}");
            }
        }
        finally
        {
            ReturnQueuedSendBuffers();
        }
    }

    private bool ProcessHeartbeat()
    {
        if (Config.HeartbeatIntervalMs <= 0)
        {
            return true;
        }

        long nowMs = _ioWatch.ElapsedMilliseconds;

        if (Config.HeartbeatTimeoutMs > 0)
        {
            long lastRecv = Interlocked.Read(ref _lastRecvMs);
            if (nowMs - lastRecv >= Config.HeartbeatTimeoutMs)
            {
                SocketLogger.Error("Heartbeat", $"id={Id} timeout, no data for {Config.HeartbeatTimeoutMs}ms");
                CloseSocket();
                return false;
            }
        }

        if (nowMs - _lastHeartbeatSendMs >= Config.HeartbeatIntervalMs)
        {
            if (!SendHeartbeat())
            {
                return false;
            }

            _lastHeartbeatSendMs = nowMs;
        }

        return true;
    }

    private bool SendHeartbeat()
    {
        int len = SocketPacketFramer.EncodePacket(Config.HeartbeatRequestOpcode, null, 0, out var buffer);
        bool success = SendPacketData(buffer, len);
        SocketPacketFramer.ReturnBuffer(buffer);

        if (success)
        {
            SocketLogger.Info("Heartbeat", $"id={Id} sent");
        }

        return success;
    }

    private bool IsHeartbeatResponse(ushort opcode)
    {
        return Config.HeartbeatIntervalMs > 0 && opcode == Config.HeartbeatResponseOpcode;
    }

    private bool SendPacketData(byte[] data, int length)
    {
        Socket s = _socket;
        if (s == null || !s.Connected)
        {
            return false;
        }

        try
        {
            int sent = 0;
            while (sent < length)
            {
                int n = s.Send(data, sent, length - sent, SocketFlags.None);
                if (n <= 0)
                {
                    SocketLogger.Error("SendLoop", $"id={Id} send returned {n}");
                    return false;
                }

                sent += n;
            }

            return true;
        }
        catch (SocketException ex)
        {
            if (!_disposed)
            {
                SocketLogger.Error("SendLoop", $"id={Id} socketError={ex.SocketErrorCode} ex={ex.Message}");
            }

            return false;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }

    private void ReturnQueuedSendBuffers()
    {
        while (_sendQueue.TryDequeue(out var pkt))
        {
            Interlocked.Decrement(ref _sendQueueCount);
            SocketPacketFramer.ReturnBuffer(pkt.Data);
        }
    }

    private void HandleDisconnected()
    {
        CloseSocket();
        _ioWatch.Stop();

        if (_disposed || _state == SocketConnectionState.Closed)
        {
            return;
        }

        if (Config.AutoReconnect && _reconnectAttempts < Config.MaxReconnectAttempts)
        {
            SetState(SocketConnectionState.Reconnecting);
            StartReconnect();
        }
        else
        {
            SetState(SocketConnectionState.Disconnected);

            if (Config.AutoReconnect)
            {
                SocketLogger.Error("Connection", $"id={Id} max reconnect attempts reached ({Config.MaxReconnectAttempts})");
            }
        }
    }

    private void HandleConnectFailed(Exception ex)
    {
        if (Config.AutoReconnect && _reconnectAttempts < Config.MaxReconnectAttempts)
        {
            SetState(SocketConnectionState.Reconnecting);
            StartReconnect();
        }
        else
        {
            SetState(SocketConnectionState.Disconnected);
            SocketLogger.Error("Connection", $"id={Id} connect failed permanently ex={ex?.Message}");
        }
    }

    private void StartReconnect()
    {
        _reconnectAttempts++;
        SocketLogger.Info("Connection", $"id={Id} reconnecting attempt={_reconnectAttempts}/{Config.MaxReconnectAttempts}");

        var thread = new Thread(() =>
        {
            Thread.Sleep(Config.ReconnectIntervalMs);

            if (_disposed)
            {
                return;
            }

            ConnectWorker();
        })
        {
            IsBackground = true,
            Name = $"Socket-Reconnect-{Id}",
        };
        thread.Start();
    }

    private void CloseSocket()
    {
        var s = Interlocked.Exchange(ref _socket, null);
        if (s == null)
        {
            return;
        }

        try
        {
            if (s.Connected)
            {
                s.Shutdown(SocketShutdown.Both);
            }
        }
        catch { }

        try
        {
            s.Close();
        }
        catch { }
    }

    private void SetState(SocketConnectionState newState)
    {
        var old = _state;
        if (old == newState)
        {
            return;
        }

        _state = newState;
        _eventQueue.Enqueue(new SocketEvent { OldState = old, NewState = newState });
    }
}
