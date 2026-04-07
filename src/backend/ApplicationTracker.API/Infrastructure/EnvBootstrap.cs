using System.Security.Cryptography;

namespace ApplicationTracker.API.Infrastructure;

public static class EnvBootstrap
{
    public static void EnsureEnvFileExists(string appRoot)
    {
        var envPath = Path.Combine(appRoot, ".env");
        if (File.Exists(envPath)) return;

        var jwtSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));

        var content = $"""
            Jwt__Secret={jwtSecret}
            """;

        File.WriteAllText(envPath, content);
    }

    public static void LoadEnvFile(string appRoot)
    {
        var envPath = Path.Combine(appRoot, ".env");
        if (!File.Exists(envPath)) return;

        foreach (var line in File.ReadAllLines(envPath))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#')) continue;

            var idx = trimmed.IndexOf('=');
            if (idx < 0) continue;

            var key = trimmed[..idx].Trim();
            var value = trimmed[(idx + 1)..].Trim();

            if (Environment.GetEnvironmentVariable(key) is null)
                Environment.SetEnvironmentVariable(key, value);
        }
    }
}
