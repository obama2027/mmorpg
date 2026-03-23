using System.Collections.Generic;

public sealed class HttpResponseData
{
    public long StatusCode;
    public string Text;
    public byte[] Bytes;
    public Dictionary<string, string> Headers;
    public string Url;
}
