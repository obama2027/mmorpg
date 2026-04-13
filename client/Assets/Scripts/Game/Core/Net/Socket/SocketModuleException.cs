using System;

public sealed class SocketModuleException : Exception
{
    public SocketErrorCode ErrorCode { get; }
    public string Detail { get; }

    public SocketModuleException(SocketErrorCode errorCode, string message, string detail = null, Exception inner = null)
        : base(message, inner)
    {
        ErrorCode = errorCode;
        Detail = detail;
    }

    public override string ToString()
    {
        return $"[{ErrorCode}] {Message} detail={Detail}\n{base.ToString()}";
    }

    public static SocketModuleException ConnectFailed(string detail, Exception inner = null)
    {
        return new SocketModuleException(SocketErrorCode.ConnectFailed, "Socket connect failed.", detail, inner);
    }

    public static SocketModuleException ConnectTimeout(string detail)
    {
        return new SocketModuleException(SocketErrorCode.ConnectTimeout, "Socket connect timeout.", detail);
    }

    public static SocketModuleException SendFailed(string detail, Exception inner = null)
    {
        return new SocketModuleException(SocketErrorCode.SendFailed, "Socket send failed.", detail, inner);
    }

    public static SocketModuleException ReceiveFailed(string detail, Exception inner = null)
    {
        return new SocketModuleException(SocketErrorCode.ReceiveFailed, "Socket receive failed.", detail, inner);
    }

    public static SocketModuleException PacketFramingError(string detail)
    {
        return new SocketModuleException(SocketErrorCode.PacketFramingError, "Packet framing error.", detail);
    }

    public static SocketModuleException ConnectionNotFound(int connectionId)
    {
        return new SocketModuleException(SocketErrorCode.ConnectionNotFound, "Connection not found.", $"connectionId={connectionId}");
    }

    public static SocketModuleException InvalidConfig(string detail)
    {
        return new SocketModuleException(SocketErrorCode.InvalidConfig, "Invalid socket config.", detail);
    }

    public static SocketModuleException AlreadyConnected(int connectionId)
    {
        return new SocketModuleException(SocketErrorCode.AlreadyConnected, "Connection already connected.", $"connectionId={connectionId}");
    }

    public static SocketModuleException ReconnectFailed(string detail, Exception inner = null)
    {
        return new SocketModuleException(SocketErrorCode.ReconnectFailed, "Reconnect failed.", detail, inner);
    }

    public static SocketModuleException ProtobufDeserializeError(string detail, Exception inner = null)
    {
        return new SocketModuleException(SocketErrorCode.ProtobufDeserializeError, "Protobuf deserialize error.", detail, inner);
    }

    public static SocketModuleException ConnectionClosed(int connectionId)
    {
        return new SocketModuleException(SocketErrorCode.ConnectionClosed, "Connection closed.", $"connectionId={connectionId}");
    }

    public static SocketModuleException PacketTooLarge(int size, int max)
    {
        return new SocketModuleException(SocketErrorCode.PacketTooLarge, "Packet too large.", $"size={size} max={max}");
    }
}
