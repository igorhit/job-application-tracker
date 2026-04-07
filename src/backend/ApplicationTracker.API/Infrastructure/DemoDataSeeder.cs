using ApplicationTracker.Domain.Entities;
using ApplicationTracker.Domain.Enums;
using ApplicationTracker.Domain.Interfaces;
using ApplicationTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ApplicationTracker.API.Infrastructure;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var config = services.GetRequiredService<IConfiguration>();

        if (!bool.TryParse(config["DemoData:Enabled"], out var enabled) || !enabled)
            return;

        var email = config["DemoData:UserEmail"] ?? "demo@tracker.dev";
        var password = config["DemoData:UserPassword"] ?? "Demo1234!";

        var db = services.GetRequiredService<AppDbContext>();
        var hasher = services.GetRequiredService<IPasswordHasher>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        await SeedE2EUserIfConfiguredAsync(config, db, hasher, logger);

        // Idempotente: não recria se já existir
        if (await db.Users.AnyAsync(u => u.Email == email))
        {
            await BackfillDemoRequirementsAsync(db, logger);
            logger.LogInformation("Seed demo já existente, ignorando");
            return;
        }

        logger.LogInformation("Criando dados demo...");

        var user = User.Create(email, hasher.Hash(password), "Demo User");
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        var companies = new[]
        {
            Company.Create(user.Id, "Nubank", "https://nubank.com.br", "Fintech brasileira, foco em produto"),
            Company.Create(user.Id, "Mercado Livre", "https://mercadolivre.com.br", "E-commerce, times grandes"),
            Company.Create(user.Id, "iFood", "https://ifood.com.br", null),
            Company.Create(user.Id, "Totvs", "https://totvs.com", "ERP, Java/C#"),
        };

        await db.Companies.AddRangeAsync(companies);
        await db.SaveChangesAsync();

        var nubank = companies[0];
        var ml = companies[1];
        var ifood = companies[2];
        var totvs = companies[3];

        var applications = new[]
        {
            JobApplication.Create(user.Id, nubank.Id, "Backend Engineer", ApplicationStatus.Interview,
                "https://nubank.com.br/vagas/backend", "Remoto", 15000,
                DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(2), "Entrevista técnica com o time",
                ["ASP.NET Core", "Distributed systems", "Cloud (AWS ou GCP)", "Design de APIs"]),

            JobApplication.Create(user.Id, ml.Id, "Senior .NET Developer", ApplicationStatus.Applied,
                "https://mercadolivre.com.br/jobs", "São Paulo / Remoto", 18000,
                DateTime.UtcNow.AddDays(-5), null, null,
                ["C#", ".NET", "Mensageria", "Microservices"]),

            JobApplication.Create(user.Id, ifood.Id, "Desenvolvedor Backend", ApplicationStatus.Challenge,
                null, "Remoto", 12000,
                DateTime.UtcNow.AddDays(-15), DateTime.UtcNow.AddDays(1), "Enviar desafio técnico",
                ["REST APIs", "SQL", "Testes automatizados"]),

            JobApplication.Create(user.Id, totvs.Id, "Analista de Sistemas .NET", ApplicationStatus.Rejected,
                null, "São Paulo", 8000,
                DateTime.UtcNow.AddDays(-30), null, null,
                ["C#", ".NET Framework", "SQL Server"]),

            JobApplication.Create(user.Id, nubank.Id, "Platform Engineer", ApplicationStatus.Wishlist,
                "https://nubank.com.br/vagas/platform", "Remoto", null,
                DateTime.UtcNow.AddDays(-2), null, null,
                ["Kubernetes", "Observability", "Infrastructure as Code"]),
        };

        await db.JobApplications.AddRangeAsync(applications);
        await db.SaveChangesAsync();

        var note1 = ApplicationNote.Create(applications[0].Id, user.Id,
            "Primeira entrevista com RH foi bem. Esperando contato do tech lead.");
        var note2 = ApplicationNote.Create(applications[0].Id, user.Id,
            "Pesquisar sobre a stack deles: Clojure no backend, Kotlin mobile.");
        var note3 = ApplicationNote.Create(applications[2].Id, user.Id,
            "Desafio: construir uma API REST em .NET. Prazo de 72h.");

        await db.ApplicationNotes.AddRangeAsync(note1, note2, note3);
        await db.SaveChangesAsync();

        logger.LogInformation("Seed demo concluído. Login: {Email} / {Password}", email, password);
    }

    private static async Task BackfillDemoRequirementsAsync(AppDbContext db, ILogger logger)
    {
        var seededRequirementsByTitle = new Dictionary<string, string[]>
        {
            ["Backend Engineer"] = ["ASP.NET Core", "Distributed systems", "Cloud (AWS ou GCP)", "Design de APIs"],
            ["Senior .NET Developer"] = ["C#", ".NET", "Mensageria", "Microservices"],
            ["Desenvolvedor Backend"] = ["REST APIs", "SQL", "Testes automatizados"],
            ["Analista de Sistemas .NET"] = ["C#", ".NET Framework", "SQL Server"],
            ["Platform Engineer"] = ["Kubernetes", "Observability", "Infrastructure as Code"],
        };

        var applications = await db.JobApplications
            .Include(application => application.Requirements)
            .Where(application => seededRequirementsByTitle.Keys.Contains(application.JobTitle))
            .ToListAsync();

        var updatedCount = 0;

        foreach (var application in applications.Where(application => application.Requirements.Count == 0))
        {
            var requirements = seededRequirementsByTitle[application.JobTitle];

            for (var i = 0; i < requirements.Length; i++)
            {
                await db.ApplicationRequirements.AddAsync(
                    ApplicationRequirement.Create(application.Id, requirements[i], i));
            }

            updatedCount++;
        }

        if (updatedCount == 0)
            return;

        await db.SaveChangesAsync();
        logger.LogInformation("Backfill de requisitos demo aplicado em {Count} candidaturas.", updatedCount);
    }

    private static async Task SeedE2EUserIfConfiguredAsync(
        IConfiguration config,
        AppDbContext db,
        IPasswordHasher hasher,
        ILogger logger)
    {
        if (!bool.TryParse(config["E2EData:Enabled"], out var enabled) || !enabled)
            return;

        var email = config["E2EData:UserEmail"] ?? "e2e@tracker.dev";
        var password = config["E2EData:UserPassword"] ?? "Password123!";
        var name = config["E2EData:UserName"] ?? "E2E Test User";

        if (await db.Users.AnyAsync(u => u.Email == email))
        {
            logger.LogInformation("Seed E2E já existente, ignorando");
            return;
        }

        logger.LogInformation("Criando usuário isolado para testes E2E...");

        var user = User.Create(email, hasher.Hash(password), name);
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();
    }
}
