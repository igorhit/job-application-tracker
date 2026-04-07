using ApplicationTracker.Domain.Entities;
using ApplicationTracker.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTracker.Infrastructure.Persistence.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly AppDbContext _db;

    public CompanyRepository(AppDbContext db) => _db = db;

    public Task<Company?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct) =>
        _db.Companies
            .Include(c => c.JobApplications)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

    public async Task<IReadOnlyList<Company>> GetAllByUserAsync(Guid userId, CancellationToken ct)
    {
        var result = await _db.Companies
            .Include(c => c.JobApplications)
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
        return result.AsReadOnly();
    }

    public async Task AddAsync(Company company, CancellationToken ct) =>
        await _db.Companies.AddAsync(company, ct);

    public void Update(Company company) => _db.Companies.Update(company);

    public void Delete(Company company) => _db.Companies.Remove(company);

    public Task SaveChangesAsync(CancellationToken ct) =>
        _db.SaveChangesAsync(ct);
}
