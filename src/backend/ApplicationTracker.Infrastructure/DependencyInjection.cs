using ApplicationTracker.Domain.Interfaces;
using ApplicationTracker.Infrastructure.Persistence;
using ApplicationTracker.Infrastructure.Persistence.Repositories;
using ApplicationTracker.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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
