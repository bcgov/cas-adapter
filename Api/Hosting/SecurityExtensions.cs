﻿namespace Api;

public static class SecurityExtensions
{
    public static void UseCsp(this WebApplication webApplication)
    {
        var settings = webApplication.Services.GetRequiredService<IOptions<CspSettings>>();
        UseCsp(webApplication, settings.Value);
    }

    public static void UseCsp(this WebApplication webApplication, CspSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var cspString = new StringValues(settings.ToCsppString());
        webApplication.Use(async (context, next) =>
        {
            context.Response.Headers.Append("Content-Security-Policy", cspString);
            await next();
        });
    }

    public static void AddCorsPolicy(this IServiceCollection services, CorsSettings? settings)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.SetIsOriginAllowedToAllowWildcardSubdomains();
                if (settings != null && settings.AllowedOrigins != null) policy.WithOrigins(settings.ToOriginsArray()!);
            });
        });
    }

    public static void UseSecurityHeaders(this WebApplication webApplication)
    {
        webApplication.Use(async (context, next) =>
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Xss-Protection", "1; mode=block");
            context.Response.Headers.Append("X-Frame-Protection", "DENY");
            await next();
        });
    }

    public static void UseDisableHttpVerbsMiddleware(this WebApplication webApplication, string? verbString)
    {
        if (verbString == null) return;
        var verbList = verbString.Split(',', ';', ' ').Select(v => v.Trim().ToUpperInvariant()).ToArray();
        webApplication.Use(async (context, next) =>
        {
            if (verbList.Contains(context.Request.Method.ToUpperInvariant()))
            {
                context.Response.StatusCode = 405;
                return;
            }
            await next.Invoke();
        });
    }
}

public record CspSettings : IOptions<CspSettings>
{
    public string BaseUri { get; set; } = string.Empty;
    public string DefaultSource { get; set; } = string.Empty;
    public string ScriptSource { get; set; } = string.Empty;
    public string ConnectSource { get; set; } = string.Empty;
    public string ImageSource { get; set; } = string.Empty;
    public string StyleSource { get; set; } = string.Empty;
    public string FontSource { get; set; } = string.Empty;
    public string FrameAncestors { get; set; } = string.Empty;
    public string FormAction { get; set; } = string.Empty;
    public string FrameSource { get; set; } = string.Empty;

    public CspSettings Value => this;

    public string ToCsppString() =>
      (string.IsNullOrWhiteSpace(BaseUri) ? "" : $"base-uri {BaseUri};") +
      (string.IsNullOrWhiteSpace(DefaultSource) ? "" : $"default-src {DefaultSource};") +
      (string.IsNullOrWhiteSpace(ScriptSource) ? "" : $"script-src {ScriptSource};") +
      (string.IsNullOrWhiteSpace(ConnectSource) ? "" : $"connect-src {ConnectSource};") +
      (string.IsNullOrWhiteSpace(ImageSource) ? "" : $"img-src {ImageSource};") +
      (string.IsNullOrWhiteSpace(StyleSource) ? "" : $"style-src {StyleSource};") +
      (string.IsNullOrWhiteSpace(FontSource) ? "" : $"font-src {this.FontSource};") +
      (string.IsNullOrWhiteSpace(FrameAncestors) ? "" : $"frame-ancestors {this.FrameAncestors};") +
      (string.IsNullOrWhiteSpace(FormAction) ? "" : $"form-action {this.FormAction};") +
      (string.IsNullOrWhiteSpace(FrameSource) ? "" : $"frame-src {this.FrameSource};")
    ;
}

public record CorsSettings
{
    public string? AllowedOrigins { get; set; }

    public string[]? ToOriginsArray() => AllowedOrigins?.Split(',', ';', ' ').Select(v => v.Trim()).ToArray() ?? Array.Empty<string>();
}
