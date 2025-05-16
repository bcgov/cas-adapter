SerilogSplunkLoggingExtensions.CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
builder.WebHost
    .UseUrls()
    .UseOpenShiftIntegration(_ => _.CertificateMountPoint = "/ssl")  
    .UseKestrel();

var services = builder.Services;
var env = builder.Environment;

var appSettings = services.AddAppSettings(env);

services.AddCasHttpClient(env.IsProduction());
services.AddCorsPolicy(builder.Configuration.GetSection("cors").Get<CorsSettings>());
services.AddSsoAuthentication(appSettings.Configuration);
services.AddSsoAuthorization();
services.AddHealthChecks();
services.AddLogging(appSettings);
services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();
app.MapHealthChecks();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers()
    .RequireAuthorization();
if (app.Environment.IsDevelopment()) 
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseDisableHttpVerbsMiddleware(app.Configuration.GetValue("DisabledHttpVerbs", string.Empty));
app.UseCsp();
app.UseSecurityHeaders();
app.UseCors();
app.UseLoggingMiddleware();

app.Run();
