using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password, string Name) : IRequest<Result<RegisterResponse>>;

public record RegisterResponse(Guid UserId, string Email, string Name);
