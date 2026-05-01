using CclInventario.Api.Data;
using CclInventario.Api.Options;
using CclInventario.Api.Patterns.Factory;
using CclInventario.Api.Patterns.Facade;
using CclInventario.Api.Patterns.Observer;
using CclInventario.Api.Patterns.Singleton;
using CclInventario.Api.Patterns.Strategies;
using CclInventario.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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

// --- JWT: opciones nombradas vía Factory ---
builder.Services.AddSingleton<IConfigureNamedOptions<JwtBearerOptions>, JwtBearerOptionsConfigurer>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddAuthorization();

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("DevAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
