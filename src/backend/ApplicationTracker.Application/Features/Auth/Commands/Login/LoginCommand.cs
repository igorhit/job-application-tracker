using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;

public record LoginResponse(string AccessToken, string RefreshToken, string Email, string Name);
