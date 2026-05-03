using System.Threading.RateLimiting;
using CclInventario.Api.Data;
using CclInventario.Api.Entities;
using CclInventario.Api.Options;
using CclInventario.Api.Patterns.Factory;
using CclInventario.Api.Patterns.Facade;
using CclInventario.Api.Patterns.Observer;
using CclInventario.Api.Patterns.Singleton;
using CclInventario.Api.Patterns.Strategies;
using CclInventario.Api.Security;
using CclInventario.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
    options.Limits.MaxRequestBodySize = 512 * 1024;
});

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<DemoAuthOptions>(builder.Configuration.GetSection(DemoAuthOptions.SectionName));

// --- GoF + DI: Singleton (vida única del proceso) ---
builder.Services.AddSingleton<OperationSequenceGenerator>();
builder.Services.AddSingleton<IJwtBearerConfigurationFactory, JwtBearerConfigurationFactory>();
builder.Services.AddSingleton<IJwtTokenFactory, JwtTokenFactory>();
builder.Services.AddSingleton<IDemoUserStore, DemoUserStore>();

// --- GoF Strategy + Factory ---
builder.Services.AddSingleton<IMovimientoStrategy, EntradaMovimientoStrategy>();
builder.Services.AddSingleton<IMovimientoStrategy, SalidaMovimientoStrategy>();
builder.Services.AddSingleton<IMovimientoStrategyFactory, MovimientoStrategyFactory>();

// --- GoF Observer ---
builder.Services.AddSingleton<IInventoryObserver, InventoryLoggingObserver>();
builder.Services.AddSingleton<IInventoryChangeNotifier, InventoryChangeNotifier>();

// --- GoF Facade (orquesta DbContext scoped) ---
builder.Services.AddScoped<IInventoryFacade, InventoryFacade>();

// --- JWT: esquema Bearer + parámetros de validación enlazados al nombre del esquema (evita "signature key was not found") ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IJwtBearerConfigurationFactory>((options, factory) =>
        options.TokenValidationParameters = factory.CreateValidationParameters());

builder.Services.AddAuthorization();

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(180);
    options.IncludeSubDomains = true;
    options.Preload = false;
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CCL Inventario API",
        Version = "v1",
        Description = "Prueba técnica — inventario con JWT, PostgreSQL y patrones GoF explícitos."
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT en encabezado. Ejemplo: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevAngular", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await context.HttpContext.Response.WriteAsJsonAsync(
                new { message = "Demasiados intentos de inicio de sesión. Espere un minuto e intente de nuevo." },
                cancellationToken)
            .ConfigureAwait(false);
    };

    options.AddPolicy("login", httpContext =>
    {
        var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 15,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });
});

var app = builder.Build();

// Diagnóstico: debe coincidir con Host/Puerto/Base de pgAdmin donde ve filas.
var connStr = app.Configuration.GetConnectionString("Default");
if (!string.IsNullOrWhiteSpace(connStr))
{
    try
    {
        var npg = new NpgsqlConnectionStringBuilder(connStr);
        app.Logger.LogInformation(
            "Inventario API → PostgreSQL Host={Host} Port={Port} Database={Database} User={Username}. " +
            "Si GET /productos/inventario devuelve [] pero pgAdmin muestra datos, ese pgAdmin debe apuntar a ESTE mismo servidor y puerto.",
            npg.Host, npg.Port, npg.Database, npg.Username);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Cadena ConnectionStrings:Default no válida para diagnóstico.");
    }
}
else
{
    app.Logger.LogWarning("No hay ConnectionStrings:Default configurada.");
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync().ConfigureAwait(false);
    // No borra datos: solo inserta si la tabla está vacía (como database/seed-manual.sql).
    if (!await db.Productos.AnyAsync().ConfigureAwait(false))
    {
        db.Productos.AddRange(
            new Producto { Nombre = "Resma papel A4", Cantidad = 120 },
            new Producto { Nombre = "Bolígrafo azul", Cantidad = 300 },
            new Producto { Nombre = "Tóner impresora", Cantidad = 15 });
        await db.SaveChangesAsync().ConfigureAwait(false);
        app.Logger.LogInformation("Seed automático aplicado: insertados los 3 productos demo.");
    }

    var filasVisiblesPorEf = await db.Productos.AsNoTracking().CountAsync().ConfigureAwait(false);
    app.Logger.LogInformation("EF ve {Count} fila(s) en public.productos con esta cadena de conexión.", filasVisiblesPorEf);
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                message = "Se produjo un error interno. No se exponen detalles por seguridad."
            }).ConfigureAwait(false);
        });
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseCors("DevAngular");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
