using ApplicationTracker.Domain.Entities;

namespace ApplicationTracker.Domain.Interfaces;

public interface IJobApplicationRepository
{
    Task<JobApplication?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<JobApplication>> GetAllByUserAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<JobApplication>> SearchAsync(Guid userId, string query, CancellationToken ct = default);
    Task AddAsync(JobApplication application, CancellationToken ct = default);
    void Update(JobApplication application);
    void Delete(JobApplication application);
    Task SaveChangesAsync(CancellationToken ct = default);
}
