using ApplicationTracker.Application.Features.JobApplications.Queries.GetApplications;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.JobApplications.Queries.SearchApplications;

public record SearchApplicationsQuery(Guid UserId, string Query) : IRequest<Result<IReadOnlyList<JobApplicationDto>>>;
