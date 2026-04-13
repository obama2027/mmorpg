using System.Threading.Tasks;

public interface IHttpClient
{
    Task<HttpResponseData> SendAsync(string method, string url, byte[] body = null, string contentType = null, HttpRequestOptions options = null);

    Task<string> GetStringAsync(string url, HttpRequestOptions options = null);
    Task<byte[]> GetBytesAsync(string url, HttpRequestOptions options = null);

    Task<string> PostJsonAsync(string url, string jsonBody, HttpRequestOptions options = null);
    Task<string> PostFormAsync(string url, string formData, HttpRequestOptions options = null);
    Task<HttpResponseData> PostAsync(string url, byte[] body, string contentType = null, HttpRequestOptions options = null);
}
