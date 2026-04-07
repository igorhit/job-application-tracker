using ApplicationTracker.Domain.Entities;

namespace ApplicationTracker.Domain.Interfaces;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Company>> GetAllByUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Company company, CancellationToken ct = default);
    void Update(Company company);
    void Delete(Company company);
    Task SaveChangesAsync(CancellationToken ct = default);
}
