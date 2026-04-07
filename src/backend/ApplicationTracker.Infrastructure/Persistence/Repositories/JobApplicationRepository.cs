using ApplicationTracker.Domain.Entities;
using ApplicationTracker.Domain.Enums;
using ApplicationTracker.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTracker.Infrastructure.Persistence.Repositories;

public class JobApplicationRepository : IJobApplicationRepository
{
    private readonly AppDbContext _db;

    public JobApplicationRepository(AppDbContext db) => _db = db;

    public Task<JobApplication?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct) =>
        _db.JobApplications
            .Include(a => a.Company)
            .Include(a => a.Notes)
            .Include(a => a.Requirements)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);

    public async Task<IReadOnlyList<JobApplication>> GetFilteredAsync(
        Guid userId,
        string? query,
        ApplicationStatus? status,
        Guid? companyId,
        JobApplicationSortBy sortBy,
        CancellationToken ct)
    {
        var applications = _db.JobApplications
            .Include(a => a.Company)
            .Include(a => a.Notes)
            .Include(a => a.Requirements)
            .Where(a => a.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lower = query.Trim().ToLowerInvariant();
            applications = applications.Where(a =>
                a.JobTitle.ToLower().Contains(lower) ||
                a.Company.Name.ToLower().Contains(lower) ||
                (a.Location != null && a.Location.ToLower().Contains(lower)));
        }

        if (status.HasValue)
            applications = applications.Where(a => a.Status == status.Value);

        if (companyId.HasValue)
            applications = applications.Where(a => a.CompanyId == companyId.Value);

        applications = sortBy switch
        {
            JobApplicationSortBy.AppliedAtAsc => applications.OrderBy(a => a.AppliedAt),
            JobApplicationSortBy.NextActionAsc => applications
                .OrderBy(a => a.NextActionAt == null)
                .ThenBy(a => a.NextActionAt)
                .ThenByDescending(a => a.AppliedAt),
            JobApplicationSortBy.CompanyAsc => applications
                .OrderBy(a => a.Company.Name)
                .ThenByDescending(a => a.AppliedAt),
            JobApplicationSortBy.StatusAsc => applications
                .OrderBy(a => a.Status)
                .ThenByDescending(a => a.AppliedAt),
            JobApplicationSortBy.CreatedAtDesc => applications.OrderByDescending(a => a.CreatedAt),
            _ => applications.OrderByDescending(a => a.AppliedAt)
        };

        var result = await applications.ToListAsync(ct);
        return result.AsReadOnly();
    }

    public async Task AddAsync(JobApplication application, CancellationToken ct) =>
        await _db.JobApplications.AddAsync(application, ct);

    public void Update(JobApplication application) => _db.JobApplications.Update(application);

    public void ReplaceRequirements(JobApplication application, IEnumerable<string>? requirements)
    {
        var existingRequirements = _db.ApplicationRequirements
            .Where(requirement => requirement.JobApplicationId == application.Id)
            .ToList();

        if (existingRequirements.Count > 0)
            _db.ApplicationRequirements.RemoveRange(existingRequirements);

        if (requirements is null)
            return;

        var normalizedRequirements = requirements
            .Where(requirement => !string.IsNullOrWhiteSpace(requirement))
            .Select(requirement => requirement.Trim())
            .ToList();

        for (var i = 0; i < normalizedRequirements.Count; i++)
        {
            _db.ApplicationRequirements.Add(
                ApplicationRequirement.Create(application.Id, normalizedRequirements[i], i));
        }
    }

    public void Delete(JobApplication application) => _db.JobApplications.Remove(application);

    public Task SaveChangesAsync(CancellationToken ct) =>
        _db.SaveChangesAsync(ct);
}
