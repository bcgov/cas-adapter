namespace Client;

public interface ICasHttpClient
{
    Task<Response> Get(string url, bool isRetryEnabled = false);
    Task<Response> Post(string url, string payload, bool isRetryEnabled = false);
}