using ApplicationTracker.API.Infrastructure;
using ApplicationTracker.Application;
using ApplicationTracker.Infrastructure;
using ApplicationTracker.Infrastructure.Persistence;
using AspNetCoreRateLimit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var isTesting = builder.Environment.EnvironmentName == "Testing";
var appRoot = AppContext.BaseDirectory;

if (!isTesting)
{
    EnvBootstrap.EnsureEnvFileExists(appRoot);
    EnvBootstrap.LoadEnvFile(appRoot);
    builder.Configuration.AddEnvironmentVariables();
}

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();

if (!isTesting)
{
    builder.Services.AddMemoryCache();
    builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
    builder.Services.AddInMemoryRateLimiting();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Application Tracker API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

if (!isTesting)
    builder.WebHost.UseUrls("http://0.0.0.0:5001");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (isTesting)
        await db.Database.EnsureCreatedAsync();
    else
    {
        await db.Database.MigrateAsync();
        await DemoDataSeeder.SeedAsync(scope.ServiceProvider);
    }
}

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";

    var path = context.Request.Path.Value ?? "";
    if (path == "/" || path == "/index.html")
        context.Response.Headers["Content-Security-Policy"] =
            "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval' blob:; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self' data:; worker-src blob:";
    else
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";

    await next();
});

if (!isTesting)
    app.UseIpRateLimiting();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Application Tracker API v1");
    c.RoutePrefix = string.Empty;
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();

public partial class Program { }
