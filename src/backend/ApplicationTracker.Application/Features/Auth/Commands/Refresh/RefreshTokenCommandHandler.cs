using ApplicationTracker.Domain.Entities;
using ApplicationTracker.Domain.Errors;
using ApplicationTracker.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Auth.Commands.Refresh;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IJwtService _jwt;

    public RefreshTokenCommandHandler(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IJwtService jwt)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _jwt = jwt;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var existing = await _refreshTokens.GetByTokenAsync(request.RefreshToken, ct);
        if (existing is null || !existing.IsValid())
            return Result.Fail<RefreshTokenResponse>(DomainErrors.Auth.InvalidRefreshToken);

        var user = await _users.GetByIdAsync(existing.UserId, ct);
        if (user is null)
            return Result.Fail<RefreshTokenResponse>(DomainErrors.Auth.InvalidRefreshToken);

        existing.Revoke();

        var newAccessToken = _jwt.GenerateAccessToken(user);
        var newRawToken = _jwt.GenerateRefreshToken();
        var newRefreshToken = RefreshToken.Create(user.Id, newRawToken, DateTime.UtcNow.AddDays(7));

        await _refreshTokens.AddAsync(newRefreshToken, ct);
        await _refreshTokens.SaveChangesAsync(ct);

        return Result.Ok(new RefreshTokenResponse(newAccessToken, newRawToken));
    }
}
