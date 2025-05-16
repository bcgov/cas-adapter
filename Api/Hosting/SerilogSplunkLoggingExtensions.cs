namespace Api;

public static class SerilogSplunkLoggingExtensions
{
    // cofigure ILogger<T> provider
    // NOTE does not seem to change Log.Logger, use injected ILogger and not the static Log.Logger
    public static void AddLogging(this IServiceCollection services, AppSettings appSettings)
    {
        services.AddSerilog((hostingContext, loggerConfiguration) =>
        {
            loggerConfiguration
               .ReadFrom.Configuration(appSettings.Configuration)
               .Enrich.WithProperty("app", "cas-adapter");

            if (!appSettings.Environment.IsDevelopment())
            {
                var splunkUrl = appSettings.Splunk.Url;
                var splunkToken = appSettings.Splunk.Token;
                if (string.IsNullOrWhiteSpace(splunkToken) || string.IsNullOrWhiteSpace(splunkUrl))
                {
                    Log.Error($"Splunk logging sink is not configured properly, check SPLUNK_TOKEN and SPLUNK_URL env vars");
                }
                else
                {
                    loggerConfiguration
                        .WriteTo.EventCollector(
                            splunkHost: splunkUrl,
                            eventCollectorToken: splunkToken,
                            messageHandler: new HttpClientHandler
                            {
                                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                            },
                            renderTemplate: false);

                        Log.Information($"Logs will be forwarded to Splunk");
                }
            }
            else
            {
                Log.Information($"Logs will not be forwarded to Splunk");
            }
        });
    }

    /// <summary>
    /// Adds observability instruments like logging to the web application's middleware pipelines
    /// </summary>
    public static void UseLoggingMiddleware(this WebApplication webApplication)
    {
        webApplication.UseSerilogRequestLogging(opts =>
        {
            opts.GetLevel = GetLevel;
            opts.IncludeQueryInRequestPath = true;
            opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("User", httpContext.User.FindFirst("preferred_username")?.Value ?? string.Empty);
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
            };
        });
    }

    // add basic console logger on startup before Serilog is configured
    public static void CreateBootstrapLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();
    }

    // check endpoint is health check
    private static bool IsHealthCheckEndpoint(HttpContext ctx)
    {
        var endpoint = ctx.GetEndpoint();
        if (endpoint is object) // same as !(endpoint is null)
        {
            return string.Equals(
                endpoint.DisplayName,
                "Health checks",
                StringComparison.Ordinal);
        }
        // No endpoint, so not a health check endpoint
        return false;
    }

    // summary logs for health check requests use a Verbose level, while errors use Error and other requests use Information
    // filter out health check requests
    private static LogEventLevel GetLevel(HttpContext ctx, double _, Exception ex) =>
        ex != null
            ? LogEventLevel.Error
            : ctx.Response.StatusCode > 499
                ? LogEventLevel.Error
                : IsHealthCheckEndpoint(ctx) // Not an error, check if it was a health check
                    ? LogEventLevel.Verbose // Was a health check, use Verbose
                    : LogEventLevel.Information;

    public static IDisposable? PushProperty(this ILogger logger, string propertyName, object value)
    {
        return logger.BeginScope(WrapProperty(propertyName, value));
    }

    private static IEnumerable<KeyValuePair<string, object>> WrapProperty(string propertyName, object value)
    {
        yield return new KeyValuePair<string, object>(propertyName, value);
    }
}
