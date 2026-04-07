using ApplicationTracker.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IRefreshTokenRepository _refreshTokens;

    public LogoutCommandHandler(IRefreshTokenRepository refreshTokens)
    {
        _refreshTokens = refreshTokens;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        await _refreshTokens.RevokeAllByUserAsync(request.UserId, ct);
        await _refreshTokens.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
