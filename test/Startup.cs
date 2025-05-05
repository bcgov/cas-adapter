using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

public class Startup
{
    /// <summary>
    /// Register dependencies needed for xunit tests
    /// NOTE to register dependencies used by making calls from HttpClient, use CustomWebApplicationFactory
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        var fakeEnvironment = new FakeEnvironment
        {
            EnvironmentName = "Development",
        };
        services.AddAppSettings(fakeEnvironment);

        services
            .AddTransient<ITokenProvider, TokenProvider>()
            .AddTransient<TokenAuthHeaderHandler>()
            .AddTransient<ICasService, CasService>()
            //.AddHttpClient<ICasHttpClient, CasHttpClient>()
            .AddHttpClient<TestService>()
                .AddHttpMessageHandler<TokenAuthHeaderHandler>();
                //.AddHttpMessageHandler((services) =>
                //{
                //    //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _tokenProvider.GetAccessTokenAsync());
                //    //return await base.SendAsync(request, cancellationToken);
                //    return new TokenAuthHeaderHandler(services.GetRequiredService<ITokenProvider>());
                //});
    }

    public class FakeEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
        public string ApplicationName { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; }
    }
}

public class TestService
{
    private readonly HttpClient _httpClient;

    public TestService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.github.com");
    }

    public async Task<IEnumerable<object>?> GetAspNetCoreDocsBranchesAsync() =>
    await _httpClient.GetFromJsonAsync<IEnumerable<object>>(
        "repos/dotnet/AspNetCore.Docs/branches");
}

public class TypedClientModel : PageModel
{
    private readonly TestService _gitHubService;

    public TypedClientModel(TestService gitHubService) =>
        _gitHubService = gitHubService;

    public IEnumerable<object>? GitHubBranches { get; set; }

    public async Task OnGet()
    {
        try
        {
            GitHubBranches = await _gitHubService.GetAspNetCoreDocsBranchesAsync();
        }
        catch (HttpRequestException)
        {
            // ...
        }
    }
}