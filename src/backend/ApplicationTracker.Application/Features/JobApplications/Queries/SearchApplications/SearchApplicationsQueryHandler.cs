using ApplicationTracker.Application.Features.JobApplications.Queries.GetApplications;
using ApplicationTracker.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.JobApplications.Queries.SearchApplications;

public class SearchApplicationsQueryHandler : IRequestHandler<SearchApplicationsQuery, Result<IReadOnlyList<JobApplicationDto>>>
{
    private readonly IJobApplicationRepository _applications;

    public SearchApplicationsQueryHandler(IJobApplicationRepository applications)
    {
        _applications = applications;
    }

    public async Task<Result<IReadOnlyList<JobApplicationDto>>> Handle(SearchApplicationsQuery request, CancellationToken ct)
    {
        var applications = await _applications.SearchAsync(request.UserId, request.Query, ct);

        var dtos = applications
            .Select(a => new JobApplicationDto(
                a.Id,
                a.CompanyId,
                a.Company.Name,
                a.JobTitle,
                a.Status,
                a.Status.ToString(),
                a.JobUrl,
                a.Location,
                a.SalaryExpectation,
                a.AppliedAt,
                a.NextActionAt,
                a.NextActionNote,
                a.Notes.Count,
                a.CreatedAt))
            .ToList()
            .AsReadOnly();

        return Result.Ok<IReadOnlyList<JobApplicationDto>>(dtos);
    }
}
