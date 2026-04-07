using ApplicationTracker.Domain.Entities;

namespace ApplicationTracker.Domain.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
