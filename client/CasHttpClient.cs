namespace Client;

public interface ICasHttpClient 
{
    Task<Response> Get(string url);
    Task<Response> Post(string url, string payload);
}

public class CasHttpClient : ICasHttpClient
{
    private readonly HttpClient _httpClient = null;

    public CasHttpClient(HttpClient httpClient, Model.Settings.Client settings)
    {
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.BaseAddress = new Uri(settings.BaseUrl);
        httpClient.Timeout = new TimeSpan(1, 0, 0);  // 1 hour timeout 
        _httpClient = httpClient;
    }

    public async Task<Response> Get(string url)
    {
        var response = await _httpClient.GetAsync(url);
        var responseContent = await response.Content.ReadAsStringAsync();
        return new Response(responseContent, response.StatusCode);
    }

    public async Task<Response> Post(string url, string payload)
    {
        var postContent = new StringContent(payload);
        var response = await _httpClient.PostAsync(url, postContent);
        var responseContent = await response.Content.ReadAsStringAsync();
        return new(responseContent, response.StatusCode);
    }
}

