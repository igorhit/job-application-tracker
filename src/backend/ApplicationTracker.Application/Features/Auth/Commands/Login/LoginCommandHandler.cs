using ApplicationTracker.Domain.Entities;
using ApplicationTracker.Domain.Errors;
using ApplicationTracker.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtService _jwt;

    public LoginCommandHandler(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IPasswordHasher hasher,
        IJwtService jwt)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(request.Email, ct);
        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            return Result.Fail<LoginResponse>(DomainErrors.Auth.InvalidCredentials);

        var accessToken = _jwt.GenerateAccessToken(user);
        var rawRefreshToken = _jwt.GenerateRefreshToken();
        var refreshToken = RefreshToken.Create(user.Id, rawRefreshToken, DateTime.UtcNow.AddDays(7));

        await _refreshTokens.AddAsync(refreshToken, ct);
        await _refreshTokens.SaveChangesAsync(ct);

        return Result.Ok(new LoginResponse(accessToken, rawRefreshToken, user.Email, user.Name));
    }
}
