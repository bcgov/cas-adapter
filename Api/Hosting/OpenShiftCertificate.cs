using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using RedHat.OpenShift.Utils;

// from https://raw.githubusercontent.com/redhat-developer/s2i-dotnetcore-ex/dotnetcore-2.1-https/app/OpenShift.cs

namespace Api;

public static class PlatformEnvironment
{
    public static bool IsOpenShift => !string.IsNullOrEmpty(OpenShiftEnvironment.BuildName);
}

public static class OpenShiftEnvironment
{
    private static string _buildCommit;
    private static string _buildName;
    private static string _buildSource;
    private static string _buildNamespace;
    private static string _buildReference;

    public static string BuildCommit => GetFromEnvironmentVariable("OPENSHIFT_BUILD_COMMIT", ref _buildCommit);
    public static string BuildName => GetFromEnvironmentVariable("OPENSHIFT_BUILD_NAME", ref _buildName);
    public static string BuildSource => GetFromEnvironmentVariable("OPENSHIFT_BUILD_SOURCE", ref _buildSource);

    public static string BuildNamespace =>
        GetFromEnvironmentVariable("OPENSHIFT_BUILD_NAMESPACE", ref _buildNamespace);

    public static string BuildReference =>
        GetFromEnvironmentVariable("OPENSHIFT_BUILD_REFERENCE", ref _buildReference);

    private static string GetFromEnvironmentVariable(string name, ref string cached)
    {
        if (cached == null) cached = Environment.GetEnvironmentVariable(name) ?? string.Empty;
        return cached;
    }
}

public class OpenShiftIntegrationOptions
{
    public string CertificateMountPoint { get; set; }

    internal bool UseHttps => !string.IsNullOrEmpty(CertificateMountPoint);
}

internal class KestrelOptionsSetup : IConfigureOptions<KestrelServerOptions>
{
    private readonly OpenShiftCertificateLoader _certificateLoader;
    private readonly IOptions<OpenShiftIntegrationOptions> _options;

    public KestrelOptionsSetup(IOptions<OpenShiftIntegrationOptions> options,
        OpenShiftCertificateLoader certificateLoader)
    {
        _options = options;
        _certificateLoader = certificateLoader;
    }

    public void Configure(KestrelServerOptions options)
    {
        if (_options.Value.UseHttps)
            options.ListenAnyIP(8080, configureListen =>
            {
                configureListen.UseHttps(_certificateLoader.ServiceCertificate);

                // enable Http2, for gRPC
                configureListen.Protocols = HttpProtocols.Http1AndHttp2;

                configureListen.UseConnectionLogging();
            });
        else
            options.ListenAnyIP(8080, configureListen =>
            {
                // enable Http2, for gRPC
                configureListen.Protocols = HttpProtocols.Http2;
                configureListen.UseConnectionLogging();
            });

        options.ConfigureHttpsDefaults(opt => opt.AllowAnyClientCertificate());

        // Also listen on port 8088 for health checks. Note that you won't be able to do gRPC calls on this port; 
        // it is only required because the OpenShift health check system does not seem to be compatible with HTTP2.
        options.ListenAnyIP(8088, configureListen => { configureListen.Protocols = HttpProtocols.Http1; });
    }
}

internal class OpenShiftCertificateExpiration : BackgroundService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly OpenShiftCertificateLoader _certificateLoader;
    private readonly ILogger<OpenShiftCertificateExpiration> _logger;
    private readonly IOptions<OpenShiftIntegrationOptions> _options;

    public OpenShiftCertificateExpiration(IOptions<OpenShiftIntegrationOptions> options,
        OpenShiftCertificateLoader certificateLoader, IHostApplicationLifetime applicationLifetime,
        ILogger<OpenShiftCertificateExpiration> logger)
    {
        _options = options;
        _certificateLoader = certificateLoader;
        _applicationLifetime = applicationLifetime;
        _logger = logger;
    }

    private static TimeSpan RestartSpan => TimeSpan.FromMinutes(15);
    private static TimeSpan NotAfterMargin => TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        if (_options.Value.UseHttps)
            try
            {
                var certificate = _certificateLoader.ServiceCertificate;
                bool loop;
                {
                    loop = false;
                    var expiresAt = certificate.NotAfter - NotAfterMargin; // NotAfter is in local time.
                    var now = DateTime.Now;
                    var tillExpires = expiresAt - now;
                    if (tillExpires > TimeSpan.Zero)
                        if (tillExpires > RestartSpan)
                        {
                            // Wait until we are in the RestartSpan.
                            var delay = tillExpires - RestartSpan
                                        + TimeSpan.FromSeconds(new Random().Next((int)RestartSpan.TotalSeconds));
                            if (delay.TotalMilliseconds > int.MaxValue)
                            {
                                // Task.Delay is limited to int.MaxValue.
                                await Task.Delay(int.MaxValue, token);
                                loop = true;
                            }
                            else
                            {
                                await Task.Delay(delay, token);
                            }
                        }
                }
                while (loop) ;
                // Our certificate expired, Stop the application.
                _logger.LogInformation(
                    $"Certificate expires at {certificate.NotAfter.ToUniversalTime()}. Stopping application.");
                _applicationLifetime.StopApplication();
            }
            catch (TaskCanceledException)
            {
            }
    }
}

internal class OpenShiftCertificateLoader
{
    private readonly IOptions<OpenShiftIntegrationOptions> _options;
    private X509Certificate2 _certificate;

    public OpenShiftCertificateLoader(IOptions<OpenShiftIntegrationOptions> options)
    {
        _options = options;
    }

    public X509Certificate2 ServiceCertificate
    {
        get
        {
            if (_certificate == null)
                if (_options.Value.UseHttps)
                {
                    var certificateMountPoint = _options.Value.CertificateMountPoint;
                    var certificateFile = Path.Combine(certificateMountPoint, "tls.crt");
                    var keyFile = Path.Combine(certificateMountPoint, "tls.key");
                    _certificate = CertificateLoader.LoadCertificateWithKey(certificateFile, keyFile);
                }

            return _certificate;
        }
    }
}

public static class OpenShiftWebHostBuilderExtensions
{
    public static IWebHostBuilder UseOpenShiftIntegration(this IWebHostBuilder builder, Action<OpenShiftIntegrationOptions> configureOptions)
    {
        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));

        if (PlatformEnvironment.IsOpenShift)
        {
            // Clear the urls. We'll explicitly configure Kestrel depending on the options.
            builder.UseUrls();

            builder.ConfigureServices(services =>
            {
                services.Configure(configureOptions);
                services.AddSingleton<OpenShiftCertificateLoader>();
                services.AddSingleton<IConfigureOptions<KestrelServerOptions>, KestrelOptionsSetup>();
                services.AddSingleton<IHostedService, OpenShiftCertificateExpiration>();
            });
        }

        return builder;
    }
}
