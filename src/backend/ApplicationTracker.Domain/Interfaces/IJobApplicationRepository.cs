using ApplicationTracker.Domain.Entities;
using ApplicationTracker.Domain.Enums;

namespace ApplicationTracker.Domain.Interfaces;

public interface IJobApplicationRepository
{
    Task<JobApplication?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<JobApplication>> GetFilteredAsync(
        Guid userId,
        string? query,
        ApplicationStatus? status,
        Guid? companyId,
        JobApplicationSortBy sortBy,
        CancellationToken ct = default);
    Task AddAsync(JobApplication application, CancellationToken ct = default);
    void Update(JobApplication application);
    void ReplaceRequirements(JobApplication application, IEnumerable<string>? requirements);
    void Delete(JobApplication application);
    Task SaveChangesAsync(CancellationToken ct = default);
}
