using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(Guid UserId) : IRequest<Result>;
