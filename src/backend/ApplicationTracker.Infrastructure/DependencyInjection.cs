using ApplicationTracker.Application.Abstractions.Ai;
using ApplicationTracker.Domain.Interfaces;
using ApplicationTracker.Infrastructure.Ai;
using ApplicationTracker.Infrastructure.Persistence;
using ApplicationTracker.Infrastructure.Persistence.Repositories;
using ApplicationTracker.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Text;

namespace ApplicationTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection") ?? "Data Source=applicationtracker.db",
                b => b.MigrationsAssembly("ApplicationTracker.Infrastructure")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
        services.AddScoped<IApplicationNoteRepository, ApplicationNoteRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.Configure<AiOptions>(configuration.GetSection("Ai"));
        services.AddHttpClient<IAiTextGenerationService, OpenAiTextGenerationService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<AiOptions>>().Value;
            var baseUrl = string.IsNullOrWhiteSpace(options.BaseUrl)
                ? "https://api.openai.com/v1/"
                : options.BaseUrl;

            client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds > 0 ? options.TimeoutSeconds : 45);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrWhiteSpace(options.ApiKey))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        // Read Jwt:Secret lazily so test overrides from ConfigureAppConfiguration are picked up
        services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            return new PostConfigureOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var secret = config["Jwt:Secret"]
                    ?? throw new InvalidOperationException("Jwt:Secret not configured");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                };
            });
        });

        services.AddAuthorization();

        return services;
    }
}
