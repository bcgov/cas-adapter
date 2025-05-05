public class TokenDelegatingHandler : DelegatingHandler
{
    private readonly ITokenProvider _tokenProvider;

    public TokenDelegatingHandler(ITokenProvider tokenProvider) => _tokenProvider = tokenProvider;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _tokenProvider.GetAccessTokenAsync());
        return await base.SendAsync(request, cancellationToken);
    }
}
