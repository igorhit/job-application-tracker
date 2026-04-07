using ApplicationTracker.Domain.Entities;
using ApplicationTracker.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTracker.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _db;

    public RefreshTokenRepository(AppDbContext db) => _db = db;

    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct) =>
        _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct) =>
        await _db.RefreshTokens.AddAsync(token, ct);

    public async Task RevokeAllByUserAsync(Guid userId, CancellationToken ct)
    {
        var tokens = await _db.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in tokens)
            token.Revoke();
    }

    public Task SaveChangesAsync(CancellationToken ct) =>
        _db.SaveChangesAsync(ct);
}
