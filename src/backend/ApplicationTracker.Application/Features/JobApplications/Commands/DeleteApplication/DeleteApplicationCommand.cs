using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.JobApplications.Commands.DeleteApplication;

public record DeleteApplicationCommand(Guid Id, Guid UserId) : IRequest<Result>;
