public enum SocketErrorCode
{
    Unknown = 0,
    ConnectFailed = 2001,
    ConnectTimeout = 2002,
    SendFailed = 2003,
    ReceiveFailed = 2004,
    PacketFramingError = 2005,
    ConnectionNotFound = 2006,
    InvalidConfig = 2007,
    AlreadyConnected = 2008,
    ReconnectFailed = 2009,
    ProtobufDeserializeError = 2010,
    ConnectionClosed = 2011,
    PacketTooLarge = 2012,
}
