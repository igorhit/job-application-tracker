using ApplicationTracker.Application.Features.JobApplications.Queries.GetApplications;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.JobApplications.Queries.GetApplicationById;

public record GetApplicationByIdQuery(Guid Id, Guid UserId) : IRequest<Result<JobApplicationDto>>;
