using UnityEngine.Networking;

public static class HttpRetryPolicy
{
    public static bool ShouldRetry(UnityWebRequest request, bool retryOnHttp4xx)
    {
        if (request == null)
        {
            return false;
        }

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.DataProcessingError)
        {
            return true;
        }

        if (request.result == UnityWebRequest.Result.ProtocolError)
        {
            long code = request.responseCode;
            if (code == 408 || code == 429)
            {
                return true;
            }

            if (code >= 500 && code <= 599)
            {
                return true;
            }

            if (retryOnHttp4xx && code >= 400 && code <= 499)
            {
                return true;
            }
        }

        return false;
    }
}
