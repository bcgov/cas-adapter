namespace Client;

public class CasHttpClient : ICasHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IPolicyProvider _policyProvider;
    private readonly IAppSettings _appSettings;
    private readonly ILogger<CasHttpClient> _logger;

    public CasHttpClient(HttpClient httpClient, IPolicyProvider policyProvider, IAppSettings appSettings, ILogger<CasHttpClient> logger)
    {
        appSettings?.Client?.BaseUrl.ThrowIfNullOrEmpty();

        _policyProvider = policyProvider;
        _appSettings = appSettings;
        _logger = logger;

        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.BaseAddress = new Uri(appSettings.Client.BaseUrl);
        httpClient.Timeout = new TimeSpan(1, 0, 0);  // 1 hour timeout 
        _httpClient = httpClient;
    }

    public async Task<Response> Get(string url, bool isRetryEnabled = false)
    {
        HttpResponseMessage response;
        if (isRetryEnabled)
        {
            response = await _policyProvider.GetRetryPolicy().ExecuteAsync(() => _httpClient.GetAsync(url));
        }
        else
        {
            response = await _httpClient.GetAsync(url);
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        if (!_appSettings.IsProduction)
        {
            _logger.LogInformation("RESPONSE BODY {0}", responseContent);
        }
        var responseStatusCode = response.StatusCode;
        // this is a hack work-around for CAS returning 404 instead of 204. When we get a 404 and json response with "code" = "NotFound", we know CAS really means a 204, the object didn't exist e.g. the invoice number didn't exist in the database
        if (response.StatusCode == HttpStatusCode.NotFound && !string.IsNullOrEmpty(responseContent))
        {
            try
            {
                var json = JObject.Parse(responseContent);
                if (json.ContainsKey("code") && json.GetValue("code").Value<string>() == "NotFound")
                {
                    responseContent = null;
                    responseStatusCode = HttpStatusCode.NoContent;
                }
            } catch (Exception ex) {  }
        }
        return new Response(responseContent, responseStatusCode);
    }

    public async Task<Response> Post(string url, string payload, bool isRetryEnabled = false)
    {
        var postContent = new StringContent(payload);

        HttpResponseMessage response;
        if (isRetryEnabled)
        {
            response = await _policyProvider.GetRetryPolicy().ExecuteAsync(() => _httpClient.PostAsync(url, postContent));
        }
        else
        {
            response = await _httpClient.PostAsync(url, postContent);
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        if (!_appSettings.IsProduction && !string.IsNullOrEmpty(responseContent))
        {
            _logger.LogInformation("RESPONSE BODY {0}", responseContent);
        }
        return new(responseContent, response.StatusCode);
    }
}

public static class CasHttpClientExtensions
{
    public static IServiceCollection AddCasHttpClient(this IServiceCollection services, bool isProduction)
    {
        services
            //.AddTransient<LogInvalidSslHttpClientHandler>()
            .AddTransient<IPolicyProvider, PollyPolicyProvider>()
            .AddTransient<ITokenProvider, TokenProvider>()
            .AddTransient<TokenDelegatingHandler>()
            .AddTransient<ICasService, CasService>()
            .AddHttpClient<ICasHttpClient, CasHttpClient>()
                // TODO 
                //.ConfigurePrimaryHttpMessageHandler<LogInvalidSslHttpClientHandler>()
                .AddHttpMessageHandler<TokenDelegatingHandler>();

        return services;
    }
}
