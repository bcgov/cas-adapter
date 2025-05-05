namespace Client;

public class TokenProvider : ITokenProvider
{
    public async Task<string> GetAccessTokenAsync()
    {
        // TODO ignore ssl errors in non-production mode
        var _httpClient = new HttpClient();
        // TODO use AppSettings
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "_QpLDOPASoDSk7iLflf6Hw..", "o9twG7c-PhaGhZoWmlzvQg.."))));
        // TODO use AppSettings
        var request = new HttpRequestMessage(HttpMethod.Post, "https://cfs-systws.cas.gov.bc.ca:7025/ords/cas/oauth/token");
        var formData = new List<KeyValuePair<string, string>>();
        formData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
        request.Content = new FormUrlEncodedContent(formData);

        var response = await _httpClient.SendAsync(request);
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        if (!response.IsSuccessStatusCode)
        {
            //logger.LogError($"Error getting token: {response.StatusCode} - {response.Content}");
            //return response.StatusCode;
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
