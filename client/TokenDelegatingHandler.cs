public class TokenDelegatingHandler(ITokenProvider tokenProvider) : DelegatingHandler
{
    private readonly AsyncRetryPolicy<HttpResponseMessage> _policy = Policy
            .HandleResult<HttpResponseMessage>(r => r.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            .RetryAsync((_, _) => tokenProvider.RefreshTokensAsync());

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => await _policy.ExecuteAsync(async () =>
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await tokenProvider.GetAccessTokenAsync());
            return await base.SendAsync(request, cancellationToken);
        });
}
