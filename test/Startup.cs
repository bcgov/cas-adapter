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
            .AddTransient<TokenDelegatingHandler>()
            .AddTransient<ICasService, CasService>()
            .AddHttpClient<ICasHttpClient, CasHttpClient>()
                .AddHttpMessageHandler<TokenDelegatingHandler>();
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