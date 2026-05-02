namespace CclInventario.Api.Security;

/// <summary>
/// Cabeceras HTTP alineadas con buenas prácticas OWASP (A05 configuración segura, mitigación XSS/clickjacking en cliente que consume API).
/// CSP estricta solo cuando no es documentación Swagger en desarrollo (Swagger UI requiere scripts inline).
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;

    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers.Append("X-Content-Type-Options", "nosniff");
        headers.Append("X-Frame-Options", "SAMEORIGIN");
        headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        headers.Append(
            "Permissions-Policy",
            "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");

        var path = context.Request.Path.Value ?? string.Empty;
        var isSwaggerDoc = path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase);
        if (!(_env.IsDevelopment() && isSwaggerDoc))
        {
            headers.Append(
                "Content-Security-Policy",
                "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'");
        }

        await _next(context).ConfigureAwait(false);
    }
}
