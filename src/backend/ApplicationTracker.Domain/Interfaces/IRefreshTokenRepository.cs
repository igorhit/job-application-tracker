using ApplicationTracker.Domain.Entities;

namespace ApplicationTracker.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    Task RevokeAllByUserAsync(Guid userId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
