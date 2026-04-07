using ApplicationTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTracker.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<ApplicationNote> ApplicationNotes => Set<ApplicationNote>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).IsRequired().HasMaxLength(256);
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Company>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
            e.Property(c => c.Website).HasMaxLength(500);
            e.Property(c => c.Notes).HasMaxLength(2000);
            e.HasOne(c => c.User)
                .WithMany(u => u.Companies)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<JobApplication>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.JobTitle).IsRequired().HasMaxLength(200);
            e.Property(a => a.JobUrl).HasMaxLength(500);
            e.Property(a => a.Location).HasMaxLength(200);
            e.Property(a => a.SalaryExpectation).HasColumnType("decimal(18,2)");
            e.Property(a => a.NextActionNote).HasMaxLength(500);
            e.HasOne(a => a.User)
                .WithMany(u => u.JobApplications)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Company)
                .WithMany(c => c.JobApplications)
                .HasForeignKey(a => a.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ApplicationNote>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Content).IsRequired().HasMaxLength(5000);
            e.HasOne(n => n.JobApplication)
                .WithMany(a => a.Notes)
                .HasForeignKey(n => n.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.Token).IsUnique();
            e.Property(r => r.Token).IsRequired();
            e.HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
