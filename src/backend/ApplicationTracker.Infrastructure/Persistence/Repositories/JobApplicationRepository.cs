using ApplicationTracker.Domain.Entities;
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
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);

    public async Task<IReadOnlyList<JobApplication>> GetAllByUserAsync(Guid userId, CancellationToken ct)
    {
        var result = await _db.JobApplications
            .Include(a => a.Company)
            .Include(a => a.Notes)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync(ct);
        return result.AsReadOnly();
    }

    public async Task<IReadOnlyList<JobApplication>> SearchAsync(Guid userId, string query, CancellationToken ct)
    {
        var lower = query.ToLowerInvariant();
        var result = await _db.JobApplications
            .Include(a => a.Company)
            .Include(a => a.Notes)
            .Where(a => a.UserId == userId &&
                (a.JobTitle.ToLower().Contains(lower) ||
                 a.Company.Name.ToLower().Contains(lower) ||
                 (a.Location != null && a.Location.ToLower().Contains(lower))))
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync(ct);
        return result.AsReadOnly();
    }

    public async Task AddAsync(JobApplication application, CancellationToken ct) =>
        await _db.JobApplications.AddAsync(application, ct);

    public void Update(JobApplication application) => _db.JobApplications.Update(application);

    public void Delete(JobApplication application) => _db.JobApplications.Remove(application);

    public Task SaveChangesAsync(CancellationToken ct) =>
        _db.SaveChangesAsync(ct);
}
