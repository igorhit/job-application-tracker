using ApplicationTracker.Domain.Entities;
using ApplicationTracker.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTracker.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct) =>
        _db.Users.AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task AddAsync(User user, CancellationToken ct) =>
        await _db.Users.AddAsync(user, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        _db.SaveChangesAsync(ct);
}
