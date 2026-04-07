using ApplicationTracker.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationTracker.IntegrationTests;

public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-secret-key-that-is-long-enough-for-hmac-sha256",
                ["Jwt:Issuer"] = "ApplicationTrackerAPI",
                ["Jwt:Audience"] = "ApplicationTrackerClients",
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_dbPath}"
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={_dbPath}"));
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        TryDelete(_dbPath);
        TryDelete(_dbPath + "-shm");
        TryDelete(_dbPath + "-wal");
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch { /* best effort */ }
    }
}
