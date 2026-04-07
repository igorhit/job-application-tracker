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

        // Idempotente: não recria se já existir
        if (await db.Users.AnyAsync(u => u.Email == email))
        {
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
                DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(2), "Entrevista técnica com o time"),

            JobApplication.Create(user.Id, ml.Id, "Senior .NET Developer", ApplicationStatus.Applied,
                "https://mercadolivre.com.br/jobs", "São Paulo / Remoto", 18000,
                DateTime.UtcNow.AddDays(-5), null, null),

            JobApplication.Create(user.Id, ifood.Id, "Desenvolvedor Backend", ApplicationStatus.Challenge,
                null, "Remoto", 12000,
                DateTime.UtcNow.AddDays(-15), DateTime.UtcNow.AddDays(1), "Enviar desafio técnico"),

            JobApplication.Create(user.Id, totvs.Id, "Analista de Sistemas .NET", ApplicationStatus.Rejected,
                null, "São Paulo", 8000,
                DateTime.UtcNow.AddDays(-30), null, null),

            JobApplication.Create(user.Id, nubank.Id, "Platform Engineer", ApplicationStatus.Wishlist,
                "https://nubank.com.br/vagas/platform", "Remoto", null,
                DateTime.UtcNow.AddDays(-2), null, null),
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
}
