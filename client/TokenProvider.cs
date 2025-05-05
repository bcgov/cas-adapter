namespace Client;

public class TokenProvider(Model.Settings.Client settings, ILogger<TokenProvider> logger) : ITokenProvider
{
    public async Task<string> GetAccessTokenAsync()
    {
        // TODO use httpClientFactory
        var httpClient = new HttpClient();
        var base64 = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", settings.Id, settings.Secret)));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);
        var request = new HttpRequestMessage(HttpMethod.Post, settings.TokenUrl);
        var formData = new List<KeyValuePair<string, string>>();
        formData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
        request.Content = new FormUrlEncodedContent(formData);

        var response = await httpClient.SendAsync(request);
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError($"Error getting token: {response.StatusCode} - {response.Content}");
            return null;
        }
        string responseBody = await response.Content.ReadAsStringAsync();
        var jo = JObject.Parse(responseBody);
        var bearerToken = jo["access_token"].ToString();
        return bearerToken;
    }

    public Task<string> RefreshTokensAsync()
    {
        throw new NotImplementedException();
    }
}
