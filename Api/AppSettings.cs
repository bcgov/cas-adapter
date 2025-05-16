namespace Api;

public class AppSettings : IAppSettings
{
    public Splunk Splunk { get; private set; }
    public IConfiguration Configuration { get; private set; }
    public IHostEnvironment Environment { get; private set; }
    public Auth Auth { get; private set; }
    public Model.Settings.Client Client { get; private set; }

    public bool IsProduction => Environment.IsProduction();
    public bool IsDevelopment => Environment.IsDevelopment();

    public AppSettings(IConfiguration configuration, IHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
        Splunk = new Splunk
        {
            Url = configuration["SPLUNK_URL"],
            Token = configuration["SPLUNK_TOKEN"]
        };
        Auth = configuration.GetSection("auth").Get<Auth>();
        Client = new Model.Settings.Client
        {
            Id = configuration["ClientId"],
            Secret = configuration["ClientKey"],
            BaseUrl = configuration["BaseUrl"]?.ToString().Trim('/'),
            TokenUrl = configuration["TokenUrl"]?.ToString().Trim('/')
        };
    }
}

public static class AppSettingsExtensions
{
    public static AppSettings AddAppSettings(this IServiceCollection services, IWebHostEnvironment environment)
    {
        // configuration binded using user secrets
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables()
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // app settings 
        var appSettings = new AppSettings(configuration, environment);
        services.AddSingleton<IAppSettings>(appSettings);
        return appSettings;
    }
}