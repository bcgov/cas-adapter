namespace Client;

public class TokenProvider(HttpClient httpClient, IPolicyProvider policyProvider, IAppSettings appSettings, ILogger<TokenProvider> logger) : ITokenProvider
{
    private string bearerToken;

    public async Task<string> GetAccessTokenAsync()
    {
        if (bearerToken == null)
            await RefreshTokenAsync();
        return bearerToken;
    }

    public async Task RefreshTokenAsync()
    {
        appSettings?.Client?.Id.ThrowIfNullOrEmpty();
        appSettings?.Client?.Secret.ThrowIfNullOrEmpty();
        appSettings?.Client?.TokenUrl.ThrowIfNullOrEmpty();

        try
        {
            var base64 = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", appSettings.Client.Id, appSettings.Client.Secret)));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);
            var request = new HttpRequestMessage(HttpMethod.Post, appSettings.Client.TokenUrl);
            var formData = new List<KeyValuePair<string, string>>();
            formData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            request.Content = new FormUrlEncodedContent(formData);
            var response = await httpClient.SendAsync(request);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError($"Error getting token: {response.StatusCode} - {response.Content}");
                throw new Exception("Unable to refresh token.");
            }
            string responseBody = await response.Content.ReadAsStringAsync();
            var jo = JObject.Parse(responseBody);
            bearerToken = jo["access_token"].ToString();
        }
        catch (Exception ex)
        {
            logger.LogError($"TokenProvider.RefreshTokenAsync failed: {ex.ToString()}");
            throw;
        }
    }
}
