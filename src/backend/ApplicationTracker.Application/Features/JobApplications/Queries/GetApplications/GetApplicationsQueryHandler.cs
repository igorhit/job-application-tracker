using ApplicationTracker.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.JobApplications.Queries.GetApplications;

public class GetApplicationsQueryHandler : IRequestHandler<GetApplicationsQuery, Result<IReadOnlyList<JobApplicationDto>>>
{
    private readonly IJobApplicationRepository _applications;

    public GetApplicationsQueryHandler(IJobApplicationRepository applications)
    {
        _applications = applications;
    }

    public async Task<Result<IReadOnlyList<JobApplicationDto>>> Handle(GetApplicationsQuery request, CancellationToken ct)
    {
        var applications = await _applications.GetFilteredAsync(
            request.UserId,
            request.Query,
            request.Status,
            request.CompanyId,
            request.SortBy,
            ct);

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
                a.Requirements
                    .OrderBy(r => r.DisplayOrder)
                    .Select(r => new ApplicationRequirementDto(r.Id, r.Content, r.DisplayOrder))
                    .ToList()
                    .AsReadOnly(),
                a.Notes.Count,
                a.CreatedAt))
            .ToList()
            .AsReadOnly();

        return Result.Ok<IReadOnlyList<JobApplicationDto>>(dtos);
    }
}
