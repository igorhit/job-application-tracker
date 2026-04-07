using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Auth.Commands.Refresh;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<RefreshTokenResponse>>;

public record RefreshTokenResponse(string AccessToken, string RefreshToken);
