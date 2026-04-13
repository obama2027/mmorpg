using System;
using System.Collections.Generic;
using Google.Protobuf;

public sealed class SocketOpcodeDispatcher
{
    private readonly Dictionary<ushort, Action<int, byte[], int>> _handlers = new Dictionary<ushort, Action<int, byte[], int>>();

    public void Register<T>(ushort opcode, Action<int, T> handler) where T : IMessage<T>, new()
    {
        var parser = new MessageParser<T>(() => new T());

        _handlers[opcode] = (connectionId, payload, payloadLength) =>
        {
            T msg;
            try
            {
                if (payload == null || payloadLength == 0)
                {
                    msg = new T();
                }
                else
                {
                    var input = new CodedInputStream(payload, 0, payloadLength);
                    msg = parser.ParseFrom(input);
                }
            }
            catch (Exception ex)
            {
                throw SocketModuleException.ProtobufDeserializeError($"opcode={opcode} type={typeof(T).Name}", ex);
            }

            handler(connectionId, msg);
        };

        SocketLogger.Info("Dispatcher", $"registered opcode={opcode} type={typeof(T).Name}");
    }

    public void RegisterRaw(ushort opcode, Action<int, byte[], int> handler)
    {
        _handlers[opcode] = handler;
        SocketLogger.Info("Dispatcher", $"registered raw opcode={opcode}");
    }

    public void Unregister(ushort opcode)
    {
        _handlers.Remove(opcode);
    }

    public void UnregisterAll()
    {
        _handlers.Clear();
    }

    public void Dispatch(int connectionId, ushort opcode, byte[] payload, int payloadLength)
    {
        if (_handlers.TryGetValue(opcode, out var handler))
        {
            handler(connectionId, payload, payloadLength);
        }
        else
        {
            SocketLogger.Warn("Dispatcher", $"unhandled opcode={opcode} connectionId={connectionId} payloadSize={payloadLength}");
        }
    }

    public bool HasHandler(ushort opcode)
    {
        return _handlers.ContainsKey(opcode);
    }
}
