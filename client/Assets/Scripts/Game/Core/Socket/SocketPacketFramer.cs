using System;
using System.Buffers;

/// <summary>
/// 包结构: [2字节包体长度(大端序)] [2字节opcode(大端序)] [N字节protobuf payload]
/// 其中包体长度 = 2(opcode) + N(payload)
/// 与 Skynet gate/netpack 原生 2 字节长度头兼容。
/// 所有 out byte[] 均从 ArrayPool 租借，调用方用完后必须调用 ReturnBuffer 归还。
/// </summary>
public sealed class SocketPacketFramer
{
    public const int HeaderSize = 2;
    public const int OpcodeSize = 2;
    public const int MinPacketSize = HeaderSize + OpcodeSize;
    public const int MaxBodySize = 65535 - OpcodeSize;

    private byte[] _buffer;
    private int _writePos;

    public SocketPacketFramer(int initialCapacity = 8192)
    {
        _buffer = new byte[initialCapacity];
        _writePos = 0;
    }

    public void Append(byte[] data, int offset, int count)
    {
        if (count <= 0)
        {
            return;
        }

        EnsureCapacity(_writePos + count);
        Buffer.BlockCopy(data, offset, _buffer, _writePos, count);
        _writePos += count;
    }

    /// <summary>
    /// 尝试从缓冲区提取一个完整包。payload 从 ArrayPool 租借，调用方必须用 ReturnBuffer 归还。
    /// </summary>
    public bool TryReadPacket(out ushort opcode, out byte[] payload, out int payloadLength)
    {
        opcode = 0;
        payload = null;
        payloadLength = 0;

        if (_writePos < HeaderSize)
        {
            return false;
        }

        int bodyLength = ReadUInt16BigEndian(_buffer, 0);

        if (bodyLength < OpcodeSize)
        {
            throw SocketModuleException.PacketFramingError($"bodyLength too small: {bodyLength}");
        }

        if (bodyLength > MaxBodySize)
        {
            throw SocketModuleException.PacketTooLarge(bodyLength, MaxBodySize);
        }

        int totalLength = HeaderSize + bodyLength;

        if (_writePos < totalLength)
        {
            return false;
        }

        opcode = ReadUInt16BigEndian(_buffer, HeaderSize);
        payloadLength = bodyLength - OpcodeSize;

        if (payloadLength > 0)
        {
            payload = ArrayPool<byte>.Shared.Rent(payloadLength);
            Buffer.BlockCopy(_buffer, HeaderSize + OpcodeSize, payload, 0, payloadLength);
        }

        int remaining = _writePos - totalLength;
        if (remaining > 0)
        {
            Buffer.BlockCopy(_buffer, totalLength, _buffer, 0, remaining);
        }

        _writePos = remaining;
        return true;
    }

    /// <summary>
    /// 编码一个包。返回实际数据长度，buffer 从 ArrayPool 租借，调用方必须用 ReturnBuffer 归还。
    /// </summary>
    public static int EncodePacket(ushort opcode, byte[] payload, int payloadLength, out byte[] buffer)
    {
        int bodyLength = OpcodeSize + payloadLength;
        int totalLength = HeaderSize + bodyLength;
        buffer = ArrayPool<byte>.Shared.Rent(totalLength);

        WriteUInt16BigEndian(buffer, 0, (ushort)bodyLength);
        WriteUInt16BigEndian(buffer, HeaderSize, opcode);

        if (payloadLength > 0 && payload != null)
        {
            Buffer.BlockCopy(payload, 0, buffer, HeaderSize + OpcodeSize, payloadLength);
        }

        return totalLength;
    }

    /// <summary>
    /// 归还从 ArrayPool 租借的缓冲区。对 null 或空数组安全。
    /// </summary>
    public static void ReturnBuffer(byte[] buffer)
    {
        if (buffer != null && buffer.Length > 0)
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public void Reset()
    {
        _writePos = 0;
    }

    private void EnsureCapacity(int required)
    {
        if (_buffer.Length >= required)
        {
            return;
        }

        int newSize = _buffer.Length;
        while (newSize < required)
        {
            newSize *= 2;
        }

        byte[] newBuffer = new byte[newSize];
        if (_writePos > 0)
        {
            Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _writePos);
        }

        _buffer = newBuffer;
    }

    public static ushort ReadUInt16BigEndian(byte[] buffer, int offset)
    {
        return (ushort)((buffer[offset] << 8) | buffer[offset + 1]);
    }

    private static void WriteUInt16BigEndian(byte[] buffer, int offset, ushort value)
    {
        buffer[offset] = (byte)(value >> 8);
        buffer[offset + 1] = (byte)value;
    }
}
